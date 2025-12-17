using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace Azure_SearchProject.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _container;
    private readonly StorageSharedKeyCredential _cred;

    public BlobStorageService(IConfiguration cfg)
    {
        var cs = cfg["AzureBlob:ConnectionString"]!;
        var containerName = cfg["AzureBlob:ContainerName"]!;

        _container = new BlobContainerClient(cs, containerName);

        var (accountName, accountKey) = ParseStorageConnectionString(cs);
        _cred = new StorageSharedKeyCredential(accountName, accountKey);
    }
    private static (string AccountName, string AccountKey) ParseStorageConnectionString(string cs)
    {
        var dict = cs.Split(';', StringSplitOptions.RemoveEmptyEntries)
                     .Select(p => p.Split('=', 2))
                     .Where(p => p.Length == 2)
                     .ToDictionary(p => p[0].Trim(), p => p[1].Trim(), StringComparer.OrdinalIgnoreCase);

        if (!dict.TryGetValue("AccountName", out var name) || string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("ConnectionString missing AccountName");

        if (!dict.TryGetValue("AccountKey", out var key) || string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("ConnectionString missing AccountKey");

        return (name, key);
    }

    public async Task<string> UploadImageAsync(
        Stream content,
        string contentType,
        string fileName)
    {
        await _container.CreateIfNotExistsAsync();

        var safeName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var blob = _container.GetBlobClient(safeName);

        await blob.UploadAsync(content, new BlobHttpHeaders
        {
            ContentType = contentType
        });

        return blob.Uri.ToString();
    }

    public string GetReadSasUrl(string blobUrl, int minutes = 60)
    {
        var blobUri = new Uri(blobUrl);

        var blobName = Path.GetFileName(blobUri.LocalPath);
        var blob = _container.GetBlobClient(blobName);

        var sas = new BlobSasBuilder
        {
            BlobContainerName = _container.Name,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(minutes)
        };

        sas.SetPermissions(BlobSasPermissions.Read);

        return blob.GenerateSasUri(sas).ToString();
    }

}

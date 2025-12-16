using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Azure_SearchProject.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _container;

    public BlobStorageService(IConfiguration cfg)
    {
        var cs = cfg["AzureBlob:ConnectionString"]!;
        var containerName = cfg["AzureBlob:ContainerName"]!;
        _container = new BlobContainerClient(cs, containerName);
    }

    public async Task<string> UploadImageAsync(Stream content, string contentType, string fileName)
    {
        await _container.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var safeName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var blob = _container.GetBlobClient(safeName);

        await blob.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType });
        return blob.Uri.ToString();
    }
}

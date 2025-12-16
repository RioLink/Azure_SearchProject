namespace Azure_SearchProject.Services;

public interface IBlobStorageService
{
    Task<string> UploadImageAsync(Stream content, string contentType, string fileName);
}

using Azure_SearchProject.Models;

namespace Azure_SearchProject.Services;

public interface ISearchService
{
    Task<List<ImageItem>> SearchAsync(string? query, string? userId);
}

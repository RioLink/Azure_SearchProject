using Azure_SearchProject.Models;

namespace Azure_SearchProject.Services;

public interface ISearchService
{
    Task<List<ImageItem>> SearchAsync(string? query, string? userId);

    Task<List<ImageItem>> GetTrendingAsync(int take = 8);

    Task<List<ImageItem>> GetRecommendedAsync(string userId, int take = 8);

    Task<List<Tag>> GetPopularTagsAsync(int take = 12);
}

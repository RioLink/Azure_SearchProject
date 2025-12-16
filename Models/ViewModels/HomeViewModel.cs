using Azure_SearchProject.Models;

namespace Azure_SearchProject.Models.ViewModels;

public class HomeViewModel
{
    public string? Query { get; set; }

    public List<ImageItem> Recommended { get; set; } = [];
    public List<ImageItem> Trending { get; set; } = [];
    public List<Tag> PopularTags { get; set; } = [];
}

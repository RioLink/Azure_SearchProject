using Azure_SearchProject.Models;
using System.ComponentModel.DataAnnotations;

namespace Azure_SearchProject.Models;

public class ImageItem
{
    public int Id { get; set; }
    public string OwnerId { get; set; } = "";
    public string BlobUrl { get; set; } = "";
    public string? Title { get; set; }
    public string? Description { get; set; }

    public bool IsPublic { get; set; }
    public string SafetyStatus { get; set; } = "Pending";

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ImageTag> ImageTags { get; set; } = new();
    public List<ImageLike> Likes { get; set; } = new();
}

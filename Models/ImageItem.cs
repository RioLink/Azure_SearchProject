using Azure_SearchProject.Models;
using System.ComponentModel.DataAnnotations;

namespace Azure_SearchProject.Models;

public class ImageItem
{
    public int Id { get; set; }

    [Required]
    public string OwnerId { get; set; } = default!;

    [Required, MaxLength(300)]
    public string BlobUrl { get; set; } = default!;

    [MaxLength(120)]
    public string? Title { get; set; }

    [MaxLength(800)]
    public string? Description { get; set; }

    public bool IsPublic { get; set; } = true;

    [MaxLength(40)]
    public string SafetyStatus { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ImageTag> ImageTags { get; set; } = new List<ImageTag>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<ImageLike> Likes { get; set; } = new List<ImageLike>();
}

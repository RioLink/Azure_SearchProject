namespace Azure_SearchProject.Models;

public class ImageLike
{
    public int ImageItemId { get; set; }
    public ImageItem ImageItem { get; set; } = default!;

    public string UserId { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

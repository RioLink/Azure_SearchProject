namespace Azure_SearchProject.Models;

public class ImageTag
{
    public int ImageItemId { get; set; }
    public ImageItem ImageItem { get; set; } = default!;

    public int TagId { get; set; }
    public Tag Tag { get; set; } = default!;
}

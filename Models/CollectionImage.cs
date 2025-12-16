namespace Azure_SearchProject.Models;

public class CollectionImage
{
    public int CollectionId { get; set; }
    public Collection Collection { get; set; } = default!;

    public int ImageItemId { get; set; }
    public ImageItem ImageItem { get; set; } = default!;
}

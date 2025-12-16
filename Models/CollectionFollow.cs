namespace Azure_SearchProject.Models;

public class CollectionFollow
{
    public int CollectionId { get; set; }
    public Collection Collection { get; set; } = default!;

    public string UserId { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

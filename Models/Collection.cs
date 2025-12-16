using Azure_SearchProject.Models;
using System.ComponentModel.DataAnnotations;

namespace Azure_SearchProject.Models;

public class Collection
{
    public int Id { get; set; }

    public string OwnerId { get; set; } = default!;

    [Required, MaxLength(80)]
    public string Name { get; set; } = default!;

    public bool IsPublic { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CollectionImage> CollectionImages { get; set; } = new List<CollectionImage>();
    public ICollection<CollectionFollow> Followers { get; set; } = new List<CollectionFollow>();
}

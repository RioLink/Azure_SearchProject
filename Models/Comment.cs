using System.ComponentModel.DataAnnotations;

namespace Azure_SearchProject.Models;

public class Comment
{
    public int Id { get; set; }

    public int ImageItemId { get; set; }
    public ImageItem ImageItem { get; set; } = default!;

    public string UserId { get; set; } = default!;

    [Required, MaxLength(500)]
    public string Text { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

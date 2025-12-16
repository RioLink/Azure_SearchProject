using Azure_SearchProject.Models;
using System.ComponentModel.DataAnnotations;

namespace Azure_SearchProject.Models;

public class Tag
{
    public int Id { get; set; }

    [Required, MaxLength(40)]
    public string Name { get; set; } = default!;

    public ICollection<ImageTag> ImageTags { get; set; } = new List<ImageTag>();
    public ICollection<UserPreferenceTag> UserPreferences { get; set; } = new List<UserPreferenceTag>();
}

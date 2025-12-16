namespace Azure_SearchProject.Models;

public class UserPreferenceTag
{
    public string UserId { get; set; } = default!;
    public int TagId { get; set; }
    public Tag Tag { get; set; } = default!;
    public int Weight { get; set; } = 0;
}

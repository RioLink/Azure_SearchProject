using Azure_SearchProject.Data;
using Azure_SearchProject.Models;
using Azure_SearchProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Azure_SearchProject.Pages.Images;

public class UploadModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IBlobStorageService _blob;
    private readonly IContentModerationService _moderation;

    public UploadModel(ApplicationDbContext db, UserManager<IdentityUser> userManager,
        IBlobStorageService blob, IContentModerationService moderation)
    {
        _db = db;
        _userManager = userManager;
        _blob = blob;
        _moderation = moderation;
    }

    [BindProperty] public IFormFile File { get; set; } = default!;
    [BindProperty] public string? Title { get; set; }
    [BindProperty] public string? Tags { get; set; }
    [BindProperty] public bool IsPublic { get; set; } = true;

    public string? Message { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (File == null || File.Length == 0) { Message = "No file."; return Page(); }
        if (!File.ContentType.StartsWith("image/")) { Message = "Only images allowed."; return Page(); }
        if (File.Length > 5 * 1024 * 1024) { Message = "Max 5MB."; return Page(); }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        await using var ms = new MemoryStream();
        await File.CopyToAsync(ms);
        ms.Position = 0;

        var mod = await _moderation.CheckAsync(ms);
        if (!mod.Allowed)
        {
            Message = $"Rejected: {mod.Reason}";
            return Page();
        }

        ms.Position = 0;
        var url = await _blob.UploadImageAsync(ms, File.ContentType, File.FileName);

        var img = new ImageItem
        {
            OwnerId = user.Id,
            BlobUrl = url,
            Title = Title,
            IsPublic = IsPublic,
            SafetyStatus = "Approved"
        };

        var tagNames = (Tags ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.ToLowerInvariant())
            .Distinct()
            .Take(20)
            .ToList();

        foreach (var name in tagNames)
        {
            var tag = _db.Tags.FirstOrDefault(t => t.Name == name) ?? new Tag { Name = name };
            img.ImageTags.Add(new ImageTag { Tag = tag });
        }

        _db.Images.Add(img);
        await _db.SaveChangesAsync();

        Message = "Uploaded!";
        return RedirectToPage("/Images/Details", new { id = img.Id });
    }
}

using Azure_SearchProject.Data;
using Azure_SearchProject.Models;
using Azure_SearchProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Azure_SearchProject.Pages.Collections;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IBlobStorageService _blob;

    public DetailsModel(
        ApplicationDbContext db,
        UserManager<IdentityUser> userManager,
        IBlobStorageService blob)
    {
        _db = db;
        _userManager = userManager;
        _blob = blob;
    }

    public Collection? Collection { get; set; }
    public List<ImageItem> Images { get; set; } = new();
    public bool CanEdit { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Collection = await _db.Collections.FirstOrDefaultAsync(c => c.Id == id);
        if (Collection == null) return NotFound();

        var user = User.Identity?.IsAuthenticated == true
            ? await _userManager.GetUserAsync(User)
            : null;

        CanEdit = user != null && Collection.OwnerId == user.Id;

        if (!Collection.IsPublic && !CanEdit)
            return Forbid();

        Images = await _db.CollectionImages
            .AsNoTracking()
            .Where(ci => ci.CollectionId == id)
            .Include(ci => ci.ImageItem)
            .Select(ci => ci.ImageItem)
            .Where(i => i.SafetyStatus == "Approved" && i.IsPublic)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostRemoveImageAsync(int id, int imageId)
    {
        var user = await _userManager.GetUserAsync(User);
        var col = await _db.Collections
            .FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == user!.Id);

        if (col == null) return Forbid();

        var rel = await _db.CollectionImages
            .FirstOrDefaultAsync(x => x.CollectionId == id && x.ImageItemId == imageId);

        if (rel != null)
        {
            _db.CollectionImages.Remove(rel);
            await _db.SaveChangesAsync();
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostToggleVisibilityAsync(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var col = await _db.Collections
            .FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == user!.Id);

        if (col == null) return Forbid();

        col.IsPublic = !col.IsPublic;
        await _db.SaveChangesAsync();

        return RedirectToPage(new { id });
    }

    public string GetImageUrl(ImageItem img)
    {
        return _blob.GetReadSasUrl(img.BlobUrl);
    }
}
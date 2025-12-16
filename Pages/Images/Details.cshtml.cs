using Azure_SearchProject.Data;
using Azure_SearchProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Azure_SearchProject.Pages.Images;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public DetailsModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public ImageItem? Image { get; set; }
    public List<string> Tags { get; set; } = new();
    public int LikeCount { get; set; }
    public bool UserLiked { get; set; }
    public List<Comment> Comments { get; set; } = new();
    public List<Collection> MyCollections { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Image = await _db.Images
            .Include(i => i.ImageTags).ThenInclude(it => it.Tag)
            .Include(i => i.Likes)
            .FirstOrDefaultAsync(i => i.Id == id && i.SafetyStatus == "Approved" && i.IsPublic);

        if (Image == null) return NotFound();

        Tags = Image.ImageTags.Select(x => x.Tag.Name).ToList();
        LikeCount = Image.Likes.Count;

        Comments = await _db.Comments
            .AsNoTracking()
            .Where(c => c.ImageItemId == id)
            .OrderByDescending(c => c.CreatedAt)
            .Take(50)
            .ToListAsync();

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                UserLiked = await _db.Likes.AnyAsync(x => x.ImageItemId == id && x.UserId == user.Id);

                MyCollections = await _db.Collections
                    .AsNoTracking()
                    .Where(c => c.OwnerId == user.Id)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                await AddPreferenceWeights(user.Id, Tags, plus: 1);
            }
        }

        return Page();
    }

    [Authorize]
    public async Task<IActionResult> OnPostToggleLikeAsync(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var like = await _db.Likes.FirstOrDefaultAsync(x => x.ImageItemId == id && x.UserId == user.Id);
        var tags = await _db.ImageTags
            .Where(x => x.ImageItemId == id)
            .Include(x => x.Tag)
            .Select(x => x.Tag.Name)
            .ToListAsync();

        if (like == null)
        {
            _db.Likes.Add(new ImageLike { ImageItemId = id, UserId = user.Id });
            await AddPreferenceWeights(user.Id, tags, plus: 2);
        }
        else
        {
            _db.Likes.Remove(like);
            await AddPreferenceWeights(user.Id, tags, plus: -1);
        }

        await _db.SaveChangesAsync();
        return RedirectToPage(new { id });
    }

    [Authorize]
    public async Task<IActionResult> OnPostAddCommentAsync(int id, string text)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        text = (text ?? "").Trim();
        if (text.Length == 0 || text.Length > 500) return RedirectToPage(new { id });

        _db.Comments.Add(new Comment { ImageItemId = id, UserId = user.Id, Text = text });

        var tags = await _db.ImageTags.Where(x => x.ImageItemId == id)
            .Include(x => x.Tag)
            .Select(x => x.Tag.Name)
            .ToListAsync();

        await AddPreferenceWeights(user.Id, tags, plus: 1);

        await _db.SaveChangesAsync();
        return RedirectToPage(new { id });
    }

    [Authorize]
    public async Task<IActionResult> OnPostAddToCollectionAsync(int id, int collectionId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var col = await _db.Collections.FirstOrDefaultAsync(c => c.Id == collectionId && c.OwnerId == user.Id);
        if (col == null) return Forbid();

        var exists = await _db.CollectionImages.AnyAsync(x => x.CollectionId == collectionId && x.ImageItemId == id);
        if (!exists)
        {
            _db.CollectionImages.Add(new CollectionImage { CollectionId = collectionId, ImageItemId = id });

            var tags = await _db.ImageTags.Where(x => x.ImageItemId == id)
                .Include(x => x.Tag)
                .Select(x => x.Tag.Name)
                .ToListAsync();

            await AddPreferenceWeights(user.Id, tags, plus: 1);

            await _db.SaveChangesAsync();
        }

        return RedirectToPage(new { id });
    }

    private async Task AddPreferenceWeights(string userId, IEnumerable<string> tags, int plus)
    {
        foreach (var t in tags.Select(x => x.ToLowerInvariant()).Distinct())
        {
            var tag = await _db.Tags.FirstOrDefaultAsync(x => x.Name == t);
            if (tag == null) continue;

            var pref = await _db.UserPreferenceTags.FirstOrDefaultAsync(x => x.UserId == userId && x.TagId == tag.Id);
            if (pref == null)
            {
                pref = new UserPreferenceTag { UserId = userId, TagId = tag.Id, Weight = Math.Max(0, plus) };
                _db.UserPreferenceTags.Add(pref);
            }
            else
            {
                pref.Weight = Math.Max(0, pref.Weight + plus);
            }
        }
    }
}

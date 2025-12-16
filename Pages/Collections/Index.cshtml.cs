using Azure_SearchProject.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Azure_SearchProject.Pages.Collections;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public IndexModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public List<Models.Collection> MyCollections { get; set; } = new();
    public List<PublicCollectionVm> PublicCollections { get; set; } = new();

    public record PublicCollectionVm(int Id, string Name, bool IsFollowedByMe);

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        MyCollections = await _db.Collections
            .AsNoTracking()
            .Where(c => c.OwnerId == user!.Id)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var publics = await _db.Collections
            .AsNoTracking()
            .Where(c => c.IsPublic && c.OwnerId != user!.Id)
            .OrderByDescending(c => c.CreatedAt)
            .Take(50)
            .ToListAsync();

        var followedIds = await _db.CollectionFollows
            .AsNoTracking()
            .Where(f => f.UserId == user!.Id)
            .Select(f => f.CollectionId)
            .ToListAsync();

        PublicCollections = publics.Select(c => new PublicCollectionVm(c.Id, c.Name, followedIds.Contains(c.Id))).ToList();
    }

    [Authorize]
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var col = await _db.Collections.FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == user!.Id);
        if (col == null) return NotFound();

        _db.Collections.Remove(col);
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }

    [Authorize]
    public async Task<IActionResult> OnPostToggleFollowAsync(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        var col = await _db.Collections.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id && c.IsPublic);
        if (col == null) return NotFound();
        if (col.OwnerId == user!.Id) return RedirectToPage();

        var follow = await _db.CollectionFollows.FirstOrDefaultAsync(x => x.CollectionId == id && x.UserId == user.Id);
        if (follow == null) _db.CollectionFollows.Add(new Models.CollectionFollow { CollectionId = id, UserId = user.Id });
        else _db.CollectionFollows.Remove(follow);

        await _db.SaveChangesAsync();
        return RedirectToPage();
    }
}

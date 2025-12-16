using Azure_SearchProject.Data;
using Azure_SearchProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Azure_SearchProject.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class ModerationModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public ModerationModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public List<ImageItem> Images { get; set; } = new();

    public async Task OnGetAsync()
    {
        Images = await _db.Images
            .AsNoTracking()
            .OrderByDescending(i => i.CreatedAt)
            .Take(50)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostSetStatusAsync(int id, string status)
    {
        var img = await _db.Images.FirstOrDefaultAsync(i => i.Id == id);
        if (img == null) return NotFound();

        if (status != "Approved" && status != "Rejected" && status != "Pending")
            return BadRequest();

        img.SafetyStatus = status;
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostLockUserAsync(string email, int minutes)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return RedirectToPage();

        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(Math.Max(1, minutes));
        await _userManager.UpdateAsync(user);

        return RedirectToPage();
    }
}

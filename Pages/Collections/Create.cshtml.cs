using Azure_SearchProject.Data;
using Azure_SearchProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Azure_SearchProject.Pages.Collections;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public CreateModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public bool IsPublic { get; set; } = false;

    public void OnGet() { }

    [Authorize]
    public async Task<IActionResult> OnPostAsync()
    {
        Name = (Name ?? "").Trim();
        if (Name.Length == 0 || Name.Length > 80) return Page();

        var user = await _userManager.GetUserAsync(User);
        _db.Collections.Add(new Collection { OwnerId = user!.Id, Name = Name, IsPublic = IsPublic });

        await _db.SaveChangesAsync();
        return RedirectToPage("/Collections/Index");
    }
}

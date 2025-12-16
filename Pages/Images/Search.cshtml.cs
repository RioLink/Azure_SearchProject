using Azure_SearchProject.Models;
using Azure_SearchProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Azure_SearchProject.Pages.Images;

public class SearchModel : PageModel
{
    private readonly ISearchService _search;
    private readonly UserManager<IdentityUser> _userManager;

    public SearchModel(ISearchService search, UserManager<IdentityUser> userManager)
    {
        _search = search;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    public List<ImageItem>? Results { get; set; }

    public async Task OnGetAsync()
    {
        var userId = User.Identity?.IsAuthenticated == true
            ? (await _userManager.GetUserAsync(User))?.Id
            : null;

        Results = await _search.SearchAsync(Q, userId);
    }
}

using Azure_SearchProject.Models.ViewModels;
using Azure_SearchProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel : PageModel
{
    private readonly ISearchService _search;
    private readonly UserManager<IdentityUser> _userManager;

    public HomeViewModel Vm { get; set; } = new();

    public IndexModel(ISearchService search, UserManager<IdentityUser> userManager)
    {
        _search = search;
        _userManager = userManager;
    }

    public async Task OnGetAsync()
    {
        Vm.Trending = await _search.GetTrendingAsync();
        Vm.PopularTags = await _search.GetPopularTagsAsync();

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(User)!;
            Vm.Recommended = await _search.GetRecommendedAsync(userId);
        }
    }
}

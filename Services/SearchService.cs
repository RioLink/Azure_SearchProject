using Azure_SearchProject.Data;
using Azure_SearchProject.Models;
using Microsoft.EntityFrameworkCore;

namespace Azure_SearchProject.Services;

public class SearchService : ISearchService
{
    private readonly ApplicationDbContext _db;

    public SearchService(ApplicationDbContext db) => _db = db;

    public async Task<List<ImageItem>> GetTrendingAsync(int take = 8)
    {
        return await _db.Images
            .AsNoTracking()
            .Include(i => i.Likes)
            .Where(i =>
                i.IsPublic &&
                i.SafetyStatus == "Approved" &&
                !i.IsDeleted)
            .OrderByDescending(i => i.Likes.Count)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<ImageItem>> GetRecommendedAsync(string userId, int take = 8)
    {
        var tagIds = await _db.UserPreferenceTags
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.Weight)
            .Select(p => p.TagId)
            .Take(10)
            .ToListAsync();

        if (!tagIds.Any())
            return await GetTrendingAsync(take);

        return await _db.Images
            .AsNoTracking()
            .Include(i => i.ImageTags)
            .ThenInclude(it => it.Tag)
            .Where(i =>
                i.IsPublic &&
                i.SafetyStatus == "Approved" &&
                !i.IsDeleted &&
                i.ImageTags.Any(it => tagIds.Contains(it.TagId)))
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<Tag>> GetPopularTagsAsync(int take = 12)
    {
        return await _db.Tags
            .AsNoTracking()
            .OrderByDescending(t => t.ImageTags.Count)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<ImageItem>> SearchAsync(string? query, string? userId)
    {
        query ??= "";
        var tokens = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                          .Select(x => x.ToLowerInvariant()).ToList();

        var baseQ = _db.Images
            .AsNoTracking()
            .Include(i => i.ImageTags).ThenInclude(it => it.Tag)
            .Include(i => i.Likes)
            .Where(i =>
                i.IsPublic &&
                i.SafetyStatus == "Approved" &&
                !i.IsDeleted);

        if (tokens.Count > 0)
        {
            baseQ = baseQ.Where(i =>
                (i.Title != null && tokens.Any(t => i.Title.ToLower().Contains(t))) ||
                (i.Description != null && tokens.Any(t => i.Description.ToLower().Contains(t))) ||
                i.ImageTags.Any(it => tokens.Contains(it.Tag.Name.ToLower()))
            );
        }

        var list = await baseQ.ToListAsync();

        Dictionary<string, int> pref = new();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            pref = await _db.UserPreferenceTags
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Include(x => x.Tag)
                .ToDictionaryAsync(x => x.Tag.Name.ToLower(), x => x.Weight);
        }

        double Score(ImageItem i)
        {
            double s = 0;
            foreach (var t in tokens)
            {
                if (!string.IsNullOrEmpty(i.Title) && i.Title.Contains(t, StringComparison.OrdinalIgnoreCase)) s += 3;
                if (!string.IsNullOrEmpty(i.Description) && i.Description.Contains(t, StringComparison.OrdinalIgnoreCase)) s += 2;

                if (i.ImageTags.Any(x => x.Tag.Name.Equals(t, StringComparison.OrdinalIgnoreCase))) s += 10;
            }

            s += 0.2 * i.Likes.Count;

            foreach (var tag in i.ImageTags.Select(x => x.Tag.Name.ToLower()))
                if (pref.TryGetValue(tag, out var w)) s += w;

            return s;
        }

        return list.OrderByDescending(Score).ThenByDescending(x => x.CreatedAt).ToList();
    }
}

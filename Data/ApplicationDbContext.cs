using Azure_SearchProject.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Azure_SearchProject.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<ImageItem> Images => Set<ImageItem>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ImageTag> ImageTags => Set<ImageTag>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<CollectionImage> CollectionImages => Set<CollectionImage>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<ImageLike> Likes => Set<ImageLike>();
    public DbSet<CollectionFollow> CollectionFollows => Set<CollectionFollow>();
    public DbSet<UserPreferenceTag> UserPreferenceTags => Set<UserPreferenceTag>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ImageTag>().HasKey(x => new { x.ImageItemId, x.TagId });
        builder.Entity<CollectionImage>().HasKey(x => new { x.CollectionId, x.ImageItemId });
        builder.Entity<ImageLike>().HasKey(x => new { x.ImageItemId, x.UserId });
        builder.Entity<CollectionFollow>().HasKey(x => new { x.CollectionId, x.UserId });
        builder.Entity<UserPreferenceTag>().HasKey(x => new { x.UserId, x.TagId });

        builder.Entity<Tag>().HasIndex(t => t.Name).IsUnique();
    }
}

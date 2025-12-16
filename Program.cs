using Azure_SearchProject.Data;
using Azure_SearchProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =======================
// DATABASE (LocalDB)
// =======================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// =======================
// IDENTITY
// =======================
builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// =======================
// RAZOR PAGES
// =======================
builder.Services.AddRazorPages();

// =======================
// APPLICATION SERVICES
// =======================
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IContentModerationService, ContentModerationStubService>();
builder.Services.AddScoped<ISearchService, SearchService>();

// =======================
// AUTHORIZATION
// =======================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

// =======================
// BUILD
// =======================
var app = builder.Build();

// =======================
// PIPELINE
// =======================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// =======================
// SEED ADMIN
// =======================
await DbSeeder.SeedAsync(app.Services);

app.Run();

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Entities;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

//Adding RateLimiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", c =>
    {
        c.PermitLimit = 60;
        c.Window = TimeSpan.FromMinutes(1);
        c.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        c.QueueLimit = 0;
    });
});

builder.Services.AddCors();

// Add Identity
builder.Services.AddIdentityApiEndpoints<User>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

// Configure authentication with JWT
builder.Services.ConfigureApplicationCookie(options => { options.Cookie.SameSite = SameSiteMode.None; });


var app = builder.Build();

// Pipeline
// ================================================================================
// Enable CORS for requests from the frontend
app.UseCors(c => c
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .WithOrigins(
        "http://localhost:3000",
        "https://localhost:3000",
        "http://127.0.0.1:3000",
        "https://127.0.0.1:3000",
        "http://127.0.0.1:5500", 
        "http://localhost:5500" 
    )
);

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireRateLimiting("fixed");

app.MapGroup("api").MapIdentityApi<User>();

// Seed dummy data...
// ================================================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();
        await InitializeDb.SeedData(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Det gick fel vid migrering av databasen.");
    }
}



app.Run();

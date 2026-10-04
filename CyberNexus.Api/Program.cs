using System.Text;
using CyberNexus.Api.Data;
using CyberNexus.Api.Models;
using CyberNexus.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ── Database ─────────────────────────────────────────────────────────────────
// Railway injects a DATABASE_URL env var in the format:
//   postgresql://user:password@host:port/dbname
// We convert it to the Npgsql connection string format.
// Locally, appsettings.json still works (the env var is absent).
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string pgConnection;

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Parse Railway's postgres:// URL → Npgsql Host=...;Database=...;...
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    pgConnection = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};" +
                   $"Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true;";
}
else
{
    // Local dev: fall back to appsettings value (still points at SQL Server
    // locally — for local dev keep using SQL Server; only Railway uses Postgres)
    pgConnection = builder.Configuration.GetConnectionString("DefaultConnection")!;
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(pgConnection));

// ── JWT ───────────────────────────────────────────────────────────────────────
// Railway: set Jwt__Key as an environment variable in the Railway dashboard.
// Locally: dotnet user-secrets set "Jwt:Key" "<your-key>"
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? Environment.GetEnvironmentVariable("Jwt__Key")
    ?? throw new InvalidOperationException(
        "Jwt:Key is missing. Set it with dotnet user-secrets (local) or the Jwt__Key env var (Railway).");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew                = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<ProgressionService>();

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebApp", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            // Add any extra origins (Cloudflare Pages preview URLs, etc.) here.
            var allowed = new[]
            {
                "https://cybernexus-learning.pages.dev",
            };

            // Also allow any *.pages.dev preview deploy
            policy.SetIsOriginAllowed(origin =>
                    allowed.Contains(origin) ||
                    origin.EndsWith(".pages.dev", StringComparison.OrdinalIgnoreCase))
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

var app = builder.Build();

// ── Seed catalogue + promote admins ──────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                                      .CreateLogger("CatalogSeed");
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // Apply any pending migrations automatically on startup.
        // Safe to run every boot — EF skips already-applied migrations.
        db.Database.Migrate();
        CatalogSeeder.Seed(db);
        logger.LogInformation("Catalogue seeding finished.");
        PromoteConfiguredAdmins(db, builder.Configuration, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Seeding/migration failed.");
    }
}

static void PromoteConfiguredAdmins(AppDbContext db, IConfiguration config, ILogger logger)
{
    var emails = config.GetSection("Bootstrap:AdminEmails").Get<string[]>()
                 ?? Array.Empty<string>();
    if (emails.Length == 0) return;

    var wanted = emails
        .Where(e => !string.IsNullOrWhiteSpace(e))
        .Select(e => e.Trim().ToLowerInvariant())
        .ToHashSet();

    var promoted = db.Users
        .Where(u => wanted.Contains(u.Email.ToLower()) && u.Role != AppUser.AdminRole)
        .ToList();

    foreach (var user in promoted)
    {
        user.Role = AppUser.AdminRole;
        logger.LogWarning("Promoted {Email} to Admin.", user.Email);
    }
    db.SaveChanges();
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();
else
    app.UseHttpsRedirection();

app.UseCors("WebApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

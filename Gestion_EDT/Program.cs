using Microsoft.EntityFrameworkCore;
using Gestion_EDT.Data;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ─────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── SQL Server via EF Core ─────────────────────────────────────
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Chaîne de connexion 'DefaultConnection' introuvable dans appsettings.json.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)  // Changé : UseMySql → UseSqlServer
);

var app = builder.Build();

// ── Middleware ───────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// ── Routes ───────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ── Migration automatique au démarrage ───────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Gestion_EDT.Data;
using Gestion_EDT.Models;

var builder = WebApplication.CreateBuilder(args);

// ── MVC + AUTORISATION GLOBALE ────────────────────────────────────
// Tous les contrôleurs/actions exigent une connexion par défaut.
// Seules les actions marquées [AllowAnonymous] (Login, AccessDenied)
// restent accessibles sans authentification.
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.Filters.Add(new AuthorizeFilter(policy));
});

// ── MySQL via Pomelo EF Core ──────────────────────────────────────
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Chaîne de connexion 'DefaultConnection' introuvable dans appsettings.json.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);

// ── AUTHENTIFICATION PAR COOKIE ────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath        = "/Account/Login";
        options.LogoutPath       = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan   = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "GestionEDT.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();

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

// ⚠️ Ordre important : Authentication AVANT Authorization
app.UseAuthentication();
app.UseAuthorization();

// ── ROUTES ───────────────────────────────────────────────────────

// 1) Route DÉDIÉE pour la racine "/" → Account/Login
//    Pattern vide = ne correspond QUE à "/" (pas à "/Planning", etc.)
//    - Non connecté  → AccountController.Login() affiche le formulaire
//                       (action marquée [AllowAnonymous], donc le
//                        filtre global d'autorisation est ignoré ici)
//    - Déjà connecté → AccountController.Login() redirige vers
//                       Home/Index (dashboard) — logique déjà présente
//                       dans AccountController.
app.MapControllerRoute(
    name: "root",
    pattern: "",
    defaults: new { controller = "Account", action = "Login" });

// 2) Route GÉNÉRALE pour tout le reste : {controller=Home}/{action=Index}/{id?}
//    Gère /Home, /Planning, /Mentions, /Seances, /Enseignants (action=Index
//    par défaut), ainsi que /Controleur/Action/id.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ── Migration automatique + création des comptes par défaut ──────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    // Créer les comptes par défaut si la table est vide
    if (!db.Utilisateurs.Any())
    {
        var hasher = new PasswordHasher<Utilisateur>();

        var admin = new Utilisateur
        {
            Login      = "admin",
            Role       = "Admin",
            NomComplet = "Administrateur EMIT",
            Actif      = true
        };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin@2026");

        var respEdt = new Utilisateur
        {
            Login      = "respedt",
            Role       = "RespEDT",
            NomComplet = "Responsable Emploi du Temps",
            Actif      = true
        };
        respEdt.PasswordHash = hasher.HashPassword(respEdt, "RespEDT@2026");

        db.Utilisateurs.AddRange(admin, respEdt);
        db.SaveChanges();
    }
}

app.Run();

/*
═══════════════════════════════════════════════════════════════════
 COMPORTEMENT DES ROUTES
═══════════════════════════════════════════════════════════════════

   GET  /                  → /Account/Login (page par défaut)
   GET  /Account/Login     → formulaire de connexion
   POST /Account/Login     → vérifie identifiants, crée le cookie,
                              redirige vers /Home/Index (ou ReturnUrl)

   Toute autre URL (ex: /Mentions, /Seances, /Planning) :
     - utilisateur NON connecté → redirigé automatiquement vers
       /Account/Login?ReturnUrl=/url/demandee
     - utilisateur connecté     → accès normal (selon son rôle)


 COMPTES PAR DÉFAUT CRÉÉS AU PREMIER DÉMARRAGE
═══════════════════════════════════════════════════════════════════

   Administrateur :
     Login    : admin
     Password : Admin@2026
     Rôle     : Admin

   Responsable Emploi du Temps :
     Login    : respedt
     Password : RespEDT@2026
     Rôle     : RespEDT

 ⚠️ À CHANGER IMMÉDIATEMENT après le premier déploiement via
    /Account/ChangePassword.
═══════════════════════════════════════════════════════════════════
*/

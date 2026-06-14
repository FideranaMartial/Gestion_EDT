using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Gestion_EDT.Data;
using Gestion_EDT.Models;

namespace Gestion_EDT.Controllers
{
    /// <summary>
    /// Authentification — route /Account
    ///
    /// RG38 — droits par rôle :
    ///   Admin      → CRUD complet structure académique + gestion des comptes
    ///   RespEDT    → CRUD séances
    ///   Enseignant → consultation de son propre planning uniquement
    /// </summary>
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly PasswordHasher<Utilisateur> _hasher = new();

        public AccountController(ApplicationDbContext db) => _db = db;

        // ═══════════════════════════════════════════════════════════
        //  CONNEXION / DÉCONNEXION
        // ═══════════════════════════════════════════════════════════

        // ── GET /Account/Login ───────────────────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                // ⚠️ Redirection EXPLICITE vers "/Home/Index" (2 segments).
                // RedirectToAction("Index","Home") génère "/" (Home+Index
                // sont les valeurs par défaut de la route → omises),
                // ce qui boucle vers la route "root" → Login() → boucle infinie.
                return Redirect("/Home/Index");

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // ── POST /Account/Login ──────────────────────────────────────
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var utilisateur = await _db.Utilisateurs
                .Include(u => u.Enseignant)
                .FirstOrDefaultAsync(u => u.Login == vm.Login);

            // Identifiant inconnu OU compte désactivé
            if (utilisateur == null || !utilisateur.Actif)
            {
                ModelState.AddModelError("", "Identifiant ou mot de passe incorrect.");
                return View(vm);
            }

            // Vérification du mot de passe (hash + salt)
            var resultat = _hasher.VerifyHashedPassword(
                utilisateur, utilisateur.PasswordHash, vm.Password);

            if (resultat == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Identifiant ou mot de passe incorrect.");
                return View(vm);
            }

            // Construction des claims (informations de session)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, utilisateur.Id.ToString()),
                new Claim(ClaimTypes.Name, utilisateur.Login),
                new Claim(ClaimTypes.Role, utilisateur.Role),
                new Claim("NomComplet", utilisateur.NomComplet ?? utilisateur.Login)
            };

            // Si le compte est lié à un enseignant → utilisé pour T14 (planning personnel)
            if (utilisateur.EnseignantId.HasValue)
                claims.Add(new Claim("EnseignantId", utilisateur.EnseignantId.Value.ToString()));

            var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = vm.RememberMe,                 // cookie persistant si "se souvenir"
                    ExpiresUtc   = DateTimeOffset.UtcNow.AddHours(8)
                });

            // Redirection vers la page demandée à l'origine, sinon dashboard
            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);

            // ⚠️ Redirection EXPLICITE (voir commentaire sur l'autre Login()
            // ci-dessus pour l'explication de la boucle infinie évitée).
            return Redirect("/Home/Index");
        }

        // ── POST /Account/Logout ─────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        // ── GET /Account/AccessDenied ────────────────────────────────
        [AllowAnonymous]
        public IActionResult AccessDenied() => View();


        // ═══════════════════════════════════════════════════════════
        //  GESTION DES COMPTES (Admin uniquement)
        // ═══════════════════════════════════════════════════════════

        // ── GET /Account/Index ───────────────────────────────────────
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var utilisateurs = await _db.Utilisateurs
                .Include(u => u.Enseignant)
                .OrderBy(u => u.Login)
                .ToListAsync();

            if (TempData["Success"] != null) ViewData["Success"] = TempData["Success"];
            if (TempData["Error"]   != null) ViewData["Error"]   = TempData["Error"];

            return View(utilisateurs);
        }

        // ── GET /Account/Create ──────────────────────────────────────
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await ChargerListeEnseignants();
            return View(new UtilisateurFormModel());
        }

        // ── POST /Account/Create ─────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UtilisateurFormModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Password))
                ModelState.AddModelError(nameof(vm.Password),
                    "Le mot de passe est obligatoire à la création.");

            if (await _db.Utilisateurs.AnyAsync(u => u.Login == vm.Login))
                ModelState.AddModelError(nameof(vm.Login), "Cet identifiant existe déjà.");

            if (!ModelState.IsValid)
            {
                await ChargerListeEnseignants();
                return View(vm);
            }

            var utilisateur = new Utilisateur
            {
                Login        = vm.Login.Trim(),
                Role         = vm.Role,
                NomComplet   = vm.NomComplet,
                EnseignantId = vm.Role == "Enseignant" ? vm.EnseignantId : null,
                Actif        = vm.Actif
            };
            utilisateur.PasswordHash = _hasher.HashPassword(utilisateur, vm.Password!);

            _db.Utilisateurs.Add(utilisateur);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Compte \"{utilisateur.Login}\" créé avec le rôle {utilisateur.Role}.";
            return RedirectToAction(nameof(Index));
        }

        // ── GET /Account/Edit/{id} ────────────────────────────────────
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var u = await _db.Utilisateurs.FindAsync(id);
            if (u == null) return NotFound();

            await ChargerListeEnseignants();

            return View(new UtilisateurFormModel
            {
                Id           = u.Id,
                Login        = u.Login,
                Role         = u.Role,
                NomComplet   = u.NomComplet,
                EnseignantId = u.EnseignantId,
                Actif        = u.Actif
                // Password laissé vide : on ne le change que si renseigné
            });
        }

        // ── POST /Account/Edit/{id} ───────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UtilisateurFormModel vm)
        {
            if (id != vm.Id) return BadRequest();

            var u = await _db.Utilisateurs.FindAsync(id);
            if (u == null) return NotFound();

            // Unicité du login (hors lui-même)
            if (await _db.Utilisateurs.AnyAsync(x => x.Login == vm.Login && x.Id != id))
                ModelState.AddModelError(nameof(vm.Login), "Cet identifiant existe déjà.");

            if (!ModelState.IsValid)
            {
                await ChargerListeEnseignants();
                return View(vm);
            }

            u.Login        = vm.Login.Trim();
            u.Role         = vm.Role;
            u.NomComplet   = vm.NomComplet;
            u.EnseignantId = vm.Role == "Enseignant" ? vm.EnseignantId : null;
            u.Actif        = vm.Actif;

            // Changer le mot de passe uniquement si un nouveau a été saisi
            if (!string.IsNullOrWhiteSpace(vm.Password))
                u.PasswordHash = _hasher.HashPassword(u, vm.Password);

            await _db.SaveChangesAsync();
            TempData["Success"] = $"Compte \"{u.Login}\" mis à jour.";
            return RedirectToAction(nameof(Index));
        }

        // ── POST /Account/Delete/{id} ─────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var u = await _db.Utilisateurs.FindAsync(id);
            if (u == null) return NotFound();

            // Empêcher la suppression de son propre compte
            if (u.Login == User.Identity?.Name)
            {
                TempData["Error"] = "Vous ne pouvez pas supprimer votre propre compte.";
                return RedirectToAction(nameof(Index));
            }

            _db.Utilisateurs.Remove(u);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Compte \"{u.Login}\" supprimé.";
            return RedirectToAction(nameof(Index));
        }


        // ═══════════════════════════════════════════════════════════
        //  CHANGEMENT DE MOT DE PASSE (utilisateur connecté)
        // ═══════════════════════════════════════════════════════════

        // ── GET /Account/ChangePassword ───────────────────────────────
        [Authorize]
        public IActionResult ChangePassword() => View(new ChangePasswordFormModel());

        // ── POST /Account/ChangePassword ──────────────────────────────
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordFormModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var login = User.Identity?.Name;
            var u = await _db.Utilisateurs.FirstOrDefaultAsync(x => x.Login == login);
            if (u == null) return NotFound();

            var verif = _hasher.VerifyHashedPassword(u, u.PasswordHash, vm.CurrentPassword);
            if (verif == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(nameof(vm.CurrentPassword), "Mot de passe actuel incorrect.");
                return View(vm);
            }

            u.PasswordHash = _hasher.HashPassword(u, vm.NewPassword);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Mot de passe modifié avec succès.";
            // ⚠️ Redirection EXPLICITE (même raison que dans Login()).
            return Redirect("/Home/Index");
        }


        // ═══════════════════════════════════════════════════════════
        //  MÉTHODES PRIVÉES
        // ═══════════════════════════════════════════════════════════

        private async Task ChargerListeEnseignants()
        {
            ViewData["Enseignants"] = new SelectList(
                await _db.Enseignants
                    .OrderBy(e => e.nom_enseignant)
                    .Select(e => new
                    {
                        e.Id,
                        NomComplet = e.prenom_enseignant + " " + e.nom_enseignant
                    })
                    .ToListAsync(),
                "Id", "NomComplet");

            ViewData["Roles"] = new SelectList(new[]
            {
                new { Value = "Admin",      Text = "Administrateur" },
                new { Value = "RespEDT",    Text = "Responsable Emploi du Temps" },
                new { Value = "Enseignant", Text = "Enseignant" }
            }, "Value", "Text");
        }
    }
}

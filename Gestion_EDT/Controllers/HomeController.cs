using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_EDT.Data;
using Gestion_EDT.Models;

namespace Gestion_EDT.Controllers
{
    /// <summary>
    /// Tableau de bord — route /Home ou /
    /// Vue : Views/Home/Index.cshtml
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db) => _db = db;

        // GET /
        public async Task<IActionResult> Index()
        {
            // Statistiques globales pour le dashboard
            ViewData["TotalMentions"]    = await _db.Mentions.CountAsync();
            ViewData["TotalCycles"]      = await _db.Cycles.CountAsync();
            ViewData["TotalParcours"]    = await _db.Parcours.CountAsync();
            ViewData["TotalGroupes"]     = await _db.Groupes.CountAsync();
            ViewData["TotalEnseignants"] = await _db.Enseignants.CountAsync();
            ViewData["TotalMatieres"]    = await _db.Matieres.CountAsync();
            ViewData["TotalSalles"]      = await _db.Salles.CountAsync();
            ViewData["TotalSeances"]     = await _db.Seances.CountAsync();

            // Séances du jour (T12 — RG13)
            var today = DateTime.Today;
            var seancesDuJour = await _db.Seances
                .Where(s => s.date_seance == today)
                .Include(s => s.Matiere)
                .Include(s => s.Salle)
                .Include(s => s.Enseignant)
                .Include(s => s.Groupe)
                .OrderBy(s => s.heure_debut)
                .ToListAsync();

            ViewData["SeancesDuJour"]   = seancesDuJour;
            ViewData["SemaineCourante"] = System.Globalization.ISOWeek
                .GetWeekOfYear(DateTime.Today);

            return View();
        }

        // GET /Home/Privacy
        public IActionResult Privacy() => View();

        // GET /Home/Error
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id
                            ?? HttpContext.TraceIdentifier
            });
        }

        // ── GET /Home/GetKPIs ──────────────────────────────────────
[HttpGet]
public async Task<IActionResult> GetKPIs()
{
    try
    {
        // 1. Total des mentions (obligatoire)
        var totalMentions = await _db.Mentions.CountAsync();

        // 2. Séances de la semaine (uniquement si la table Seances existe)
        var seancesSemaine = 0;
        var heuresEnseignees = 0.0;
        try
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1); // Lundi
            var endOfWeek = startOfWeek.AddDays(5); // Vendredi
            seancesSemaine = await _db.Seances
                .Where(s => s.date_seance >= startOfWeek && s.date_seance <= endOfWeek)
                .CountAsync();
            heuresEnseignees = await _db.Seances
                .Where(s => s.date_seance >= startOfWeek && s.date_seance <= endOfWeek)
                .SumAsync(s => (s.heure_fin - s.heure_debut).TotalHours);
        }
        catch
        {
            // Si la table Seances n'existe pas, on ignore et on garde les valeurs par défaut
        }

        // 3. Taux d'occupation (uniquement si Salles existe)
        var tauxOccupation = 0;
        try
        {
            var totalSalles = await _db.Salles.CountAsync();
            if (totalSalles > 0)
            {
                tauxOccupation = (int)Math.Round((double)seancesSemaine / (totalSalles * 5) * 100);
            }
        }
        catch
        {
            // Si la table Salles n'existe pas, on ignore
        }

        return Json(new
        {
            totalMentions,
            seancesSemaine,
            tauxOccupation,
            heuresEnseignees = Math.Round(heuresEnseignees, 1)
        });
    }
    catch (Exception ex)
    {
        // Log de l'erreur réelle
        System.Console.WriteLine($"Erreur GetKPIs: {ex.Message}");
        // Retourner des valeurs par défaut (0) pour ne pas planter le dashboard
        return Json(new { totalMentions = 0, seancesSemaine = 0, tauxOccupation = 0, heuresEnseignees = 0 });
    }
}
// ── GET /Home/GetSeancesParJour ────────────────────────────
[HttpGet]
public async Task<IActionResult> GetSeancesParJour()
{
    try
    {
        var jours = new[] { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi" };
        var data = new int[5];
        for (int i = 0; i < 5; i++)
        {
            int jourSemaine = i + 1; // Lundi = 1, Mardi = 2, ...
            data[i] = await _db.Seances
                .Where(s => ((int)s.date_seance.DayOfWeek + 6) % 7 + 1 == jourSemaine)
                .CountAsync();
        }
        return Json(new { labels = jours, data = data });
    }
    catch (Exception ex)
    {
        // En cas d'erreur, renvoyer des données vides (évite la casse du dashboard)
        return Json(new { labels = new[] { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi" }, data = new int[5] });
    }
}
    }
}

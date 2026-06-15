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
            var kpis = new
            {
                totalMentions = await _db.Mentions.CountAsync(),
                totalEnseignants = await _db.Enseignants.CountAsync(),
                totalGroupes = await _db.Groupes.CountAsync(),
                totalSalles = await _db.Salles.CountAsync(),
                totalMatieres = await _db.Matieres.CountAsync(),
                seancesSemaine = await _db.Seances.CountAsync(s => s.semaine == System.Globalization.ISOWeek.GetWeekOfYear(DateTime.Today)),
                heuresEnseignees = await _db.Seances
                    .Where(s => s.semaine == System.Globalization.ISOWeek.GetWeekOfYear(DateTime.Today))
                    .SumAsync(s => (s.heure_fin - s.heure_debut).TotalHours)
            };
            return Json(kpis);
        }

        // ── GET /Home/GetSeancesParJour ────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetSeancesParJour()
        {
            var jours = new[] { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi" };
            var data = new int[5];

            for (int i = 0; i < 5; i++)
            {
                // Lundi = 1, Mardi = 2, etc.
                int jourSemaine = i + 1;
                data[i] = await _db.Seances
                    .Where(s => ((int)s.date_seance.DayOfWeek + 6) % 7 + 1 == jourSemaine)
                    .CountAsync();
            }

            return Json(new { labels = jours, data = data });
        }
    }
}

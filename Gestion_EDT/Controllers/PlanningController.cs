using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Gestion_EDT.Data;

namespace Gestion_EDT.Controllers
{
    /// <summary>
    /// Planning — route /Planning
    /// Vue : Planning/Index.cshtml (FullCalendar)
    ///
    /// T12 — EDT global (Admin / Resp. EDT)
    /// T14 — Planning personnel enseignant
    /// T15 — EDT filtré Mention → Parcours → Groupe
    /// RG13 — droits de consultation différenciés
    /// </summary>
    public class PlanningController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PlanningController(ApplicationDbContext db) => _db = db;

        // ── GET /Planning ────────────────────────────────────────────
        // Alimente les 3 selects en cascade : Mention → Parcours → Groupe
        public async Task<IActionResult> Index()
        {
            // Filtre Mention (1er select)
            ViewData["Mentions"] = new SelectList(
                await _db.Mentions.OrderBy(m => m.nom_mention).ToListAsync(),
                "Id", "nom_mention");

            // Filtre Enseignant (vue personnelle T14)
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

            ViewData["SemaineCourante"] = System.Globalization.ISOWeek
                .GetWeekOfYear(DateTime.Today);

            return View();
        }

        // ── GET /Planning/GetParcours/{mentionId} ────────────────────
        // Filtre en cascade : Mention → Parcours (tous niveaux)
        // Appelé en AJAX par le select "Parcours" de Index.cshtml
        [HttpGet]
        public async Task<IActionResult> GetParcours(int mentionId)
        {
            var parcours = await _db.Parcours
                .Where(p => p.Cycle.MentionId == mentionId)
                .OrderBy(p => p.nom_parcours)
                .Select(p => new
                {
                    value = p.Id,
                    label = p.nom_parcours,
                    cycle = p.Cycle.nom_cycle
                })
                .ToListAsync();

            return Json(parcours);
        }

        // ── GET /Planning/GetGroupes/{parcoursId} ────────────────────
        // Filtre en cascade : Parcours → Groupes
        // Appelé en AJAX par le select "Groupe" de Index.cshtml
        [HttpGet]
        public async Task<IActionResult> GetGroupes(int parcoursId)
        {
            var groupes = await _db.Groupes
                .Where(g => g.ParcoursId == parcoursId)
                .OrderBy(g => g.nom_groupe)
                .Select(g => new
                {
                    value      = g.Id,
                    label      = g.nom_groupe,
                    nbEtudiant = g.nb_etudiant
                })
                .ToListAsync();

            return Json(groupes);
        }
    }
}

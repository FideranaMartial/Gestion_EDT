using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_EDT.Data;
using Gestion_EDT.Models;

namespace Gestion_EDT.Controllers
{
    /// <summary>
    /// Gestion des Enseignants — route /Enseignants
    ///
    /// RG13 — matricule unique, champs obligatoires
    /// RG14 — pas deux séances simultanées pour un même enseignant
    /// RG15 — plafond charge horaire hebdomadaire
    /// RG17 — suppression bloquée si séances à venir
    /// </summary>
    public class EnseignantsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public EnseignantsController(ApplicationDbContext db) => _db = db;

        // ── GET /Enseignants ─────────────────────────────────────────
        public async Task<IActionResult> Index(string? search)
        {
            var query = _db.Enseignants.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(e =>
                    e.nom_enseignant.Contains(search) ||
                    e.prenom_enseignant.Contains(search) ||
                    e.matricule.Contains(search));

            var liste = await query
                .OrderBy(e => e.nom_enseignant)
                .ThenBy(e => e.prenom_enseignant)
                .ToListAsync();

            ViewData["Search"] = search;

            if (TempData["Success"] != null) ViewData["Success"] = TempData["Success"];
            if (TempData["Error"]   != null) ViewData["Error"]   = TempData["Error"];

            return View(liste);
        }

        // ── GET /Enseignants/Details/{id} ────────────────────────────
        // T14 — planning personnel de l'enseignant (filtré par matricule)
        public async Task<IActionResult> Details(int id)
        {
            var enseignant = await _db.Enseignants
                .Include(e => e.Seances)
                    .ThenInclude(s => s.Matiere)
                .Include(e => e.Seances)
                    .ThenInclude(s => s.Salle)
                .Include(e => e.Seances)
                    .ThenInclude(s => s.Groupe)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (enseignant == null) return NotFound();

            // RG15 — charge horaire semaine courante
            int semaineCourante = System.Globalization.ISOWeek
                .GetWeekOfYear(DateTime.Today);

            double totalHeures = enseignant.Seances
                .Where(s => s.semaine == semaineCourante)
                .Sum(s => (s.heure_fin - s.heure_debut).TotalHours);

            ViewData["TotalHeuresSemaine"] = Math.Round(totalHeures, 1);
            ViewData["SemaineCourante"]    = semaineCourante;

            return View(enseignant);
        }

        // ── GET /Enseignants/Create ──────────────────────────────────
        public IActionResult Create() => View(new Enseignant());

        // ── POST /Enseignants/Create ─────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Enseignant enseignant)
        {
            if (!ModelState.IsValid) return View(enseignant);

            // RG13 — unicité du matricule
            bool existe = await _db.Enseignants
                .AnyAsync(e => e.matricule == enseignant.matricule);
            if (existe)
            {
                ModelState.AddModelError(nameof(enseignant.matricule),
                    "Ce matricule est déjà utilisé. (RG13)");
                return View(enseignant);
            }

            _db.Enseignants.Add(enseignant);
            await _db.SaveChangesAsync();

            TempData["Success"] =
                $"Enseignant {enseignant.prenom_enseignant} {enseignant.nom_enseignant} enregistré.";
            return RedirectToAction(nameof(Index));
        }

        // ── GET /Enseignants/Edit/{id} ───────────────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            var enseignant = await _db.Enseignants.FindAsync(id);
            if (enseignant == null) return NotFound();
            return View(enseignant);
        }

        // ── POST /Enseignants/Edit/{id} ──────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Enseignant enseignant)
        {
            if (id != enseignant.Id) return BadRequest();
            if (!ModelState.IsValid) return View(enseignant);

            try
            {
                _db.Update(enseignant);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Enseignant mis à jour.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.Enseignants.AnyAsync(e => e.Id == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // ── GET /Enseignants/Delete/{id} ─────────────────────────────
        public async Task<IActionResult> Delete(int id)
        {
            var enseignant = await _db.Enseignants
                .Include(e => e.Seances)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (enseignant == null) return NotFound();
            return View(enseignant);
        }

        // ── POST /Enseignants/Delete/{id} ────────────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var enseignant = await _db.Enseignants
                .Include(e => e.Seances)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (enseignant == null) return NotFound();

            // RG17 — séances à venir encore assignées ?
            bool aSeancesAVenir = enseignant.Seances
                .Any(s => s.date_seance >= DateTime.Today);

            if (aSeancesAVenir)
            {
                TempData["Error"] =
                    "Impossible de supprimer : cet enseignant a des séances à venir. (RG17)";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _db.Enseignants.Remove(enseignant);
            await _db.SaveChangesAsync();

            TempData["Success"] =
                $"{enseignant.prenom_enseignant} {enseignant.nom_enseignant} supprimé.";
            return RedirectToAction(nameof(Index));
        }

        // ── GET /Enseignants/GetAll ───────────────────────────────────
        // API JSON — alimente le <select> Enseignant dans Seance/Create.cshtml
        // Remplace window.MockAPI.getEnseignants()
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var liste = await _db.Enseignants
                .OrderBy(e => e.nom_enseignant)
                .Select(e => new
                {
                    id     = e.Id,
                    nom    = e.nom_enseignant,
                    prenom = e.prenom_enseignant,
                    grade  = e.grade
                })
                .ToListAsync();

            return Json(liste);
        }

        // ── GET /Enseignants/Planning/{id}?semaine=10 ────────────────
        // API JSON — planning hebdomadaire d'un enseignant (T14)
        [HttpGet]
        public async Task<IActionResult> Planning(int id, int semaine = 0)
        {
            if (semaine == 0)
                semaine = System.Globalization.ISOWeek.GetWeekOfYear(DateTime.Today);

            var seances = await _db.Seances
                .Where(s => s.EnseignantId == id && s.semaine == semaine)
                .Include(s => s.Matiere)
                .Include(s => s.Salle)
                .Include(s => s.Groupe)
                .OrderBy(s => s.date_seance)
                .ThenBy(s => s.heure_debut)
                .Select(s => new
                {
                    s.Id,
                    date       = s.date_seance.ToString("yyyy-MM-dd"),
                    heureDebut = s.heure_debut.ToString(@"hh\:mm"),
                    heureFin   = s.heure_fin.ToString(@"hh\:mm"),
                    matiere    = s.Matiere!.intitule,
                    salle      = s.Salle!.num_salle,
                    groupe     = s.Groupe!.nom_groupe,
                    s.semaine
                })
                .ToListAsync();

            return Json(seances);
        }
    }
}

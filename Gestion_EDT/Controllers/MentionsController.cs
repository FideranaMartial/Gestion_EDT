using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_EDT.Data;
using Gestion_EDT.Models;

namespace Gestion_EDT.Controllers
{
    /// <summary>
    /// Gestion des Mentions — route /Mentions
    /// Vue Edit.cshtml déjà fournie
    ///
    /// RG01 — 3 mentions EMIT
    /// RG02 — code_mention unique, nom obligatoire
    /// RG03 — génération automatique des cycles L / M
    /// RG04 — génération automatique des parcours L1→L3, M1→M2
    /// RG06 — suppression bloquée si cycles actifs
    /// RG07 — suppression bloquée si matières liées
    /// </summary>
    public class MentionsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public MentionsController(ApplicationDbContext db) => _db = db;

        // ── GET /Mentions ────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var mentions = await _db.Mentions
                .Include(m => m.Cycles)
                    .ThenInclude(c => c.Parcours)
                        .ThenInclude(p => p.Groupes)
                .OrderBy(m => m.nom_mention)
                .ToListAsync();

            // Message flash depuis TempData
            if (TempData["Success"] != null)
                ViewData["Success"] = TempData["Success"];
            if (TempData["Error"] != null)
                ViewData["Error"] = TempData["Error"];

            return View(mentions);
        }

        // ── GET /Mentions/Details/{id} ───────────────────────────────
        public async Task<IActionResult> Details(int id)
        {
            var mention = await _db.Mentions
                .Include(m => m.Cycles)
                    .ThenInclude(c => c.Parcours)
                        .ThenInclude(p => p.Groupes)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mention == null) return NotFound();
            return View(mention);
        }

        // ── GET /Mentions/Create ─────────────────────────────────────
        public IActionResult Create() => View(new Mention());

                // ── POST /Mentions/Create ────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Mention mention)
        {
            System.Diagnostics.Debug.WriteLine("=== CREATE POST ENTRÉE ===");
            System.Diagnostics.Debug.WriteLine($"Nom: {mention?.nom_mention ?? "null"}");
            System.Diagnostics.Debug.WriteLine($"Code: {mention?.code_mention ?? "null"}");

            if (mention == null)
            {
                System.Diagnostics.Debug.WriteLine("Mention est null !");
                return View(new Mention());
            }

            // Ignorer la validation de Cycles (non fourni par le formulaire)
            ModelState.Remove("Cycles");

            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("ModelState invalide");
                return View(mention);
            }

            // Unicité
            if (await _db.Mentions.AnyAsync(m => m.code_mention == mention.code_mention))
            {
                System.Diagnostics.Debug.WriteLine("Code déjà existant");
                ModelState.AddModelError(nameof(mention.code_mention), "Ce code mention existe déjà.");
                return View(mention);
            }

            mention.code_mention = mention.code_mention.Trim().ToUpperInvariant();
            System.Diagnostics.Debug.WriteLine($"Code transformé: {mention.code_mention}");

            _db.Mentions.Add(mention);
            System.Diagnostics.Debug.WriteLine("Avant SaveChangesAsync");
            await _db.SaveChangesAsync();
            System.Diagnostics.Debug.WriteLine("Après SaveChangesAsync");

            // Création des cycles (Licence + Master)
            var cycleL = new Cycle { nom_cycle = $"Licence {mention.nom_mention}", niveau = "L", MentionId = mention.Id };
            var cycleM = new Cycle { nom_cycle = $"Master {mention.nom_mention}", niveau = "M", MentionId = mention.Id };
            _db.Cycles.AddRange(cycleL, cycleM);
            await _db.SaveChangesAsync();

            // Création des parcours L1, L2, L3 et M1, M2
            var parcoursLicence = new[] { "L1", "L2", "L3" }
                .Select(n => new Parcours { nom_parcours = n, CycleId = cycleL.Id });
            var parcoursMaster = new[] { "M1", "M2" }
                .Select(n => new Parcours { nom_parcours = n, CycleId = cycleM.Id });
            _db.Parcours.AddRange(parcoursLicence);
            _db.Parcours.AddRange(parcoursMaster);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Mention \"{mention.nom_mention}\" créée avec cycles et parcours.";
            return RedirectToAction(nameof(Index), new { created = 1 });
        }

        // ── GET /Mentions/Edit/{id} ──────────────────────────────────
        // Correspond à la vue Edit.cshtml fournie
        public async Task<IActionResult> Edit(int id)
        {
            var mention = await _db.Mentions.FindAsync(id);
            if (mention == null) return NotFound();

            ViewData["MentionId"] = mention.Id;
            return View(mention);
        }

        // ── POST /Mentions/Edit/{id} ─────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,code_mention,nom_mention")] Mention mention)
        {
            if (id != mention.Id) return BadRequest();

            // Supprime la validation de Cycles (qui n'est pas soumise par le formulaire)
            ModelState.Remove("Cycles");

            if (!ModelState.IsValid)
            {
                ViewData["MentionId"] = id;
                return View(mention);
            }

            try
            {
                _db.Update(mention);
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Mention \"{mention.nom_mention}\" modifiée.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.Mentions.AnyAsync(m => m.Id == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // ── GET /Mentions/Delete/{id} ────────────────────────────────
        public async Task<IActionResult> Delete(int id)
        {
            var mention = await _db.Mentions
                .Include(m => m.Cycles)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mention == null) return NotFound();
            return View(mention);
        }

        // ── POST /Mentions/Delete/{id} ───────────────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mention = await _db.Mentions
                .Include(m => m.Cycles)
                    .ThenInclude(c => c.Parcours)
                        .ThenInclude(p => p.Groupes)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mention == null) return NotFound();

            // RG06 — groupes encore actifs ?
            bool aDesGroupes = mention.Cycles
                .SelectMany(c => c.Parcours)
                .SelectMany(p => p.Groupes)
                .Any();
            if (aDesGroupes)
            {
                TempData["Error"] =
                    "Impossible de supprimer : des groupes existent encore dans cette mention.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _db.Mentions.Remove(mention);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Mention \"{mention.nom_mention}\" supprimée.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteAjax(int id)
{
    try
    {
        var mention = await _db.Mentions
            .Include(m => m.Cycles)
                .ThenInclude(c => c.Parcours)
                    .ThenInclude(p => p.Groupes)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (mention == null)
            return Json(new { success = false, message = "Mention non trouvée." });

        bool aDesGroupes = mention.Cycles
            .SelectMany(c => c.Parcours)
            .SelectMany(p => p.Groupes)
            .Any();
        if (aDesGroupes)
            return Json(new { success = false, message = "Impossible de supprimer : des groupes existent encore." });

        _db.Mentions.Remove(mention);
        await _db.SaveChangesAsync();
        return Json(new { success = true, message = $"Mention \"{mention.nom_mention}\" supprimée." });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = ex.Message });
    }
}

        // ── GET /Mentions/GetCycles/{mentionId} ──────────────────────
        // API JSON — filtre en cascade Mention → Cycles dans le Planning
        [HttpGet]
        public async Task<IActionResult> GetCycles(int mentionId)
        {
            var cycles = await _db.Cycles
                .Where(c => c.MentionId == mentionId)
                .OrderBy(c => c.niveau)
                .Select(c => new { c.Id, c.nom_cycle, c.niveau })
                .ToListAsync();
            return Json(cycles);
        }

        // ── GET /Mentions/GetParcours/{cycleId} ──────────────────────
        // API JSON — filtre en cascade Cycle → Parcours dans le Planning
        [HttpGet]
        public async Task<IActionResult> GetParcours(int cycleId)
        {
            var parcours = await _db.Parcours
                .Where(p => p.CycleId == cycleId)
                .OrderBy(p => p.nom_parcours)
                .Select(p => new { p.Id, p.nom_parcours })
                .ToListAsync();
            return Json(parcours);
        }

                // ── GET /Mentions/GetMentionsJson ───────────────────────────
        // API JSON pour le chargement asynchrone dans la vue Index
        [HttpGet]
        public async Task<IActionResult> GetMentionsJson()
        {
            var mentions = await _db.Mentions
                .OrderBy(m => m.nom_mention)
                .Select(m => new { m.Id, Nom = m.nom_mention, Niveau = "Non défini" }) // Le niveau n'est pas stocké dans Mention, on peut le déduire ou le laisser vide
                .ToListAsync();
            return Json(mentions);
        }
    }
}

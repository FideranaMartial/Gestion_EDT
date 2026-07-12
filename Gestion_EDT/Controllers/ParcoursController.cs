using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Gestion_EDT.Data;
using Gestion_EDT.Models;

namespace Gestion_EDT.Controllers
{
    public class ParcoursController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ParcoursController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var parcours = await _db.Parcours
                .Include(p => p.Cycle)
                .ThenInclude(c => c.Mention)
                .OrderBy(p => p.nom_parcours)
                .ToListAsync();
            return View(parcours);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["CycleId"] = new SelectList(await _db.Cycles.Include(c => c.Mention).ToListAsync(), "Id", "nom_cycle");
            return View(new Parcours());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Parcours parcours)
        {

            ModelState.Remove("Cycle");
            ModelState.Remove("Groupes");

            ModelState.Remove("Parcours.Cycle");
            ModelState.Remove("Parcours.Groupes");
            if (ModelState.IsValid)
            {
                _db.Parcours.Add(parcours);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Parcours créé avec succès.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["CycleId"] = new SelectList(await _db.Cycles.Include(c => c.Mention).ToListAsync(), "Id", "nom_cycle", parcours.CycleId);
            return View(parcours);
        }

        // GET /Parcours/GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _db.Parcours
                .Include(p => p.Cycle)
                .Select(p => new { p.Id, Nom = p.nom_parcours, Cycle = p.Cycle.nom_cycle, CycleId = p.CycleId })
                .ToListAsync();
            return Json(data);
        }

        // GET /Parcours/GetById/{id}
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _db.Parcours
                .Include(p => p.Cycle)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (p == null) return NotFound();
            var cycles = await _db.Cycles.Select(c => new { c.Id, Nom = c.nom_cycle }).ToListAsync();
            return Json(new { id = p.Id, nom = p.nom_parcours, cycleId = p.CycleId, cycles });
        }

        // POST /Parcours/Edit (JSON)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromBody] ParcoursEditDto dto)
        {
            var parcours = await _db.Parcours.FindAsync(dto.Id);
            if (parcours == null)
                return NotFound(new { success = false, message = "Parcours non trouvé." });

            parcours.nom_parcours = dto.Nom;
            parcours.CycleId = dto.CycleId;
            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Parcours modifié avec succès." });
        }

        // POST /Parcours/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var parcours = await _db.Parcours.FindAsync(id);
            if (parcours == null)
                return NotFound(new { success = false, message = "Parcours non trouvé." });

            _db.Parcours.Remove(parcours);
            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Parcours supprimé." });
        }
    }

    public class ParcoursEditDto
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public int CycleId { get; set; }
    }
}
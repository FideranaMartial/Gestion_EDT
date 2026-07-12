using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Gestion_EDT.Data;
using Gestion_EDT.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Gestion_EDT.Controllers
{
    public class CyclesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CyclesController(ApplicationDbContext db) => _db = db;

        // GET: Cycles/Index
        public async Task<IActionResult> Index()
        {
            var cycles = await _db.Cycles
                .Include(c => c.Mention)
                .OrderBy(c => c.nom_cycle)
                .ToListAsync();
            return View(cycles);
        }

        // GET: Cycles/Create
        public async Task<IActionResult> Create()
        {
            ViewData["MentionId"] = new SelectList(await _db.Mentions.OrderBy(m => m.nom_mention).ToListAsync(), "Id", "nom_mention");
            return View(new Cycle());
        }

        // POST: Cycles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Cycle cycle)
        {
            // Supprimer les propriétés de navigation de la validation
            ModelState.Remove("Mention");
            ModelState.Remove("Parcours");
            ModelState.Remove("Cycle.Mention");
            ModelState.Remove("Cycle.Parcours");

            if (ModelState.IsValid)
            {
                _db.Cycles.Add(cycle);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Cycle créé avec succès.";
                return RedirectToAction(nameof(Index));
            }

            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new { errors });
        }

        // GET: Cycles/GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _db.Cycles
                .Include(c => c.Mention)
                .Select(c => new
                {
                    c.Id,
                    Nom = c.nom_cycle,
                    Niveau = c.niveau,
                    Mention = c.Mention.nom_mention,
                    MentionId = c.MentionId
                })
                .ToListAsync();
            return Json(data);
        }

        // GET: Cycles/GetById/{id}
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var cycle = await _db.Cycles
                .Include(c => c.Mention)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cycle == null) return NotFound();

            var mentions = await _db.Mentions
                .OrderBy(m => m.nom_mention)
                .Select(m => new { m.Id, Nom = m.nom_mention })
                .ToListAsync();

            return Json(new
            {
                id = cycle.Id,
                nom = cycle.nom_cycle,
                niveau = cycle.niveau,
                mentionId = cycle.MentionId,
                mentions = mentions
            });
        }

        // POST: Cycles/Edit (JSON)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromBody] CycleEditDto dto)
        {
            var cycle = await _db.Cycles.FindAsync(dto.Id);
            if (cycle == null)
                return NotFound(new { success = false, message = "Cycle non trouvé." });

            cycle.nom_cycle = dto.Nom;
            cycle.niveau = dto.Niveau;
            cycle.MentionId = dto.MentionId;

            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Cycle modifié." });
        }

        // POST: Cycles/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var cycle = await _db.Cycles.FindAsync(id);
            if (cycle == null)
                return NotFound(new { success = false, message = "Cycle non trouvé." });

            _db.Cycles.Remove(cycle);
            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Cycle supprimé." });
        }

        // DTO pour l'édition
        public class CycleEditDto
        {
            public int Id { get; set; }
            public string Nom { get; set; }
            public string Niveau { get; set; }
            public int MentionId { get; set; }
        }
    }
}
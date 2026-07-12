using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Gestion_EDT.Data;
using Gestion_EDT.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Gestion_EDT.Controllers
{
    public class SallesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public SallesController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var salles = await _db.Salles
                .Include(s => s.Batiment)
                .OrderBy(s => s.num_salle)
                .ToListAsync();
            return View(salles);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["BatimentId"] = new SelectList(await _db.Batiments.OrderBy(b => b.nom_batiment).ToListAsync(), "Id", "nom_batiment");
            return View(new Salle());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Salle salle)
        {
            ModelState.Remove("Batiment");
            ModelState.Remove("Seances");
            ModelState.Remove("Salle.Batiment");
            ModelState.Remove("Salle.Seances");

            if (ModelState.IsValid)
            {
                _db.Salles.Add(salle);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Salle créée avec succès.";
                return RedirectToAction(nameof(Index));
            }

            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new { errors });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _db.Salles
                .Include(s => s.Batiment)
                .Select(s => new { s.Id, Numero = s.num_salle, Capacite = s.capacite, BatimentNom = s.Batiment.nom_batiment, BatimentId = s.BatimentId })
                .ToListAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var salle = await _db.Salles
                .Include(s => s.Batiment)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (salle == null) return NotFound();

            var batiments = await _db.Batiments.OrderBy(b => b.nom_batiment)
                .Select(b => new { b.Id, Nom = b.nom_batiment })
                .ToListAsync();

            return Json(new
            {
                id = salle.Id,
                numero = salle.num_salle,
                capacite = salle.capacite,
                batimentId = salle.BatimentId,
                batiments = batiments
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromBody] SalleEditDto dto)
        {
            var salle = await _db.Salles.FindAsync(dto.Id);
            if (salle == null)
                return NotFound(new { success = false, message = "Salle non trouvée." });

            salle.num_salle = dto.Numero;
            salle.capacite = dto.Capacite;
            salle.BatimentId = dto.BatimentId;

            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Salle modifiée." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var salle = await _db.Salles.FindAsync(id);
            if (salle == null)
                return NotFound(new { success = false, message = "Salle non trouvée." });

            _db.Salles.Remove(salle);
            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Salle supprimée." });
        }

        public class SalleEditDto
        {
            public int Id { get; set; }
            public string Numero { get; set; }
            public int Capacite { get; set; }
            public int BatimentId { get; set; }
        }
    }
}
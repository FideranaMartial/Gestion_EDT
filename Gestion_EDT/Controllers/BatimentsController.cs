using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_EDT.Data;
using Gestion_EDT.Models;
using System.Threading.Tasks;
using System.Linq;

namespace Gestion_EDT.Controllers
{
    public class BatimentsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public BatimentsController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var batiments = await _db.Batiments.OrderBy(b => b.nom_batiment).ToListAsync();
            return View(batiments);
        }

        public IActionResult Create() => View(new Batiment());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Batiment batiment)
        {
            ModelState.Remove("Salles");
            ModelState.Remove("Batiment.Salles");

            if (ModelState.IsValid)
            {
                _db.Batiments.Add(batiment);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Bâtiment créé avec succès.";
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
            var data = await _db.Batiments
                .Select(b => new { b.Id, Nom = b.nom_batiment })
                .ToListAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var batiment = await _db.Batiments.FindAsync(id);
            if (batiment == null) return NotFound();
            return Json(new { id = batiment.Id, nom = batiment.nom_batiment });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromBody] BatimentEditDto dto)
        {
            var batiment = await _db.Batiments.FindAsync(dto.Id);
            if (batiment == null)
                return NotFound(new { success = false, message = "Bâtiment non trouvé." });

            batiment.nom_batiment = dto.Nom;
            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Bâtiment modifié." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var batiment = await _db.Batiments.FindAsync(id);
            if (batiment == null)
                return NotFound(new { success = false, message = "Bâtiment non trouvé." });

            _db.Batiments.Remove(batiment);
            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Bâtiment supprimé." });
        }

        public class BatimentEditDto
        {
            public int Id { get; set; }
            public string Nom { get; set; }
        }
    }
}
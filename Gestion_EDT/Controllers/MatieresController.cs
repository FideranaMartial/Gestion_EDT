using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Gestion_EDT.Data;
using Gestion_EDT.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Gestion_EDT.Controllers
{
    public class MatieresController : Controller
    {
        private readonly ApplicationDbContext _db;

        public MatieresController(ApplicationDbContext db) => _db = db;

        // GET: Matieres
        public async Task<IActionResult> Index()
        {
            var matieres = await _db.Matieres
                .OrderBy(m => m.intitule)
                .ToListAsync();
            return View(matieres);
        }

        // GET: Matieres/Create
        public IActionResult Create() => View(new Matiere());

        // POST: Matieres/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Matiere matiere)
        {
            ModelState.Remove("Seances");
            ModelState.Remove("ParcoursMatieres");
            ModelState.Remove("Matiere.Seances");
            ModelState.Remove("Matiere.ParcoursMatieres");

            if (ModelState.IsValid)
            {
                _db.Matieres.Add(matiere);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Matière créée avec succès.";
                return RedirectToAction(nameof(Index));
            }

            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new { errors });
        }

        // GET: Matieres/GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _db.Matieres
                .Select(m => new { m.Id, Code = m.code_mat, Intitule = m.intitule, NbHeure = m.nb_heure })
                .ToListAsync();
            return Json(data);
        }

        // GET: Matieres/GetById/{id}
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var matiere = await _db.Matieres.FindAsync(id);
            if (matiere == null) return NotFound();
            return Json(new { id = matiere.Id, code = matiere.code_mat, intitule = matiere.intitule, nbHeure = matiere.nb_heure });
        }

        // POST: Matieres/Edit (JSON)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromBody] MatiereEditDto dto)
        {
            var matiere = await _db.Matieres.FindAsync(dto.Id);
            if (matiere == null)
                return NotFound(new { success = false, message = "Matière non trouvée." });

            matiere.code_mat = dto.Code;
            matiere.intitule = dto.Intitule;
            matiere.nb_heure = dto.NbHeure;

            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Matière modifiée." });
        }

        // POST: Matieres/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var matiere = await _db.Matieres.FindAsync(id);
            if (matiere == null)
                return NotFound(new { success = false, message = "Matière non trouvée." });

            _db.Matieres.Remove(matiere);
            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Matière supprimée." });
        }

        public class MatiereEditDto
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Intitule { get; set; }
            public int NbHeure { get; set; }
        }
    }
}
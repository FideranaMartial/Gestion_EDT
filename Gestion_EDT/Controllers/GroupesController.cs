using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Gestion_EDT.Data;
using Gestion_EDT.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Gestion_EDT.Controllers
{
    public class GroupesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public GroupesController(ApplicationDbContext db) => _db = db;

        // GET: Groupes
        public async Task<IActionResult> Index()
        {
            var groupes = await _db.Groupes
                .Include(g => g.Parcours)
                .ThenInclude(p => p.Cycle)
                .OrderBy(g => g.nom_groupe)
                .ToListAsync();
            return View(groupes);
        }

        // GET: Groupes/Create
        public async Task<IActionResult> Create()
        {
            ViewData["ParcoursId"] = new SelectList(await _db.Parcours
                .Include(p => p.Cycle)
                .OrderBy(p => p.nom_parcours)
                .ToListAsync(), "Id", "nom_parcours");
            return View(new Groupe());
        }

        // POST: Groupes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Groupe groupe)
        {
            ModelState.Remove("Parcours");
            ModelState.Remove("Seances");
            ModelState.Remove("Groupe.Parcours");
            ModelState.Remove("Groupe.Seances");

            if (ModelState.IsValid)
            {
                _db.Groupes.Add(groupe);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Groupe créé avec succès.";
                return RedirectToAction(nameof(Index));
            }

            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new { errors });
        }

        // GET: Groupes/GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _db.Groupes
                .Include(g => g.Parcours)
                .Select(g => new { g.Id, Nom = g.nom_groupe, Code = g.code_groupe, Effectif = g.nb_etudiant, ParcoursNom = g.Parcours.nom_parcours, ParcoursId = g.ParcoursId })
                .ToListAsync();
            return Json(data);
        }

        // GET: Groupes/GetById/{id}
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var groupe = await _db.Groupes
                .Include(g => g.Parcours)
                .FirstOrDefaultAsync(g => g.Id == id);
            if (groupe == null) return NotFound();

            var parcoursList = await _db.Parcours
                .Include(p => p.Cycle)
                .OrderBy(p => p.nom_parcours)
                .Select(p => new { p.Id, Nom = p.nom_parcours })
                .ToListAsync();

            return Json(new
            {
                id = groupe.Id,
                nom = groupe.nom_groupe,
                code = groupe.code_groupe,
                effectif = groupe.nb_etudiant,
                parcoursId = groupe.ParcoursId,
                parcours = parcoursList
            });
        }

        // POST: Groupes/Edit (AJAX JSON)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromBody] GroupeEditDto dto)
        {
            var groupe = await _db.Groupes.FindAsync(dto.Id);
            if (groupe == null)
                return NotFound(new { success = false, message = "Groupe non trouvé." });

            groupe.nom_groupe = dto.Nom;
            groupe.code_groupe = dto.Code;
            groupe.nb_etudiant = dto.Effectif;
            groupe.ParcoursId = dto.ParcoursId;

            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Groupe modifié." });
        }

        // POST: Groupes/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var groupe = await _db.Groupes.FindAsync(id);
            if (groupe == null)
                return NotFound(new { success = false, message = "Groupe non trouvé." });

            _db.Groupes.Remove(groupe);
            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Groupe supprimé." });
        }

        // DTO pour l'édition
        public class GroupeEditDto
        {
            public int Id { get; set; }
            public string Nom { get; set; }
            public string Code { get; set; }
            public int Effectif { get; set; }
            public int ParcoursId { get; set; }
        }
    }
}
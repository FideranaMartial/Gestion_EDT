using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Gestion_EDT.Data;
using Gestion_EDT.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Gestion_EDT.Controllers
{
    public class ParcoursMatieresController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ParcoursMatieresController(ApplicationDbContext db) => _db = db;

        // GET: ParcoursMatieres
        public async Task<IActionResult> Index()
        {
            var associations = await _db.ParcoursMatieres
                .Include(pm => pm.Parcours)
                    .ThenInclude(p => p.Cycle)
                .Include(pm => pm.Matiere)
                .OrderBy(pm => pm.Parcours.nom_parcours)
                .ThenBy(pm => pm.Matiere.intitule)
                .ToListAsync();
            return View(associations);
        }

        // GET: ParcoursMatieres/Create
        public async Task<IActionResult> Create()
        {
            ViewData["ParcoursId"] = new SelectList(await _db.Parcours.Include(p => p.Cycle).OrderBy(p => p.nom_parcours).ToListAsync(), "Id", "nom_parcours");
            ViewData["MatiereId"] = new SelectList(await _db.Matieres.OrderBy(m => m.intitule).ToListAsync(), "Id", "intitule");
            return View();
        }

        // POST: ParcoursMatieres/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] int parcoursId, [FromForm] int matiereId)
        {
            if (parcoursId <= 0 || matiereId <= 0)
            {
                return BadRequest(new { errors = new[] { "Parcours et matière sont requis." } });
            }

            // Vérifier si l'association existe déjà
            var exists = await _db.ParcoursMatieres.AnyAsync(pm => pm.ParcoursId == parcoursId && pm.MatiereId == matiereId);
            if (exists)
            {
                return BadRequest(new { errors = new[] { "Cette association existe déjà." } });
            }

            var association = new ParcoursMatiere
            {
                ParcoursId = parcoursId,
                MatiereId = matiereId
            };

            _db.ParcoursMatieres.Add(association);
            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Association créée." });
        }

        // POST: ParcoursMatieres/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] int parcoursId, [FromForm] int oldMatiereId, [FromForm] int newMatiereId)
        {
            if (parcoursId <= 0 || oldMatiereId <= 0 || newMatiereId <= 0)
                return BadRequest(new { errors = new[] { "Tous les champs sont requis." } });

            // Supprimer l'ancienne association
            var old = await _db.ParcoursMatieres
                .FirstOrDefaultAsync(pm => pm.ParcoursId == parcoursId && pm.MatiereId == oldMatiereId);
            if (old == null)
                return NotFound(new { success = false, message = "Association non trouvée." });

            // Vérifier si la nouvelle association existe déjà
            var exists = await _db.ParcoursMatieres
                .AnyAsync(pm => pm.ParcoursId == parcoursId && pm.MatiereId == newMatiereId);
            if (exists)
                return BadRequest(new { errors = new[] { "Cette matière est déjà associée à ce parcours." } });

            // Créer la nouvelle association
            var newAssociation = new ParcoursMatiere
            {
                ParcoursId = parcoursId,
                MatiereId = newMatiereId
            };

            _db.ParcoursMatieres.Remove(old);
            _db.ParcoursMatieres.Add(newAssociation);
            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Association modifiée." });
        }

        // POST: ParcoursMatieres/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int parcoursId, int matiereId)
        {
            var association = await _db.ParcoursMatieres
                .FirstOrDefaultAsync(pm => pm.ParcoursId == parcoursId && pm.MatiereId == matiereId);
            if (association == null)
                return NotFound(new { success = false, message = "Association non trouvée." });

            _db.ParcoursMatieres.Remove(association);
            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Association supprimée." });
        }

        // GET: ParcoursMatieres/GetByParcours
        [HttpGet]
        public async Task<IActionResult> GetByParcours(int parcoursId)
        {
            var matieres = await _db.ParcoursMatieres
                .Where(pm => pm.ParcoursId == parcoursId)
                .Include(pm => pm.Matiere)
                .Select(pm => new { pm.Matiere.Id, pm.Matiere.intitule, pm.Matiere.code_mat })
                .ToListAsync();
            return Json(matieres);
        }

        // GET: ParcoursMatieres/GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _db.ParcoursMatieres
                .Include(pm => pm.Parcours)
                    .ThenInclude(p => p.Cycle)
                .Include(pm => pm.Matiere)
                .Select(pm => new
                {
                    ParcoursId = pm.ParcoursId,
                    ParcoursNom = pm.Parcours.nom_parcours,
                    CycleNiveau = pm.Parcours.Cycle.niveau,
                    MatiereId = pm.MatiereId,
                    MatiereIntitule = pm.Matiere.intitule,
                    MatiereCode = pm.Matiere.code_mat
                })
                .ToListAsync();
            return Json(data);
        }
    }
}
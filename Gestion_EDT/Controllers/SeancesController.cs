using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Gestion_EDT.Data;
using Gestion_EDT.Models;

namespace Gestion_EDT.Controllers
{
    /// <summary>
    /// Gestion des Séances — route /Seances
    /// Vue Create.cshtml déjà fournie (formulaire date/heures/salle/enseignant)
    ///
    /// RG28 — id auto-généré
    /// RG29 — date_seance >= aujourd'hui
    /// RG30 — heure_fin > heure_debut
    /// RG31 — semaine cohérente avec date_seance
    /// RG32 — matière, salle, enseignant, groupe obligatoires
    /// RG34 — triple détection de conflits (enseignant, salle, groupe)
    /// RG35 — transaction atomique
    /// RG36 — séance passée non supprimable
    /// RG37 — modification relance la vérification complète
    /// </summary>
    public class SeancesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public SeancesController(ApplicationDbContext db) => _db = db;

        // ── GET /Seances ─────────────────────────────────────────────
        public async Task<IActionResult> Index(
            int? mentionId,
            int? parcoursId,
            int? enseignantId,
            int  semaine = 0)
        {
            var query = _db.Seances
                .Include(s => s.Matiere)
                .Include(s => s.Salle).ThenInclude(sa => sa.Batiment)
                .Include(s => s.Enseignant)
                .Include(s => s.Groupe)
                    .ThenInclude(g => g.Parcours)
                        .ThenInclude(p => p.Cycle)
                            .ThenInclude(c => c.Mention)
                .AsQueryable();

            if (mentionId.HasValue)
                query = query.Where(s =>
                    s.Groupe.Parcours.Cycle.MentionId == mentionId);

            if (parcoursId.HasValue)
                query = query.Where(s =>
                    s.Groupe.ParcoursId == parcoursId);

            if (enseignantId.HasValue)
                query = query.Where(s => s.EnseignantId == enseignantId);

            if (semaine > 0)
                query = query.Where(s => s.semaine == semaine);

            var seances = await query
                .OrderBy(s => s.date_seance)
                .ThenBy(s => s.heure_debut)
                .ToListAsync();

            // Listes pour les filtres
            ViewData["Mentions"]     = new SelectList(await _db.Mentions.ToListAsync(),    "Id", "nom_mention",    mentionId);
            ViewData["Parcours"]     = new SelectList(await _db.Parcours.ToListAsync(),    "Id", "nom_parcours",   parcoursId);
            ViewData["Enseignants"]  = new SelectList(await _db.Enseignants.ToListAsync(), "Id", "nom_enseignant", enseignantId);
            ViewData["SemaineActuelle"] = semaine;

            if (TempData["Success"] != null) ViewData["Success"] = TempData["Success"];
            if (TempData["Error"]   != null) ViewData["Error"]   = TempData["Error"];

            return RedirectToAction(nameof(Create));
        }

        // ── GET /Seances/Create ──────────────────────────────────────
        // Correspond à la vue Create.cshtml fournie
        public async Task<IActionResult> Create()
        {
            await ChargerSelectLists();
            return View(new Seance { date_seance = DateTime.Today });
        }

        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Seance seance)
{
    // RG29
    if (seance.date_seance < DateTime.Today)
        ModelState.AddModelError(nameof(seance.date_seance), "La date doit être égale ou postérieure à aujourd'hui.");

    // RG30
    if (seance.heure_fin <= seance.heure_debut)
        ModelState.AddModelError(nameof(seance.heure_fin), "L'heure de fin doit être supérieure à l'heure de début.");

    // Calcul automatique de la semaine
    if (seance.date_seance != default)
    {
        seance.semaine = System.Globalization.ISOWeek.GetWeekOfYear(seance.date_seance);
    }

    // On retire la validation de semaine car elle est calculée automatiquement
    ModelState.Remove(nameof(seance.semaine));

    if (!ModelState.IsValid)
    {
        await ChargerSelectLists(seance);
        // Retourner une vue avec les erreurs ou un JSON d'erreur selon le type de requête
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.ContentType?.Contains("application/json") == true)
            return BadRequest(ModelState);
        return View(seance);
    }

    var conflit = await DetecterConflit(seance);
    if (conflit != null)
    {
        ModelState.AddModelError("", conflit);
        await ChargerSelectLists(seance);
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.ContentType?.Contains("application/json") == true)
            return BadRequest(new { message = conflit });
        return View(seance);
    }

    using var transaction = await _db.Database.BeginTransactionAsync();
    try
    {
        _db.Seances.Add(seance);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.ContentType?.Contains("application/json") == true)
            return Json(new { success = true, message = "Séance planifiée avec succès." });

        TempData["Success"] = "Séance planifiée avec succès.";
        return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        ModelState.AddModelError("", "Erreur lors de l'enregistrement.");
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.ContentType?.Contains("application/json") == true)
            return BadRequest(new { message = ex.Message });
        await ChargerSelectLists(seance);
        return View(seance);
    }
}

        // ── GET /Seances/Edit/{id} ───────────────────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            var seance = await _db.Seances.FindAsync(id);
            if (seance == null) return NotFound();

            // RG36 — séance passée non modifiable
            if (seance.date_seance < DateTime.Today)
            {
                TempData["Error"] =
                    "Une séance passée ne peut pas être modifiée. (RG36)";
                return RedirectToAction(nameof(Index));
            }

            await ChargerSelectLists(seance);
            return View(seance);
        }

        // ── POST /Seances/Edit/{id} ──────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Seance seance)
        {
            if (id != seance.Id) return BadRequest();

            // RG36
            if (seance.date_seance < DateTime.Today)
                ModelState.AddModelError(nameof(seance.date_seance),
                    "Une séance passée ne peut pas être modifiée. (RG36)");

            // RG30
            if (seance.heure_fin <= seance.heure_debut)
                ModelState.AddModelError(nameof(seance.heure_fin),
                    "L'heure de fin doit être supérieure à l'heure de début. (RG30)");

            // RG31
            int semaineAttendue = System.Globalization.ISOWeek
                .GetWeekOfYear(seance.date_seance);
            if (seance.semaine != semaineAttendue)
                ModelState.AddModelError(nameof(seance.semaine),
                    $"Semaine {seance.semaine} incohérente avec la date (attendu : {semaineAttendue}). (RG31)");

            if (!ModelState.IsValid)
            {
                await ChargerSelectLists(seance);
                return View(seance);
            }

            // RG37 — re-vérification complète, séance courante exclue
            var conflit = await DetecterConflit(seance, excludeId: id);
            if (conflit != null)
            {
                ModelState.AddModelError("", conflit);
                await ChargerSelectLists(seance);
                return View(seance);
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.Update(seance);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Séance modifiée avec succès.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Erreur lors de la modification.");
                await ChargerSelectLists(seance);
                return View(seance);
            }
        }

        // ── GET /Seances/Delete/{id} ─────────────────────────────────
        public async Task<IActionResult> Delete(int id)
        {
            var seance = await _db.Seances
                .Include(s => s.Matiere)
                .Include(s => s.Salle)
                .Include(s => s.Enseignant)
                .Include(s => s.Groupe)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (seance == null) return NotFound();
            return View(seance);
        }

        // ── POST /Seances/Delete/{id} ────────────────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var seance = await _db.Seances.FindAsync(id);
            if (seance == null) return NotFound();

            // RG36 — séance passée → non supprimable
            if (seance.date_seance < DateTime.Today)
            {
                TempData["Error"] =
                    "Une séance passée ne peut pas être supprimée, seulement archivée. (RG36)";
                return RedirectToAction(nameof(Index));
            }

            _db.Seances.Remove(seance);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Séance supprimée.";
            return RedirectToAction(nameof(Index));
        }

        // ── GET /Seances/CheckConflit ────────────────────────────────
        // Endpoint AJAX appelé par le JS de Create.cshtml
        // Remplace window.MockAPI.checkConflit()
        // Retourne JSON : { hasConflit, type, message }
        [HttpGet]
        public async Task<IActionResult> CheckConflit(
            string   date,
            string   heureDebut,
            string   heureFin,
            int      salleId,
            int      enseignantId,
            int?     seanceId = null)
        {
            if (!DateTime.TryParse(date,       out var dateParsed)  ||
                !TimeSpan.TryParse(heureDebut, out var debut)       ||
                !TimeSpan.TryParse(heureFin,   out var fin))
            {
                return Json(new { hasConflit = false });
            }

            int excludeId = seanceId ?? 0;

            // RG14 / RG34 — Conflit enseignant (Assigner)
            bool conflitEns = await _db.Seances.AnyAsync(s =>
                s.Id            != excludeId    &&
                s.EnseignantId  == enseignantId &&
                s.date_seance   == dateParsed   &&
                s.heure_debut   <  fin          &&
                s.heure_fin     >  debut);

            if (conflitEns)
                return Json(new
                {
                    hasConflit = true,
                    type       = "prof",
                    message    = "Cet enseignant est déjà assigné à une séance sur ce créneau. (RG14)"
                });

            // RG26 / RG34 — Conflit salle (Occuper)
            bool conflitSalle = await _db.Seances.AnyAsync(s =>
                s.Id          != excludeId &&
                s.SalleId     == salleId   &&
                s.date_seance == dateParsed &&
                s.heure_debut <  fin       &&
                s.heure_fin   >  debut);

            if (conflitSalle)
                return Json(new
                {
                    hasConflit = true,
                    type       = "salle",
                    message    = "La salle est déjà occupée sur ce créneau. (RG26)"
                });

            return Json(new { hasConflit = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetSeancesJson(int? groupeId, int? enseignantId)
        {
            var query = _db.Seances.AsQueryable();
            if (groupeId.HasValue)
                query = query.Where(s => s.GroupeId == groupeId.Value);
            if (enseignantId.HasValue)
                query = query.Where(s => s.EnseignantId == enseignantId.Value);

            var seances = await query
                .Select(s => new {
                    s.Id,
                    title = s.Matiere.intitule,
                    start = s.date_seance.Add(s.heure_debut),
                    end = s.date_seance.Add(s.heure_fin),
                    salle = s.Salle.num_salle,
                    professeur = s.Enseignant.prenom_enseignant + " " + s.Enseignant.nom_enseignant,
                    groupe = s.Groupe.nom_groupe,
                    mention = s.Groupe.Parcours.Cycle.Mention.nom_mention
                })
                .ToListAsync();

            return Json(seances);
        }

        // ── GET /Seances/GetEvents ───────────────────────────────────
        // API JSON pour FullCalendar
        [HttpGet]
        public async Task<IActionResult> GetEvents(
            int? mentionId = null,
            int? parcoursId = null,
            int? enseignantId = null,
            int? groupeId = null)
        {
            var query = _db.Seances
                .Include(s => s.Matiere)
                .Include(s => s.Salle)
                .Include(s => s.Enseignant)
                .Include(s => s.Groupe)
                    .ThenInclude(g => g.Parcours)
                        .ThenInclude(p => p.Cycle)
                            .ThenInclude(c => c.Mention)
                .AsQueryable();

            if (mentionId.HasValue)
                query = query.Where(s => s.Groupe.Parcours.Cycle.MentionId == mentionId);

            if (parcoursId.HasValue)
                query = query.Where(s => s.Groupe.ParcoursId == parcoursId);

            if (enseignantId.HasValue)
                query = query.Where(s => s.EnseignantId == enseignantId);

            if (groupeId.HasValue)
                query = query.Where(s => s.GroupeId == groupeId);

            var seances = await query.ToListAsync();

            // Fonction de couleur selon la matière
            string GetCouleur(string? matiere)
            {
                return matiere?.ToUpper() switch
                {
                    var s when s?.Contains("CM") == true || s?.Contains("COURS") == true => "#73B9E6",
                    var s when s?.Contains("TD") == true => "#34D399",
                    var s when s?.Contains("TP") == true => "#FBBF24",
                    var s when s?.Contains("EXAM") == true => "#F87171",
                    _ => "#94A3B8"
                };
            }

            var events = seances.Select(s => new
            {
                id = s.Id,
                title = s.Matiere?.intitule ?? "Séance",
                start = $"{s.date_seance:yyyy-MM-dd}T{s.heure_debut:hh\\:mm\\:ss}",
                end = $"{s.date_seance:yyyy-MM-dd}T{s.heure_fin:hh\\:mm\\:ss}",
                backgroundColor = GetCouleur(s.Matiere?.intitule),
                borderColor = GetCouleur(s.Matiere?.intitule),
                extendedProps = new
                {
                    enseignant = s.Enseignant != null ? $"{s.Enseignant.prenom_enseignant} {s.Enseignant.nom_enseignant}" : "—",
                    salle = s.Salle?.num_salle ?? "—",
                    groupe = s.Groupe?.nom_groupe ?? "—",
                    mention = s.Groupe?.Parcours?.Cycle?.Mention?.nom_mention ?? "—",
                    parcours = s.Groupe?.Parcours?.nom_parcours ?? "—",
                    semaine = s.semaine
                }
            });

            return Json(events);
        }

        // ── GET /Seances/GetSalles ───────────────────────────────────
        // API JSON — alimente le <select> Salle dans Create.cshtml
        [HttpGet]
        public async Task<IActionResult> GetSalles()
        {
            var salles = await _db.Salles
                .Include(s => s.Batiment)
                .OrderBy(s => s.num_salle)
                .Select(s => new
                {
                    id       = s.Id,
                    nom      = s.num_salle,
                    capacite = s.capacite,
                    batiment = s.Batiment.nom_batiment
                })
                .ToListAsync();

            return Json(salles);
        }

        // ── GET /Seances/GetGroupes ──────────────────────────────────
        // API JSON — alimente le <select> Groupe dans Create.cshtml
        [HttpGet]
        public async Task<IActionResult> GetGroupes(int? parcoursId = null)
        {
            var query = _db.Groupes
                .Include(g => g.Parcours).ThenInclude(p => p.Cycle).ThenInclude(c => c.Mention)
                .AsQueryable();

            if (parcoursId.HasValue)
                query = query.Where(g => g.ParcoursId == parcoursId);

            var groupes = await query
                .OrderBy(g => g.nom_groupe)
                .Select(g => new
                {
                    id          = g.Id,
                    nom         = g.nom_groupe,
                    nbEtudiant  = g.nb_etudiant,
                    parcours    = g.Parcours.nom_parcours,
                    mention     = g.Parcours.Cycle.Mention.nom_mention
                })
                .ToListAsync();

            return Json(groupes);
        }

        // ═════════════════════════════════════════════════════════════
        //  MÉTHODES PRIVÉES
        // ═════════════════════════════════════════════════════════════

        // ── GET /Seances/GetEnseignants ──────────────────────────────
        // API JSON pour alimenter le select Enseignant dans Create.cshtml
        [HttpGet]
        public async Task<IActionResult> GetEnseignants()
        {
            var enseignants = await _db.Enseignants
                .OrderBy(e => e.nom_enseignant)
                .Select(e => new
                {
                    id = e.Id,
                    nom = e.nom_enseignant,
                    prenom = e.prenom_enseignant,
                    grade = e.grade
                })
                .ToListAsync();
            return Json(enseignants);
        }


        /// <summary>
        /// RG34 — Détecte les conflits enseignant, salle et groupe.
        /// Retourne null si aucun conflit, sinon le message.
        /// </summary>
        private async Task<string?> DetecterConflit(Seance seance, int excludeId = 0)
        {
            var debut = seance.heure_debut;
            var fin   = seance.heure_fin;
            var date  = seance.date_seance;

            // RG14 — Conflit enseignant
            bool conflitEns = await _db.Seances.AnyAsync(s =>
                s.Id           != excludeId        &&
                s.EnseignantId == seance.EnseignantId &&
                s.date_seance  == date             &&
                s.heure_debut  <  fin              &&
                s.heure_fin    >  debut);

            if (conflitEns)
                return "⚠ Conflit enseignant : cet enseignant a déjà une séance sur ce créneau. (RG14)";

            // RG26 — Conflit salle
            bool conflitSalle = await _db.Seances.AnyAsync(s =>
                s.Id          != excludeId     &&
                s.SalleId     == seance.SalleId &&
                s.date_seance == date           &&
                s.heure_debut <  fin            &&
                s.heure_fin   >  debut);

            if (conflitSalle)
                return "⚠ Conflit salle : la salle est déjà réservée sur ce créneau. (RG26)";

            // RG11 — Conflit groupe
            bool conflitGroupe = await _db.Seances.AnyAsync(s =>
                s.Id          != excludeId      &&
                s.GroupeId    == seance.GroupeId &&
                s.date_seance == date            &&
                s.heure_debut <  fin             &&
                s.heure_fin   >  debut);

            if (conflitGroupe)
                return "⚠ Conflit groupe : ce groupe a déjà une séance sur ce créneau. (RG11)";

            return null;
        }

        /// <summary>
        /// Charge les SelectList nécessaires à la vue Create/Edit.
        /// </summary>
        private async Task ChargerSelectLists(Seance? seance = null)
        {
            ViewData["MatiereId"]    = new SelectList(
                await _db.Matieres.OrderBy(m => m.intitule).ToListAsync(),
                "Id", "intitule", seance?.MatiereId);

            ViewData["SalleId"]      = new SelectList(
                await _db.Salles.OrderBy(s => s.num_salle).ToListAsync(),
                "Id", "num_salle", seance?.SalleId);

            ViewData["EnseignantId"] = new SelectList(
                await _db.Enseignants
                    .OrderBy(e => e.nom_enseignant)
                    .Select(e => new
                    {
                        e.Id,
                        NomComplet = e.prenom_enseignant + " " + e.nom_enseignant
                    })
                    .ToListAsync(),
                "Id", "NomComplet", seance?.EnseignantId);

            ViewData["GroupeId"]     = new SelectList(
                await _db.Groupes
                    .Include(g => g.Parcours).ThenInclude(p => p.Cycle)
                    .OrderBy(g => g.nom_groupe)
                    .ToListAsync(),
                "Id", "nom_groupe", seance?.GroupeId);
        }

        // ── GET /Seances/GetMatieres ──────────────────────────────
        // API JSON — alimente le <select> Matière dans Create.cshtml
        [HttpGet]
        public async Task<IActionResult> GetMatieres()
        {
            var matieres = await _db.Matieres
                .OrderBy(m => m.intitule)
                .Select(m => new
                {
                    id = m.Id,
                    intitule = m.intitule,
                    code = m.code_mat
                })
                .ToListAsync();
            return Json(matieres);
        }

    }
}

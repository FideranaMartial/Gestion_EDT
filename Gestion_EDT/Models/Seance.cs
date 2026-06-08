using System.ComponentModel.DataAnnotations;

namespace Gestion_EDT.Models
{
    public class Seance
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Date")]
        public DateTime date_seance { get; set; }

        [Required]
        [Display(Name = "Heure de début")]
        public TimeSpan heure_debut { get; set; }

        [Required]
        [Display(Name = "Heure de fin")]
        public TimeSpan heure_fin { get; set; }

        // RG31 — semaine de l'année universitaire [1..52]
        [Range(1, 52, ErrorMessage = "La semaine doit être entre 1 et 52.")]
        [Display(Name = "Semaine")]
        public int semaine { get; set; }

        // ── Clés étrangères ───────────────────────────────────────
        public int MatiereId    { get; set; }
        public int SalleId      { get; set; }
        public int EnseignantId { get; set; }
        public int GroupeId     { get; set; }

        // ── Navigation ────────────────────────────────────────────
        public Matiere?    Matiere    { get; set; }
        public Salle?      Salle      { get; set; }
        public Enseignant? Enseignant { get; set; }
        public Groupe?     Groupe     { get; set; }
    }
}

namespace Gestion_EDT.Models
{
    public class Seance
    {
        public int Id { get; set; }
        public DateTime date_seance { get; set; }
        public TimeSpan heure_debut { get; set; }
        public TimeSpan heure_fin { get; set; }

        public int MatiereId { get; set; }
        public Matiere Matiere { get; set; }

        public int SalleId { get; set; }
        public Salle Salle { get; set; }

        public int EnseignantId { get; set; }
        public Enseignant Enseignant { get; set; }

        public int GroupeId { get; set; }
        public Groupe Groupe { get; set; }
    }
}
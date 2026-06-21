namespace Gestion_EDT.Models
{
    public class Parcours
    {
        public int Id { get; set; }
        public string nom_parcours { get; set; }

        public int CycleId { get; set; }
        public Cycle Cycle { get; set; }

        public ICollection<Groupe> Groupes { get; set; }
        public ICollection<Matiere> Matieres { get; set; } = new List<Matiere>();
        public ICollection<ParcoursMatiere> ParcoursMatieres { get; set; } = new List<ParcoursMatiere>();
}
}
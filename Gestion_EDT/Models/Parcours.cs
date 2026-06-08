namespace Gestion_EDT.Models
{
    public class Parcours
    {
        public int Id { get; set; }
        public string nom_parcours { get; set; }

        public int CycleId { get; set; }
        public Cycle Cycle { get; set; }

        public ICollection<Groupe> Groupes { get; set; }
}
}
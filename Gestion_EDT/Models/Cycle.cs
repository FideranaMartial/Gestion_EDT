namespace Gestion_EDT.Models
{
    public class Cycle
    {
        public int Id { get; set; }
        public string nom_cycle { get; set; }
        public string niveau { get; set; }

        public int MentionId { get; set; }
        public Mention Mention { get; set; }

        public ICollection<Parcours> Parcours { get; set; }
}
}
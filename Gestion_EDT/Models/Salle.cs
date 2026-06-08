namespace Gestion_EDT.Models
{
    public class Salle
    {
        public int Id { get; set; }
        public string num_salle { get; set; }
        public int capacite { get; set; }

        public int BatimentId { get; set; }
        public Batiment Batiment { get; set; }

        public ICollection<Seance> Seances { get; set; }
}
}
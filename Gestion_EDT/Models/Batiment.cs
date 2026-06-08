namespace Gestion_EDT.Models
{
    public class Batiment
    {
        public int Id { get; set; }
        public string nom_batiment { get; set; }

        public ICollection<Salle> Salles { get; set; }
}
}
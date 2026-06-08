namespace Gestion_EDT.Models
{
    public class Enseignant
    {
        public int Id { get; set; }
        public string matricule { get; set; }
        public string nom_enseignant { get; set; }
        public string prenom_enseignant { get; set; }
        public string grade { get; set; }

        public ICollection<Seance> Seances { get; set; }
}
}
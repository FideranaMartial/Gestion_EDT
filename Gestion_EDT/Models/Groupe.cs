namespace Gestion_EDT.Models
{
    public class Groupe
    {
        public int Id { get; set; }
        public string code_groupe { get; set; }
        public int nb_etudiant { get; set; }
        public string nom_groupe { get; set; }

        public int ParcoursId { get; set; }
        public Parcours Parcours { get; set; }

        public ICollection<Seance> Seances { get; set; }
}
}
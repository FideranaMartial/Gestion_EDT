namespace Gestion_EDT.Models
{
    public class Matiere
    {
        public int Id { get; set; }
        public string code_mat { get; set; }
        public string intitule { get; set; }
        public int nb_heure { get; set; }

        public ICollection<Seance> Seances { get; set; }
        public ICollection<Parcours> Parcours { get; set; } = new List<Parcours>();
        public ICollection<ParcoursMatiere> ParcoursMatieres { get; set; } = new List<ParcoursMatiere>();
}
}
using System.ComponentModel.DataAnnotations;

namespace Gestion_EDT.Models
{
    public class Enseignant
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le matricule est obligatoire.")]
        public string matricule { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        public string nom_enseignant { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est obligatoire.")]
        public string prenom_enseignant { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le grade est obligatoire.")]
        public string grade { get; set; } = string.Empty;

        public ICollection<Seance> Seances { get; set; } = new List<Seance>();
    }
}
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_EDT.Models
{
    [Table("parcours_matieres")] // nom exact de la table
    public class ParcoursMatiere
    {
        [Column("id_parcours")]
        public int ParcoursId { get; set; }
        public Parcours Parcours { get; set; }

        [Column("id_matiere")]
        public int MatiereId { get; set; }
        public Matiere Matiere { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_EDT.Models
{
    /// <summary>
    /// Compte utilisateur de l'application.
    /// Rôles possibles : "Admin", "RespEDT", "Enseignant" (RG38).
    ///
    /// - Admin     : gère mentions, cycles, salles, enseignants, matières (RG01-RG27)
    /// - RespEDT   : crée/modifie/supprime les séances (RG28-RG38)
    /// - Enseignant: consulte uniquement son propre planning (T14, RG13)
    /// </summary>
    public class Utilisateur
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "L'identifiant est obligatoire.")]
        [StringLength(50)]
        [Display(Name = "Identifiant")]
        public string Login { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Rôle")]
        public string Role { get; set; } = "Enseignant"; // Admin | RespEDT | Enseignant

        [StringLength(100)]
        [Display(Name = "Nom complet")]
        public string? NomComplet { get; set; }

        // Si le compte correspond à un enseignant, on le relie ici
        // pour filtrer automatiquement son planning personnel (T14).
        public int? EnseignantId { get; set; }

        [ForeignKey("EnseignantId")]
        public Enseignant? Enseignant { get; set; }

        public bool Actif { get; set; } = true;
    }
}

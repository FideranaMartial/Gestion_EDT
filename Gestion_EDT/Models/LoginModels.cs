using System.ComponentModel.DataAnnotations;

namespace Gestion_EDT.Models
{
    // ── Formulaire de connexion ──────────────────────────────────────
    public class LoginViewModel
    {
        [Required(ErrorMessage = "L'identifiant est obligatoire.")]
        [Display(Name = "Identifiant")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Se souvenir de moi")]
        public bool RememberMe { get; set; }

        // URL de retour après connexion
        public string? ReturnUrl { get; set; }
    }

    // ── Formulaire de création / modification d'un compte (Admin) ───
    public class UtilisateurFormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "L'identifiant est obligatoire.")]
        [StringLength(50)]
        [Display(Name = "Identifiant")]
        public string Login { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        [StringLength(100, MinimumLength = 6,
            ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères.")]
        public string? Password { get; set; }

        [Required]
        [Display(Name = "Rôle")]
        public string Role { get; set; } = "Enseignant";

        [StringLength(100)]
        [Display(Name = "Nom complet")]
        public string? NomComplet { get; set; }

        [Display(Name = "Lier à un enseignant")]
        public int? EnseignantId { get; set; }

        [Display(Name = "Compte actif")]
        public bool Actif { get; set; } = true;
    }

    // ── Changement de mot de passe par l'utilisateur connecté ────────
    public class ChangePasswordFormModel
    {
        [Required(ErrorMessage = "Le mot de passe actuel est obligatoire.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe actuel")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nouveau mot de passe est obligatoire.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6,
            ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères.")]
        [Display(Name = "Nouveau mot de passe")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Veuillez confirmer le mot de passe.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas.")]
        [Display(Name = "Confirmer le mot de passe")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

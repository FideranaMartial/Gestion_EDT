using Microsoft.EntityFrameworkCore;
using Gestion_EDT.Models;

// CORRECTION : le DbContext doit être dans le namespace Data (ou au moins pas dans Models)
namespace Gestion_EDT.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // ── Tables ──────────────────────────────────────────────────
        public DbSet<Batiment>   Batiments   { get; set; }
        public DbSet<Salle>      Salles      { get; set; }
        public DbSet<Matiere>    Matieres    { get; set; }
        public DbSet<Seance>     Seances     { get; set; }
        public DbSet<Enseignant> Enseignants { get; set; }
        public DbSet<Mention>    Mentions    { get; set; }
        public DbSet<Cycle>      Cycles      { get; set; }
        public DbSet<Parcours>   Parcours    { get; set; }
        public DbSet<Groupe>     Groupes     { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Contraintes métier ──────────────────────────────────

            // RG30 : heure_fin > heure_debut (vérifiée aussi en contrôleur)
            // RG31 : semaine ∈ [1..52]
            modelBuilder.Entity<Seance>()
                .ToTable(t => t.HasCheckConstraint(
                    "CHK_Seance_Heures",
                    "heure_fin > heure_debut"));

            modelBuilder.Entity<Seance>()
                .ToTable(t => t.HasCheckConstraint(
                    "CHK_Seance_Semaine",
                    "semaine BETWEEN 1 AND 52"));

            // RG09 : nb_etudiant > 0
            modelBuilder.Entity<Groupe>()
                .ToTable(t => t.HasCheckConstraint(
                    "CHK_Groupe_NbEtudiant",
                    "nb_etudiant > 0"));

            // RG24 : capacite > 0
            modelBuilder.Entity<Salle>()
                .ToTable(t => t.HasCheckConstraint(
                    "CHK_Salle_Capacite",
                    "capacite > 0"));

            // ── Index pour la détection de conflits (RG34) ──────────

            // Conflit enseignant : même enseignant, même date
            modelBuilder.Entity<Seance>()
                .HasIndex(s => new { s.EnseignantId, s.date_seance })
                .HasDatabaseName("IDX_Seance_Enseignant_Date");

            // Conflit salle : même salle, même date
            modelBuilder.Entity<Seance>()
                .HasIndex(s => new { s.SalleId, s.date_seance })
                .HasDatabaseName("IDX_Seance_Salle_Date");

            // Recherche par semaine
            modelBuilder.Entity<Seance>()
                .HasIndex(s => s.semaine)
                .HasDatabaseName("IDX_Seance_Semaine");

            // ── Données initiales (Seed) ────────────────────────────

            // Mentions EMIT (RG01)
            modelBuilder.Entity<Mention>().HasData(
                new Mention { Id = 1, code_mention = "INFO", nom_mention = "Informatique" },
                new Mention { Id = 2, code_mention = "MGT",  nom_mention = "Management" },
                new Mention { Id = 3, code_mention = "ICM",  nom_mention = "Information, Communication et Multimédia" }
            );

            // Cycles : Licence (L1→L3) et Master (M1→M2) pour chaque mention (RG03)
            modelBuilder.Entity<Cycle>().HasData(
                new Cycle { Id = 1, nom_cycle = "Licence Informatique", niveau = "L", MentionId = 1 },
                new Cycle { Id = 2, nom_cycle = "Master Informatique",  niveau = "M", MentionId = 1 },
                new Cycle { Id = 3, nom_cycle = "Licence Management",   niveau = "L", MentionId = 2 },
                new Cycle { Id = 4, nom_cycle = "Master Management",    niveau = "M", MentionId = 2 },
                new Cycle { Id = 5, nom_cycle = "Licence ICM",          niveau = "L", MentionId = 3 },
                new Cycle { Id = 6, nom_cycle = "Master ICM",           niveau = "M", MentionId = 3 }
            );

            // Parcours (niveaux L1→L3 / M1→M2) (RG04)
            int pid = 1;
            var parcoursData = new List<Parcours>();
            foreach (var (cycleId, niveaux) in new[]
            {
                (1, new[]{"L1","L2","L3"}),
                (2, new[]{"M1","M2"}),
                (3, new[]{"L1","L2","L3"}),
                (4, new[]{"M1","M2"}),
                (5, new[]{"L1","L2","L3"}),
                (6, new[]{"M1","M2"}),
            })
            {
                foreach (var n in niveaux)
                    parcoursData.Add(new Parcours
                    {
                        Id          = pid++,
                        nom_parcours = n,
                        CycleId     = cycleId
                    });
            }
            modelBuilder.Entity<Parcours>().HasData(parcoursData);

            // Bâtiments
            modelBuilder.Entity<Batiment>().HasData(
                new Batiment { Id = 1, nom_batiment = "Bâtiment A" },
                new Batiment { Id = 2, nom_batiment = "Bâtiment B" },
                new Batiment { Id = 3, nom_batiment = "Bâtiment C" }
            );

            // Salles
            modelBuilder.Entity<Salle>().HasData(
                new Salle { Id = 1, num_salle = "A101", capacite = 40,  BatimentId = 1 },
                new Salle { Id = 2, num_salle = "A102", capacite = 40,  BatimentId = 1 },
                new Salle { Id = 3, num_salle = "LABO", capacite = 30,  BatimentId = 2 },
                new Salle { Id = 4, num_salle = "AMP1", capacite = 200, BatimentId = 3 }
            );
        }
    }
}

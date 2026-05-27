using Microsoft.EntityFrameworkCore;

namespace Gestion_EDT.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<DbContext> options) : base(options)
        {
        }

        public DbSet<Batiment> Batiments { get; set; }
    public DbSet<Salle> Salles { get; set; }
public DbSet< Matiere > Matieres { get; set; }
public DbSet< Seance > Seances { get; set; }
public DbSet< Enseignant > Enseignants { get; set; }
public DbSet< Mention > Mentions { get; set; }
public DbSet< Cycle > Cycles { get; set; }
public DbSet< Parcours > Parcours { get; set; }
public DbSet< Groupe > Groupes { get; set; }
    }
}
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gestion_EDT.Migrations
{
    /// <inheritdoc />
    public partial class AjoutColonneSemaine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ajouter la colonne semaine à la table Seances existante
            migrationBuilder.AddColumn<int>(
                name: "semaine",
                table: "Seances",
                type: "int",
                nullable: false,
                defaultValue: 1);  // Valeur par défaut pour les lignes existantes

            // Ajouter les index manquants si nécessaire
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IDX_Seance_Semaine' AND object_id = OBJECT_ID('Seances'))
                BEGIN
                    CREATE INDEX IDX_Seance_Semaine ON Seances(semaine);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Supprimer la colonne semaine
            migrationBuilder.DropColumn(
                name: "semaine",
                table: "Seances");
        }
    }
}
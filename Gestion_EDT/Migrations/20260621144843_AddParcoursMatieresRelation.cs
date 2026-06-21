using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gestion_EDT.Migrations
{
    /// <inheritdoc />
    public partial class AddParcoursMatieresRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "parcours_matieres",
                columns: table => new
                {
                    id_parcours = table.Column<int>(nullable: false),
                    id_matiere = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parcours_matieres", x => new { x.id_parcours, x.id_matiere });
                    table.ForeignKey(
                        name: "FK_parcours_matieres_Matieres_id_matiere",
                        column: x => x.id_matiere,
                        principalTable: "Matieres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_parcours_matieres_Parcours_id_parcours",
                        column: x => x.id_parcours,
                        principalTable: "Parcours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "parcours_matieres");
        }
    }
}
using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Gestion_EDT.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Batiments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nom_batiment = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batiments", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Enseignants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    matricule = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nom_enseignant = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    prenom_enseignant = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    grade = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enseignants", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Matieres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    code_mat = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    intitule = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nb_heure = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matieres", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Mentions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    code_mention = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nom_mention = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mentions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Salles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    num_salle = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    capacite = table.Column<int>(type: "int", nullable: false),
                    BatimentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salles", x => x.Id);
                    table.CheckConstraint("CHK_Salle_Capacite", "capacite > 0");
                    table.ForeignKey(
                        name: "FK_Salles_Batiments_BatimentId",
                        column: x => x.BatimentId,
                        principalTable: "Batiments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Cycles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nom_cycle = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    niveau = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MentionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cycles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cycles_Mentions_MentionId",
                        column: x => x.MentionId,
                        principalTable: "Mentions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Parcours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nom_parcours = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CycleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parcours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parcours_Cycles_CycleId",
                        column: x => x.CycleId,
                        principalTable: "Cycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Groupes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    code_groupe = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nb_etudiant = table.Column<int>(type: "int", nullable: false),
                    nom_groupe = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParcoursId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groupes", x => x.Id);
                    table.CheckConstraint("CHK_Groupe_NbEtudiant", "nb_etudiant > 0");
                    table.ForeignKey(
                        name: "FK_Groupes_Parcours_ParcoursId",
                        column: x => x.ParcoursId,
                        principalTable: "Parcours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Seances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    date_seance = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    heure_debut = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    heure_fin = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    semaine = table.Column<int>(type: "int", nullable: false),
                    MatiereId = table.Column<int>(type: "int", nullable: false),
                    SalleId = table.Column<int>(type: "int", nullable: false),
                    EnseignantId = table.Column<int>(type: "int", nullable: false),
                    GroupeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seances", x => x.Id);
                    table.CheckConstraint("CHK_Seance_Heures", "heure_fin > heure_debut");
                    table.CheckConstraint("CHK_Seance_Semaine", "semaine BETWEEN 1 AND 52");
                    table.ForeignKey(
                        name: "FK_Seances_Enseignants_EnseignantId",
                        column: x => x.EnseignantId,
                        principalTable: "Enseignants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Seances_Groupes_GroupeId",
                        column: x => x.GroupeId,
                        principalTable: "Groupes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Seances_Matieres_MatiereId",
                        column: x => x.MatiereId,
                        principalTable: "Matieres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Seances_Salles_SalleId",
                        column: x => x.SalleId,
                        principalTable: "Salles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Batiments",
                columns: new[] { "Id", "nom_batiment" },
                values: new object[,]
                {
                    { 1, "Bâtiment A" },
                    { 2, "Bâtiment B" },
                    { 3, "Bâtiment C" }
                });

            migrationBuilder.InsertData(
                table: "Mentions",
                columns: new[] { "Id", "code_mention", "nom_mention" },
                values: new object[,]
                {
                    { 1, "INFO", "Informatique" },
                    { 2, "MGT", "Management" },
                    { 3, "ICM", "Information, Communication et Multimédia" }
                });

            migrationBuilder.InsertData(
                table: "Cycles",
                columns: new[] { "Id", "MentionId", "niveau", "nom_cycle" },
                values: new object[,]
                {
                    { 1, 1, "L", "Licence Informatique" },
                    { 2, 1, "M", "Master Informatique" },
                    { 3, 2, "L", "Licence Management" },
                    { 4, 2, "M", "Master Management" },
                    { 5, 3, "L", "Licence ICM" },
                    { 6, 3, "M", "Master ICM" }
                });

            migrationBuilder.InsertData(
                table: "Salles",
                columns: new[] { "Id", "BatimentId", "capacite", "num_salle" },
                values: new object[,]
                {
                    { 1, 1, 40, "A101" },
                    { 2, 1, 40, "A102" },
                    { 3, 2, 30, "LABO" },
                    { 4, 3, 200, "AMP1" }
                });

            migrationBuilder.InsertData(
                table: "Parcours",
                columns: new[] { "Id", "CycleId", "nom_parcours" },
                values: new object[,]
                {
                    { 1, 1, "L1" },
                    { 2, 1, "L2" },
                    { 3, 1, "L3" },
                    { 4, 2, "M1" },
                    { 5, 2, "M2" },
                    { 6, 3, "L1" },
                    { 7, 3, "L2" },
                    { 8, 3, "L3" },
                    { 9, 4, "M1" },
                    { 10, 4, "M2" },
                    { 11, 5, "L1" },
                    { 12, 5, "L2" },
                    { 13, 5, "L3" },
                    { 14, 6, "M1" },
                    { 15, 6, "M2" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cycles_MentionId",
                table: "Cycles",
                column: "MentionId");

            migrationBuilder.CreateIndex(
                name: "IX_Groupes_ParcoursId",
                table: "Groupes",
                column: "ParcoursId");

            migrationBuilder.CreateIndex(
                name: "IX_Parcours_CycleId",
                table: "Parcours",
                column: "CycleId");

            migrationBuilder.CreateIndex(
                name: "IX_Salles_BatimentId",
                table: "Salles",
                column: "BatimentId");

            migrationBuilder.CreateIndex(
                name: "IDX_Seance_Enseignant_Date",
                table: "Seances",
                columns: new[] { "EnseignantId", "date_seance" });

            migrationBuilder.CreateIndex(
                name: "IDX_Seance_Salle_Date",
                table: "Seances",
                columns: new[] { "SalleId", "date_seance" });

            migrationBuilder.CreateIndex(
                name: "IDX_Seance_Semaine",
                table: "Seances",
                column: "semaine");

            migrationBuilder.CreateIndex(
                name: "IX_Seances_GroupeId",
                table: "Seances",
                column: "GroupeId");

            migrationBuilder.CreateIndex(
                name: "IX_Seances_MatiereId",
                table: "Seances",
                column: "MatiereId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Seances");

            migrationBuilder.DropTable(
                name: "Enseignants");

            migrationBuilder.DropTable(
                name: "Groupes");

            migrationBuilder.DropTable(
                name: "Matieres");

            migrationBuilder.DropTable(
                name: "Salles");

            migrationBuilder.DropTable(
                name: "Parcours");

            migrationBuilder.DropTable(
                name: "Batiments");

            migrationBuilder.DropTable(
                name: "Cycles");

            migrationBuilder.DropTable(
                name: "Mentions");
        }
    }
}

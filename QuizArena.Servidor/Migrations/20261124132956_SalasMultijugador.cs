using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizArena.Servidor.Migrations
{
    /// <inheritdoc />
    public partial class SalasMultijugador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Salas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    EsPrivada = table.Column<bool>(type: "INTEGER", nullable: false),
                    Modalidad = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    MaxJugadores = table.Column<int>(type: "INTEGER", nullable: false),
                    NumeroPreguntas = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoriaId = table.Column<int>(type: "INTEGER", nullable: true),
                    Dificultad = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    CreadorId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreadaUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Salas_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Salas_Jugadores_CreadorId",
                        column: x => x.CreadorId,
                        principalTable: "Jugadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParticipantesSala",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SalaId = table.Column<int>(type: "INTEGER", nullable: false),
                    JugadorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Preparado = table.Column<bool>(type: "INTEGER", nullable: false),
                    UnidoUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantesSala", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParticipantesSala_Jugadores_JugadorId",
                        column: x => x.JugadorId,
                        principalTable: "Jugadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParticipantesSala_Salas_SalaId",
                        column: x => x.SalaId,
                        principalTable: "Salas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantesSala_JugadorId",
                table: "ParticipantesSala",
                column: "JugadorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantesSala_SalaId_JugadorId",
                table: "ParticipantesSala",
                columns: new[] { "SalaId", "JugadorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Salas_CategoriaId",
                table: "Salas",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Salas_Codigo",
                table: "Salas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Salas_CreadorId",
                table: "Salas",
                column: "CreadorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParticipantesSala");

            migrationBuilder.DropTable(
                name: "Salas");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizArena.Servidor.Migrations
{
    /// <inheritdoc />
    public partial class ChatSalas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MensajesSala",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SalaId = table.Column<int>(type: "INTEGER", nullable: false),
                    JugadorId = table.Column<int>(type: "INTEGER", nullable: false),
                    EquipoPartidaId = table.Column<int>(type: "INTEGER", nullable: true),
                    Texto = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    FechaUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MensajesSala", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MensajesSala_EquiposPartida_EquipoPartidaId",
                        column: x => x.EquipoPartidaId,
                        principalTable: "EquiposPartida",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MensajesSala_Jugadores_JugadorId",
                        column: x => x.JugadorId,
                        principalTable: "Jugadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MensajesSala_Salas_SalaId",
                        column: x => x.SalaId,
                        principalTable: "Salas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MensajesSala_EquipoPartidaId",
                table: "MensajesSala",
                column: "EquipoPartidaId");

            migrationBuilder.CreateIndex(
                name: "IX_MensajesSala_JugadorId",
                table: "MensajesSala",
                column: "JugadorId");

            migrationBuilder.CreateIndex(
                name: "IX_MensajesSala_SalaId_Id",
                table: "MensajesSala",
                columns: new[] { "SalaId", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MensajesSala");
        }
    }
}

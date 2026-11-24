using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizArena.Servidor.Migrations
{
    /// <inheritdoc />
    public partial class EquiposYVotaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EquipoPartidaId",
                table: "PartidasJugadores",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EquiposPartida",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartidaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Puntos = table.Column<int>(type: "INTEGER", nullable: false),
                    Racha = table.Column<int>(type: "INTEGER", nullable: false),
                    Aciertos = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquiposPartida", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquiposPartida_Partidas_PartidaId",
                        column: x => x.PartidaId,
                        principalTable: "Partidas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VotosEquipo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartidaPreguntaId = table.Column<int>(type: "INTEGER", nullable: false),
                    JugadorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Opcion = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false),
                    FechaUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VotosEquipo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VotosEquipo_Jugadores_JugadorId",
                        column: x => x.JugadorId,
                        principalTable: "Jugadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VotosEquipo_PartidasPreguntas_PartidaPreguntaId",
                        column: x => x.PartidaPreguntaId,
                        principalTable: "PartidasPreguntas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RespuestasEquipo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartidaPreguntaId = table.Column<int>(type: "INTEGER", nullable: false),
                    EquipoPartidaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Opcion = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false),
                    EsCorrecta = table.Column<bool>(type: "INTEGER", nullable: false),
                    PuntosGanados = table.Column<int>(type: "INTEGER", nullable: false),
                    ConfirmadaUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespuestasEquipo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RespuestasEquipo_EquiposPartida_EquipoPartidaId",
                        column: x => x.EquipoPartidaId,
                        principalTable: "EquiposPartida",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RespuestasEquipo_PartidasPreguntas_PartidaPreguntaId",
                        column: x => x.PartidaPreguntaId,
                        principalTable: "PartidasPreguntas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartidasJugadores_EquipoPartidaId",
                table: "PartidasJugadores",
                column: "EquipoPartidaId");

            migrationBuilder.CreateIndex(
                name: "IX_EquiposPartida_PartidaId_Nombre",
                table: "EquiposPartida",
                columns: new[] { "PartidaId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RespuestasEquipo_EquipoPartidaId",
                table: "RespuestasEquipo",
                column: "EquipoPartidaId");

            migrationBuilder.CreateIndex(
                name: "IX_RespuestasEquipo_PartidaPreguntaId_EquipoPartidaId",
                table: "RespuestasEquipo",
                columns: new[] { "PartidaPreguntaId", "EquipoPartidaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VotosEquipo_JugadorId",
                table: "VotosEquipo",
                column: "JugadorId");

            migrationBuilder.CreateIndex(
                name: "IX_VotosEquipo_PartidaPreguntaId_JugadorId",
                table: "VotosEquipo",
                columns: new[] { "PartidaPreguntaId", "JugadorId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PartidasJugadores_EquiposPartida_EquipoPartidaId",
                table: "PartidasJugadores",
                column: "EquipoPartidaId",
                principalTable: "EquiposPartida",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartidasJugadores_EquiposPartida_EquipoPartidaId",
                table: "PartidasJugadores");

            migrationBuilder.DropTable(
                name: "RespuestasEquipo");

            migrationBuilder.DropTable(
                name: "VotosEquipo");

            migrationBuilder.DropTable(
                name: "EquiposPartida");

            migrationBuilder.DropIndex(
                name: "IX_PartidasJugadores_EquipoPartidaId",
                table: "PartidasJugadores");

            migrationBuilder.DropColumn(
                name: "EquipoPartidaId",
                table: "PartidasJugadores");
        }
    }
}

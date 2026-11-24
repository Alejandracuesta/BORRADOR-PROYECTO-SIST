using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizArena.Servidor.Migrations
{
    /// <inheritdoc />
    public partial class MotorPartidasIndividuales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Partidas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SalaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Fase = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    IndicePregunta = table.Column<int>(type: "INTEGER", nullable: false),
                    CreadaUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FinFaseUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partidas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Partidas_Salas_SalaId",
                        column: x => x.SalaId,
                        principalTable: "Salas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PartidasJugadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartidaId = table.Column<int>(type: "INTEGER", nullable: false),
                    JugadorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Puntos = table.Column<int>(type: "INTEGER", nullable: false),
                    Racha = table.Column<int>(type: "INTEGER", nullable: false),
                    Aciertos = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartidasJugadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartidasJugadores_Jugadores_JugadorId",
                        column: x => x.JugadorId,
                        principalTable: "Jugadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartidasJugadores_Partidas_PartidaId",
                        column: x => x.PartidaId,
                        principalTable: "Partidas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartidasPreguntas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartidaId = table.Column<int>(type: "INTEGER", nullable: false),
                    PreguntaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Orden = table.Column<int>(type: "INTEGER", nullable: false),
                    AperturaUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CierreUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartidasPreguntas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartidasPreguntas_Partidas_PartidaId",
                        column: x => x.PartidaId,
                        principalTable: "Partidas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartidasPreguntas_Preguntas_PreguntaId",
                        column: x => x.PreguntaId,
                        principalTable: "Preguntas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RespuestasPartida",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartidaPreguntaId = table.Column<int>(type: "INTEGER", nullable: false),
                    JugadorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Opcion = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false),
                    EsCorrecta = table.Column<bool>(type: "INTEGER", nullable: false),
                    PuntosGanados = table.Column<int>(type: "INTEGER", nullable: false),
                    EnviadaUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespuestasPartida", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RespuestasPartida_Jugadores_JugadorId",
                        column: x => x.JugadorId,
                        principalTable: "Jugadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RespuestasPartida_PartidasPreguntas_PartidaPreguntaId",
                        column: x => x.PartidaPreguntaId,
                        principalTable: "PartidasPreguntas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_SalaId",
                table: "Partidas",
                column: "SalaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartidasJugadores_JugadorId",
                table: "PartidasJugadores",
                column: "JugadorId");

            migrationBuilder.CreateIndex(
                name: "IX_PartidasJugadores_PartidaId_JugadorId",
                table: "PartidasJugadores",
                columns: new[] { "PartidaId", "JugadorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartidasPreguntas_PartidaId_Orden",
                table: "PartidasPreguntas",
                columns: new[] { "PartidaId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartidasPreguntas_PartidaId_PreguntaId",
                table: "PartidasPreguntas",
                columns: new[] { "PartidaId", "PreguntaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartidasPreguntas_PreguntaId",
                table: "PartidasPreguntas",
                column: "PreguntaId");

            migrationBuilder.CreateIndex(
                name: "IX_RespuestasPartida_JugadorId",
                table: "RespuestasPartida",
                column: "JugadorId");

            migrationBuilder.CreateIndex(
                name: "IX_RespuestasPartida_PartidaPreguntaId_JugadorId",
                table: "RespuestasPartida",
                columns: new[] { "PartidaPreguntaId", "JugadorId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartidasJugadores");

            migrationBuilder.DropTable(
                name: "RespuestasPartida");

            migrationBuilder.DropTable(
                name: "PartidasPreguntas");

            migrationBuilder.DropTable(
                name: "Partidas");
        }
    }
}

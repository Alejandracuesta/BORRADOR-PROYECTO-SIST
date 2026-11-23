using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizArena.Servidor.Migrations
{
    /// <inheritdoc />
    public partial class BancoPreguntas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    NombreNormalizado = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Preguntas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Enunciado = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    EnunciadoNormalizado = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    CategoriaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Dificultad = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    OpcionA = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    OpcionB = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    OpcionC = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    OpcionD = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    RespuestaCorrecta = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false),
                    Fuente = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Licencia = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preguntas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Preguntas_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_NombreNormalizado",
                table: "Categorias",
                column: "NombreNormalizado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Preguntas_CategoriaId",
                table: "Preguntas",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Preguntas_EnunciadoNormalizado",
                table: "Preguntas",
                column: "EnunciadoNormalizado",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Preguntas");

            migrationBuilder.DropTable(
                name: "Categorias");
        }
    }
}

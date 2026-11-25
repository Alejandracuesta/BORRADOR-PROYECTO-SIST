using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizArena.Servidor.Migrations
{
    /// <inheritdoc />
    public partial class RevanchaSalas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Partidas_SalaId",
                table: "Partidas");

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_SalaId",
                table: "Partidas",
                column: "SalaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Partidas_SalaId",
                table: "Partidas");

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_SalaId",
                table: "Partidas",
                column: "SalaId",
                unique: true);
        }
    }
}

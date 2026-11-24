using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizArena.Servidor.Migrations
{
    /// <inheritdoc />
    public partial class AccesoJugadores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Jugadores",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Jugadores");
        }
    }
}

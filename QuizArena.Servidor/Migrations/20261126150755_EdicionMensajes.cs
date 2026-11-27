using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizArena.Servidor.Migrations
{
    /// <inheritdoc />
    public partial class EdicionMensajes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EditadoUtc",
                table: "MensajesSala",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Eliminado",
                table: "MensajesSala",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditadoUtc",
                table: "MensajesSala");

            migrationBuilder.DropColumn(
                name: "Eliminado",
                table: "MensajesSala");
        }
    }
}

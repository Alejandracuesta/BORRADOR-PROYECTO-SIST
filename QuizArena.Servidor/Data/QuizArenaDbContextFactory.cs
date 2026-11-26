using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace QuizArena.Servidor.Data;

// Las herramientas de migración no arrancan el servidor ni aplican migraciones.
public sealed class QuizArenaDbContextFactory : IDesignTimeDbContextFactory<QuizArenaDbContext>
{
    public QuizArenaDbContext CreateDbContext(string[] args)
    {
        string path = Environment.GetEnvironmentVariable("BaseDatos__Ruta") ?? "quizarena.db";
        return new QuizArenaDbContext(new DbContextOptionsBuilder<QuizArenaDbContext>()
            .UseSqlite(new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = path }.ToString()).Options);
    }
}

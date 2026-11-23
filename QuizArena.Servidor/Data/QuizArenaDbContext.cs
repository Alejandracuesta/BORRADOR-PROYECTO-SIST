using Microsoft.EntityFrameworkCore;
using QuizArena.Servidor.Models;

namespace QuizArena.Servidor.Data;

public class QuizArenaDbContext : DbContext
{
    public QuizArenaDbContext(DbContextOptions<QuizArenaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Jugador> Jugadores => Set<Jugador>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var jugador = modelBuilder.Entity<Jugador>();

        jugador.Property(j => j.Nombre)
            .IsRequired()
            .HasMaxLength(20);

        jugador.Property(j => j.NombreNormalizado)
            .IsRequired()
            .HasMaxLength(20);

        // SQLite garantiza la unicidad incluso con peticiones simultáneas.
        jugador.HasIndex(j => j.NombreNormalizado)
            .IsUnique();

        // SQLite no conserva DateTime.Kind; la API siempre devuelve esta fecha en UTC.
        jugador.Property(j => j.FechaRegistroUtc)
            .HasConversion(
                fecha => fecha,
                fecha => DateTime.SpecifyKind(fecha, DateTimeKind.Utc));
    }
}

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
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Pregunta> Preguntas => Set<Pregunta>();

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

        // Una categoría puede contener muchas preguntas.
        var categoria = modelBuilder.Entity<Categoria>();

        categoria.Property(c => c.Nombre)
            .IsRequired()
            .HasMaxLength(60);

        categoria.Property(c => c.NombreNormalizado)
            .IsRequired()
            .HasMaxLength(60);

        categoria.HasIndex(c => c.NombreNormalizado)
            .IsUnique();

        var pregunta = modelBuilder.Entity<Pregunta>();

        pregunta.Property(p => p.Enunciado)
            .IsRequired()
            .HasMaxLength(300);

        pregunta.Property(p => p.EnunciadoNormalizado)
            .IsRequired()
            .HasMaxLength(300);

        pregunta.HasIndex(p => p.EnunciadoNormalizado)
            .IsUnique();

        pregunta.Property(p => p.Dificultad)
            .HasConversion<string>()
            .HasMaxLength(10);

        pregunta.Property(p => p.OpcionA).HasMaxLength(150);
        pregunta.Property(p => p.OpcionB).HasMaxLength(150);
        pregunta.Property(p => p.OpcionC).HasMaxLength(150);
        pregunta.Property(p => p.OpcionD).HasMaxLength(150);

        pregunta.Property(p => p.RespuestaCorrecta)
            .IsRequired()
            .HasMaxLength(1);

        pregunta.Property(p => p.Fuente)
            .HasMaxLength(100);

        pregunta.Property(p => p.Licencia)
            .HasMaxLength(50);

        pregunta.HasOne(p => p.Categoria)
            .WithMany(c => c.Preguntas)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

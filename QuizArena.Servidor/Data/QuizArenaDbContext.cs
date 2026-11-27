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
    public DbSet<Sala> Salas => Set<Sala>();

    public DbSet<ParticipanteSala> ParticipantesSala
        => Set<ParticipanteSala>();

    public DbSet<Partida> Partidas => Set<Partida>();

    public DbSet<PartidaPregunta> PartidasPreguntas
        => Set<PartidaPregunta>();

    public DbSet<PartidaJugador> PartidasJugadores
        => Set<PartidaJugador>();

    public DbSet<RespuestaPartida> RespuestasPartida
        => Set<RespuestaPartida>();

    public DbSet<EquipoPartida> EquiposPartida
        => Set<EquipoPartida>();

    public DbSet<VotoEquipo> VotosEquipo
        => Set<VotoEquipo>();

    public DbSet<RespuestaEquipo> RespuestasEquipo
        => Set<RespuestaEquipo>();

    public DbSet<MensajeSala> MensajesSala => Set<MensajeSala>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var jugador = modelBuilder.Entity<Jugador>();
        jugador.Property(j => j.Activo).HasDefaultValue(true);
        jugador.Property(j => j.EsAdministrador).HasDefaultValue(false);

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

        // Configuración de salas.
        var sala = modelBuilder.Entity<Sala>();

        sala.Property(s => s.Codigo)
            .IsRequired()
            .HasMaxLength(8);

        sala.HasIndex(s => s.Codigo)
            .IsUnique();

        sala.Property(s => s.Nombre)
            .IsRequired()
            .HasMaxLength(40);

        sala.Property(s => s.Modalidad)
            .HasConversion<string>()
            .HasMaxLength(15);

        sala.Property(s => s.Estado)
            .HasConversion<string>()
            .HasMaxLength(15);

        sala.Property(s => s.Dificultad)
            .HasConversion<string>()
            .HasMaxLength(10);

        sala.HasOne(s => s.Creador)
            .WithMany()
            .HasForeignKey(s => s.CreadorId)
            .OnDelete(DeleteBehavior.Restrict);

        sala.HasOne(s => s.Categoria)
            .WithMany()
            .HasForeignKey(s => s.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configuración de participantes.
        var participante = modelBuilder.Entity<ParticipanteSala>();

        participante.HasOne(p => p.Sala)
            .WithMany(s => s.Participantes)
            .HasForeignKey(p => p.SalaId)
            .OnDelete(DeleteBehavior.Cascade);

        participante.HasOne(p => p.Jugador)
            .WithMany()
            .HasForeignKey(p => p.JugadorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Impide registrar al mismo jugador dos veces en una sala.
        participante.HasIndex(p => new { p.SalaId, p.JugadorId })
            .IsUnique();

        // Durante la fase de espera, cada jugador solo puede
        // pertenecer a una sala.
        participante.HasIndex(p => p.JugadorId)
            .IsUnique();

        // PARTIDAS
        var partida = modelBuilder.Entity<Partida>();

        partida.HasIndex(p => p.SalaId);

        partida.HasOne(p => p.Sala)
            .WithMany()
            .HasForeignKey(p => p.SalaId)
            .OnDelete(DeleteBehavior.Restrict);

        partida.Property(p => p.Fase)
            .HasConversion<string>()
            .HasMaxLength(15);

        partida.Property(p => p.CreadaUtc)
            .HasConversion(
                f => f,
                f => DateTime.SpecifyKind(f, DateTimeKind.Utc));

        partida.Property(p => p.FinFaseUtc)
            .HasConversion(
                f => f,
                f => DateTime.SpecifyKind(f, DateTimeKind.Utc));

        // PREGUNTAS ASIGNADAS A UNA PARTIDA
        var partidaPregunta =
            modelBuilder.Entity<PartidaPregunta>();

        partidaPregunta.HasOne(p => p.Partida)
            .WithMany(p => p.Preguntas)
            .HasForeignKey(p => p.PartidaId)
            .OnDelete(DeleteBehavior.Cascade);

        partidaPregunta.HasOne(p => p.Pregunta)
            .WithMany()
            .HasForeignKey(p => p.PreguntaId)
            .OnDelete(DeleteBehavior.Restrict);

        partidaPregunta.HasIndex(
            p => new { p.PartidaId, p.Orden })
            .IsUnique();

        partidaPregunta.HasIndex(
            p => new { p.PartidaId, p.PreguntaId })
            .IsUnique();

        partidaPregunta.Property(p => p.AperturaUtc)
            .HasConversion(
                f => f,
                f => f.HasValue
                    ? DateTime.SpecifyKind(f.Value, DateTimeKind.Utc)
                    : null);

        partidaPregunta.Property(p => p.CierreUtc)
            .HasConversion(
                f => f,
                f => f.HasValue
                    ? DateTime.SpecifyKind(f.Value, DateTimeKind.Utc)
                    : null);

        // PARTICIPANTES Y PUNTUACIONES
        var partidaJugador =
            modelBuilder.Entity<PartidaJugador>();

        partidaJugador.HasOne(p => p.Partida)
            .WithMany(p => p.Jugadores)
            .HasForeignKey(p => p.PartidaId)
            .OnDelete(DeleteBehavior.Cascade);

        partidaJugador.HasOne(p => p.Jugador)
            .WithMany()
            .HasForeignKey(p => p.JugadorId)
            .OnDelete(DeleteBehavior.Restrict);

        partidaJugador.HasIndex(
            p => new { p.PartidaId, p.JugadorId })
            .IsUnique();

        // RESPUESTAS DE LOS JUGADORES
        var respuesta = modelBuilder.Entity<RespuestaPartida>();

        respuesta.HasOne(r => r.PartidaPregunta)
            .WithMany(p => p.Respuestas)
            .HasForeignKey(r => r.PartidaPreguntaId)
            .OnDelete(DeleteBehavior.Cascade);

        respuesta.HasOne(r => r.Jugador)
            .WithMany()
            .HasForeignKey(r => r.JugadorId)
            .OnDelete(DeleteBehavior.Restrict);

        respuesta.Property(r => r.Opcion)
            .IsRequired()
            .HasMaxLength(1);

        respuesta.HasIndex(
            r => new { r.PartidaPreguntaId, r.JugadorId })
            .IsUnique();

        respuesta.Property(r => r.EnviadaUtc)
            .HasConversion(
                f => f,
                f => DateTime.SpecifyKind(f, DateTimeKind.Utc));

        // Equipos de una partida.
        var equipo = modelBuilder.Entity<EquipoPartida>();

        equipo.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(30);

        equipo.HasOne(e => e.Partida)
            .WithMany(p => p.Equipos)
            .HasForeignKey(e => e.PartidaId)
            .OnDelete(DeleteBehavior.Cascade);

        equipo.HasIndex(e => new { e.PartidaId, e.Nombre })
            .IsUnique();

        // Relación entre jugador y equipo.
        modelBuilder.Entity<PartidaJugador>()
            .HasOne(j => j.Equipo)
            .WithMany(e => e.Miembros)
            .HasForeignKey(j => j.EquipoPartidaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Votos provisionales.
        var voto = modelBuilder.Entity<VotoEquipo>();

        voto.Property(v => v.Opcion)
            .IsRequired()
            .HasMaxLength(1);

        voto.HasOne(v => v.PartidaPregunta)
            .WithMany(p => p.VotosEquipo)
            .HasForeignKey(v => v.PartidaPreguntaId)
            .OnDelete(DeleteBehavior.Cascade);

        voto.HasOne(v => v.Jugador)
            .WithMany()
            .HasForeignKey(v => v.JugadorId)
            .OnDelete(DeleteBehavior.Restrict);

        voto.HasIndex(v => new
        {
            v.PartidaPreguntaId,
            v.JugadorId
        }).IsUnique();

        // Respuesta definitiva del equipo.
        var respuestaEquipo = modelBuilder.Entity<RespuestaEquipo>();

        respuestaEquipo.Property(r => r.Opcion)
            .IsRequired()
            .HasMaxLength(1);

        respuestaEquipo.HasOne(r => r.PartidaPregunta)
            .WithMany(p => p.RespuestasEquipo)
            .HasForeignKey(r => r.PartidaPreguntaId)
            .OnDelete(DeleteBehavior.Cascade);

        respuestaEquipo.HasOne(r => r.Equipo)
            .WithMany()
            .HasForeignKey(r => r.EquipoPartidaId)
            .OnDelete(DeleteBehavior.Restrict);

        respuestaEquipo.HasIndex(r => new
        {
            r.PartidaPreguntaId,
            r.EquipoPartidaId
        }).IsUnique();

var mensaje = modelBuilder.Entity<MensajeSala>();

mensaje.Property(m => m.Texto)
    .IsRequired()
    .HasMaxLength(250);

mensaje.HasOne(m => m.Sala)
    .WithMany()
    .HasForeignKey(m => m.SalaId)
    .OnDelete(DeleteBehavior.Cascade);

mensaje.HasOne(m => m.Jugador)
    .WithMany()
    .HasForeignKey(m => m.JugadorId)
    .OnDelete(DeleteBehavior.Restrict);

mensaje.HasOne(m => m.Equipo)
    .WithMany()
    .HasForeignKey(m => m.EquipoPartidaId)
    .OnDelete(DeleteBehavior.Restrict);

mensaje.Property(m => m.FechaUtc)
    .HasConversion(
        f => f,
        f => DateTime.SpecifyKind(f, DateTimeKind.Utc));

mensaje.HasIndex(m => new { m.SalaId, m.Id });

        voto.Property(v => v.FechaUtc).HasConversion(
            f => f, f => DateTime.SpecifyKind(f, DateTimeKind.Utc));
        respuestaEquipo.Property(r => r.ConfirmadaUtc).HasConversion(
            f => f, f => DateTime.SpecifyKind(f, DateTimeKind.Utc));
    }
}

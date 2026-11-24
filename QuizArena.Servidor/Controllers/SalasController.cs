using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using QuizArena.Servidor.Data;
using QuizArena.Servidor.DTOs;
using QuizArena.Servidor.Models;
using QuizArena.Servidor.Services;

namespace QuizArena.Servidor.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SalasController : ControllerBase
{
    private readonly QuizArenaDbContext db;
    private readonly CoordinadorSalas coordinador;

    public SalasController(
        QuizArenaDbContext contexto,
        CoordinadorSalas coordinadorSalas)
    {
        db = contexto;
        coordinador = coordinadorSalas;
    }

    private int? JugadorActual()
    {
        string? valor =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(valor, out int id) ? id : null;
    }

    // GET /api/salas/publicas
    [HttpGet("publicas")]
    public async Task<IActionResult> Publicas(
        CancellationToken ct)
    {
        var salas = await db.Salas
            .AsNoTracking()
            .Include(s => s.Categoria)
            .Include(s => s.Participantes)
            .Where(s => !s.EsPrivada &&
                        s.Estado == EstadoSala.EnEspera)
            .OrderByDescending(s => s.CreadaUtc)
            .ToListAsync(ct);

        return Ok(salas.Select(s => new SalaPublicaDto(
            s.Codigo,
            s.Nombre,
            s.Modalidad.ToString(),
            s.Participantes.Count,
            s.MaxJugadores,
            s.NumeroPreguntas,
            s.Categoria?.Nombre ?? "Todas",
            s.Dificultad?.ToString() ?? "Mixta"
        )).ToList());
    }

    // GET /api/salas/mi-sala
    [HttpGet("mi-sala")]
    public async Task<IActionResult> MiSala(
        CancellationToken ct)
    {
        if (JugadorActual() is not int jugadorId)
            return Unauthorized();

        string? codigo = await db.ParticipantesSala
            .AsNoTracking()
            .Where(p => p.JugadorId == jugadorId)
            .Select(p => p.Sala.Codigo)
            .SingleOrDefaultAsync(ct);

        if (codigo == null)
            return NotFound(new { mensaje = "No estás en una sala" });

        var sala = await CargarSalaAsync(codigo, ct);

        return sala == null
            ? NotFound()
            : Ok(Convertir(sala));
    }

    // GET /api/salas/AB12CD34
    [HttpGet("{codigo}")]
    public async Task<IActionResult> Detalle(
        string codigo, CancellationToken ct)
    {
        if (JugadorActual() is not int jugadorId)
            return Unauthorized();

        var sala = await CargarSalaAsync(
            codigo.ToUpperInvariant(), ct);

        if (sala == null)
            return NotFound();

        if (!sala.Participantes.Any(
                p => p.JugadorId == jugadorId))
            return Forbid();

        return Ok(Convertir(sala));
    }

    // POST /api/salas
    [HttpPost]
    public async Task<IActionResult> Crear(
        CrearSalaDto datos, CancellationToken ct)
    {
        if (JugadorActual() is not int jugadorId)
            return Unauthorized();

        await coordinador.Semaforo.WaitAsync(ct);

        try
        {
            bool yaTieneSala = await db.ParticipantesSala
                .AnyAsync(p => p.JugadorId == jugadorId, ct);

            if (yaTieneSala)
                return Conflict(new
                {
                    mensaje = "Ya perteneces a una sala"
                });

            string? error = await ValidarConfiguracionAsync(
                datos, ct);

            if (error != null)
                return BadRequest(new { mensaje = error });

            Enum.TryParse<ModalidadSala>(
                datos.Modalidad, true, out var modalidad);

            var sala = new Sala
            {
                Codigo = Guid.NewGuid().ToString("N")[..8]
                    .ToUpperInvariant(),
                Nombre = datos.Nombre.Trim(),
                EsPrivada = datos.EsPrivada,
                Modalidad = modalidad,
                Estado = EstadoSala.EnEspera,
                MaxJugadores = datos.MaxJugadores,
                NumeroPreguntas = datos.NumeroPreguntas,
                CategoriaId = datos.CategoriaId,
                Dificultad = datos.Dificultad.HasValue
                    ? (DificultadPregunta)datos.Dificultad.Value
                    : null,
                CreadorId = jugadorId,
                CreadaUtc = DateTime.UtcNow,
                Participantes = new List<ParticipanteSala>
                {
                    new ParticipanteSala
                    {
                        JugadorId = jugadorId,
                        Preparado = false,
                        UnidoUtc = DateTime.UtcNow
                    }
                }
            };

            db.Salas.Add(sala);

            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is SqliteException sql &&
                      sql.SqliteExtendedErrorCode == 2067)
            {
                return Conflict(new
                {
                    mensaje = "Conflicto al crear la sala. Reinténtalo."
                });
            }

            var guardada = await CargarSalaAsync(sala.Codigo, ct);

            return CreatedAtAction(
                nameof(Detalle),
                new { codigo = sala.Codigo },
                Convertir(guardada!));
        }
        finally
        {
            coordinador.Semaforo.Release();
        }
    }

    // POST /api/salas/AB12CD34/unirse
    [HttpPost("{codigo}/unirse")]
    public async Task<IActionResult> Unirse(
        string codigo, CancellationToken ct)
    {
        if (JugadorActual() is not int jugadorId)
            return Unauthorized();

        codigo = codigo.Trim().ToUpperInvariant();

        await coordinador.Semaforo.WaitAsync(ct);

        try
        {
            bool yaTieneSala = await db.ParticipantesSala
                .AnyAsync(p => p.JugadorId == jugadorId, ct);

            if (yaTieneSala)
                return Conflict(new
                {
                    mensaje = "Ya perteneces a una sala"
                });

            var sala = await db.Salas
                .Include(s => s.Participantes)
                .SingleOrDefaultAsync(s => s.Codigo == codigo, ct);

            if (sala == null)
                return NotFound(new
                {
                    mensaje = "Sala no encontrada"
                });

            if (sala.Estado != EstadoSala.EnEspera)
                return Conflict(new
                {
                    mensaje = "La partida ya ha comenzado"
                });

            if (sala.Participantes.Count >= sala.MaxJugadores)
                return Conflict(new
                {
                    mensaje = "La sala está llena"
                });

            db.ParticipantesSala.Add(new ParticipanteSala
            {
                SalaId = sala.Id,
                JugadorId = jugadorId,
                Preparado = false,
                UnidoUtc = DateTime.UtcNow
            });

            await db.SaveChangesAsync(ct);

            var actualizada = await CargarSalaAsync(codigo, ct);

            return Ok(Convertir(actualizada!));
        }
        finally
        {
            coordinador.Semaforo.Release();
        }
    }

    // PUT /api/salas/AB12CD34/preparado
    [HttpPut("{codigo}/preparado")]
    public async Task<IActionResult> Preparado(
        string codigo,
        PreparadoSalaDto datos,
        CancellationToken ct)
    {
        if (JugadorActual() is not int jugadorId)
            return Unauthorized();

        codigo = codigo.Trim().ToUpperInvariant();

        await coordinador.Semaforo.WaitAsync(ct);

        try
        {
            var participante = await db.ParticipantesSala
                .Include(p => p.Sala)
                .SingleOrDefaultAsync(
                    p => p.JugadorId == jugadorId &&
                         p.Sala.Codigo == codigo, ct);

            if (participante == null)
                return NotFound(new
                {
                    mensaje = "No perteneces a esta sala"
                });

            if (participante.Sala.Estado != EstadoSala.EnEspera)
                return Conflict(new
                {
                    mensaje = "La sala ya no está en espera"
                });

            participante.Preparado = datos.Preparado;
            await db.SaveChangesAsync(ct);

            return Ok(new
            {
                mensaje = "Estado actualizado",
                preparado = participante.Preparado
            });
        }
        finally
        {
            coordinador.Semaforo.Release();
        }
    }

    // PUT /api/salas/AB12CD34/configuracion
    [HttpPut("{codigo}/configuracion")]
    public async Task<IActionResult> Configuracion(
        string codigo,
        CrearSalaDto datos,
        CancellationToken ct)
    {
        if (JugadorActual() is not int jugadorId)
            return Unauthorized();

        codigo = codigo.Trim().ToUpperInvariant();

        await coordinador.Semaforo.WaitAsync(ct);

        try
        {
            var sala = await db.Salas
                .Include(s => s.Participantes)
                .SingleOrDefaultAsync(s => s.Codigo == codigo, ct);

            if (sala == null)
                return NotFound();

            if (sala.CreadorId != jugadorId)
                return Forbid();

            if (sala.Estado != EstadoSala.EnEspera)
                return Conflict(new
                {
                    mensaje = "La partida ya ha comenzado"
                });

            string? error = await ValidarConfiguracionAsync(
                datos, ct);

            if (error != null)
                return BadRequest(new { mensaje = error });

            if (datos.MaxJugadores < sala.Participantes.Count)
                return Conflict(new
                {
                    mensaje = "El nuevo aforo es menor que los participantes"
                });

            Enum.TryParse<ModalidadSala>(
                datos.Modalidad, true, out var modalidad);

            sala.Nombre = datos.Nombre.Trim();
            sala.EsPrivada = datos.EsPrivada;
            sala.Modalidad = modalidad;
            sala.MaxJugadores = datos.MaxJugadores;
            sala.NumeroPreguntas = datos.NumeroPreguntas;
            sala.CategoriaId = datos.CategoriaId;
            sala.Dificultad = datos.Dificultad.HasValue
                ? (DificultadPregunta)datos.Dificultad.Value
                : null;

            foreach (var participante in sala.Participantes)
                participante.Preparado = false;

            await db.SaveChangesAsync(ct);

            var actualizada = await CargarSalaAsync(codigo, ct);

            return Ok(Convertir(actualizada!));
        }
        finally
        {
            coordinador.Semaforo.Release();
        }
    }

    // DELETE /api/salas/AB12CD34/salir
    [HttpDelete("{codigo}/salir")]
    public async Task<IActionResult> Salir(
        string codigo, CancellationToken ct)
    {
        if (JugadorActual() is not int jugadorId)
            return Unauthorized();

        codigo = codigo.Trim().ToUpperInvariant();

        await coordinador.Semaforo.WaitAsync(ct);

        try
        {
            var sala = await db.Salas
                .Include(s => s.Participantes)
                .SingleOrDefaultAsync(s => s.Codigo == codigo, ct);

            if (sala == null)
                return NotFound();

            var participante = sala.Participantes
                .SingleOrDefault(p => p.JugadorId == jugadorId);

            if (participante == null)
                return NotFound(new
                {
                    mensaje = "No perteneces a la sala"
                });

            if (sala.Estado == EstadoSala.EnPartida)
                return Conflict(new
                {
                    mensaje = "No se puede salir durante la partida"
                });

            if (sala.Participantes.Count == 1)
            {
                if (sala.Estado == EstadoSala.EnEspera)
                    db.Salas.Remove(sala);
                else
                    // Conservamos la sala finalizada y su historial.
                    db.ParticipantesSala.Remove(participante);
            }
            else
            {
                db.ParticipantesSala.Remove(participante);

                if (sala.CreadorId == jugadorId)
                {
                    var nuevoAnfitrion = sala.Participantes
                        .Where(p => p.JugadorId != jugadorId)
                        .OrderBy(p => p.UnidoUtc)
                        .ThenBy(p => p.Id)
                        .First();

                    sala.CreadorId = nuevoAnfitrion.JugadorId;
                }
            }

            await db.SaveChangesAsync(ct);
            return NoContent();
        }
        finally
        {
            coordinador.Semaforo.Release();
        }
    }

    private async Task<string?> ValidarConfiguracionAsync(
        CrearSalaDto datos, CancellationToken ct)
    {
        string nombre = datos.Nombre?.Trim() ?? "";

        if (nombre.Length is < 3 or > 40)
            return "El nombre debe tener entre 3 y 40 caracteres";

        if (!Enum.TryParse<ModalidadSala>(
                datos.Modalidad, true, out var modalidad) ||
            !Enum.IsDefined(modalidad))
            return "Modalidad inválida";

        if (modalidad == ModalidadSala.Individual &&
            datos.MaxJugadores is < 2 or > 6)
            return "El modo individual admite entre 2 y 6 jugadores";

        if (modalidad == ModalidadSala.Equipos &&
            datos.MaxJugadores is < 4 or > 12)
            return "El modo equipos admite entre 4 y 12 jugadores";

        if (datos.NumeroPreguntas is not (5 or 10 or 15))
            return "Solo se permiten 5, 10 o 15 preguntas";

        if (datos.CategoriaId.HasValue &&
            datos.CategoriaId.Value <= 0)
            return "Categoría inválida";

        if (datos.Dificultad.HasValue &&
            !Enum.IsDefined(
                typeof(DificultadPregunta), datos.Dificultad.Value))
            return "Dificultad inválida";

        var consulta = db.Preguntas.Where(p => p.Activa);

        if (datos.CategoriaId.HasValue)
        {
            int categoriaId = datos.CategoriaId.Value;
            consulta = consulta.Where(
                p => p.CategoriaId == categoriaId);
        }

        if (datos.Dificultad.HasValue)
        {
            var dificultad =
                (DificultadPregunta)datos.Dificultad.Value;

            consulta = consulta.Where(
                p => p.Dificultad == dificultad);
        }

        int disponibles = await consulta.CountAsync(ct);

        if (disponibles < datos.NumeroPreguntas)
            return $"Solo existen {disponibles} preguntas para esos filtros";

        return null;
    }

    private async Task<Sala?> CargarSalaAsync(
        string codigo, CancellationToken ct)
    {
        return await db.Salas
            .AsNoTracking()
            .Include(s => s.Categoria)
            .Include(s => s.Participantes)
                .ThenInclude(p => p.Jugador)
            .SingleOrDefaultAsync(s => s.Codigo == codigo, ct);
    }

    private static SalaDetalleDto Convertir(Sala sala)
    {
        return new SalaDetalleDto(
            sala.Codigo,
            sala.Nombre,
            sala.EsPrivada,
            sala.Modalidad.ToString(),
            sala.Estado.ToString(),
            sala.MaxJugadores,
            sala.NumeroPreguntas,
            sala.CategoriaId,
            sala.Categoria?.Nombre ?? "Todas",
            sala.Dificultad?.ToString() ?? "Mixta",
            sala.CreadorId,
            sala.Participantes
                .OrderBy(p => p.UnidoUtc)
                .ThenBy(p => p.Id)
                .Select(p => new ParticipanteSalaDto(
                    p.JugadorId,
                    p.Jugador.Nombre,
                    p.Preparado,
                    p.JugadorId == sala.CreadorId))
                .ToList()
        );
    }
}

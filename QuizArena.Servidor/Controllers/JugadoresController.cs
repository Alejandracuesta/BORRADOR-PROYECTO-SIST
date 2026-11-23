using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QuizArena.Servidor.Data;
using QuizArena.Servidor.DTOs;
using QuizArena.Servidor.Models;

namespace QuizArena.Servidor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JugadoresController : ControllerBase
{
    private readonly QuizArenaDbContext db;

    public JugadoresController(QuizArenaDbContext contexto)
    {
        db = contexto;
    }

    // GET /api/jugadores
    [HttpGet]
    public async Task<ActionResult<List<JugadorRespuestaDto>>> Obtener(
        CancellationToken cancellationToken)
    {
        var jugadores = await db.Jugadores
            .AsNoTracking()
            .OrderBy(j => j.Id)
            .Select(j => new JugadorRespuestaDto(
                j.Id,
                j.Nombre,
                j.FechaRegistroUtc))
            .ToListAsync(cancellationToken);

        return Ok(jugadores);
    }

    // GET /api/jugadores/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<JugadorRespuestaDto>> ObtenerPorId(
        int id, CancellationToken cancellationToken)
    {
        var jugador = await db.Jugadores
            .AsNoTracking()
            .SingleOrDefaultAsync(j => j.Id == id, cancellationToken);

        if (jugador is null)
        {
            return NotFound(new
            {
                mensaje = "Jugador no encontrado"
            });
        }

        return Ok(new JugadorRespuestaDto(
            jugador.Id,
            jugador.Nombre,
            jugador.FechaRegistroUtc));
    }

    // POST /api/jugadores
    [HttpPost]
    public async Task<ActionResult<JugadorRespuestaDto>> Crear(
        CrearJugadorDto datos, CancellationToken cancellationToken)
    {
        string nombre = datos.Nombre.Trim();

        if (nombre.Length < 3)
        {
            return BadRequest(new
            {
                mensaje = "El nombre debe tener al menos 3 caracteres"
            });
        }

        var jugador = new Jugador
        {
            Nombre = nombre,
            NombreNormalizado = nombre.ToUpperInvariant(),
            FechaRegistroUtc = DateTime.UtcNow
        };

        db.Jugadores.Add(jugador);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqliteException error &&
                  error.SqliteExtendedErrorCode == 2067)
        {
            return Conflict(new
            {
                mensaje = "Ya existe un jugador con ese nombre"
            });
        }

        var respuesta = new JugadorRespuestaDto(
            jugador.Id,
            jugador.Nombre,
            jugador.FechaRegistroUtc);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = jugador.Id },
            respuesta);
    }
}

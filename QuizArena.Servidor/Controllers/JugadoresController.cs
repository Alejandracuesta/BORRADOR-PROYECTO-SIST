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
            .AsNoTracking().Where(j => j.Activo)
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
            .AsNoTracking().Where(j => j.Activo)
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

    [HttpPost]
    public IActionResult Crear(CrearJugadorDto datos) => StatusCode(410,
        new { mensaje = "Registra tu cuenta con contraseña en /api/acceso/registrar" });
}

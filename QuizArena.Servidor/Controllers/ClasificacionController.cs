using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizArena.Servidor.Data;
using QuizArena.Servidor.DTOs;
using QuizArena.Servidor.Models;

namespace QuizArena.Servidor.Controllers;

[ApiController]
[Authorize]
[Route("api/clasificacion")]
public class ClasificacionController : ControllerBase
{
    private readonly QuizArenaDbContext db;

    public ClasificacionController(QuizArenaDbContext contexto)
    {
        db = contexto;
    }

    // GET /api/clasificacion/individual
    [HttpGet("individual")]
    public async Task<IActionResult> Individual(
        CancellationToken ct)
    {
        var registros = await db.PartidasJugadores
            .AsNoTracking()
            .Where(p =>
                p.Partida.Fase == FasePartida.Finalizada &&
                p.EquipoPartidaId == null)
            .Select(p => new
            {
                p.JugadorId,
                p.Jugador.Nombre,
                p.Puntos,
                p.Aciertos
            })
            .ToListAsync(ct);

        var clasificacion = registros
            .GroupBy(p => new { p.JugadorId, p.Nombre })
            .Select(g => new ClasificacionIndividualDto(
                g.Key.JugadorId,
                g.Key.Nombre,
                g.Count(),
                g.Sum(p => p.Puntos),
                g.Sum(p => p.Aciertos)))
            .OrderByDescending(p => p.Puntos)
            .ThenByDescending(p => p.Aciertos)
            .ThenBy(p => p.Nombre)
            .Take(20)
            .ToList();

        return Ok(clasificacion);
    }

    // GET /api/clasificacion/equipos
    [HttpGet("equipos")]
    public async Task<IActionResult> Equipos(
        CancellationToken ct)
    {
        var equipos = await db.EquiposPartida
            .AsNoTracking()
            .Where(e =>
                e.Partida.Fase == FasePartida.Finalizada)
            .OrderByDescending(e => e.Puntos)
            .ThenByDescending(e => e.Aciertos)
            .Take(20)
            .Select(e => new ClasificacionEquipoDto(
                e.PartidaId,
                e.Partida.Sala.Codigo,
                e.Nombre,
                e.Puntos,
                e.Aciertos,
                e.Partida.CreadaUtc))
            .ToListAsync(ct);

        return Ok(equipos);
    }
}

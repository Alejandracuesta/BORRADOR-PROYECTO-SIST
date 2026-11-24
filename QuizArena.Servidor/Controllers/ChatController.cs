using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizArena.Servidor.Data;
using QuizArena.Servidor.DTOs;
using QuizArena.Servidor.Models;

namespace QuizArena.Servidor.Controllers;

[ApiController]
[Authorize]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly QuizArenaDbContext db;

    public ChatController(QuizArenaDbContext contexto)
    {
        db = contexto;
    }

    [HttpGet("{codigo}")]
    public async Task<IActionResult> Historial(
        string codigo, CancellationToken ct)
    {
        string? identidad =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(identidad, out int jugadorId))
            return Unauthorized();

        codigo = codigo.Trim().ToUpperInvariant();

        var sala = await db.Salas
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Codigo == codigo, ct);

        if (sala == null)
            return NotFound();

        bool pertenece = await db.ParticipantesSala
            .AnyAsync(p => p.SalaId == sala.Id &&
                           p.JugadorId == jugadorId, ct);

        if (!pertenece)
            return Forbid();

        var consulta = db.MensajesSala
            .AsNoTracking()
            .Where(m => m.SalaId == sala.Id);

        if (sala.Modalidad == ModalidadSala.Equipos &&
            sala.Estado == EstadoSala.EnPartida)
        {
            int? equipoId = await db.PartidasJugadores
                .AsNoTracking()
                .Where(p => p.Partida.SalaId == sala.Id &&
                            p.JugadorId == jugadorId)
                .OrderByDescending(p => p.PartidaId)
                .Select(p => p.EquipoPartidaId)
                .FirstOrDefaultAsync(ct);

            if (!equipoId.HasValue)
                return Forbid();

            consulta = consulta.Where(
                m => m.EquipoPartidaId == equipoId);
        }
        else
        {
            consulta = consulta.Where(
                m => m.EquipoPartidaId == null);
        }

        var ultimos = await consulta
            .OrderByDescending(m => m.Id)
            .Take(50)
            .Select(m => new MensajeSalaDto(
                m.Id,
                codigo,
                m.JugadorId,
                m.Jugador.Nombre,
                m.Texto,
                m.FechaUtc,
                m.EquipoPartidaId))
            .ToListAsync(ct);

        ultimos.Reverse();
        return Ok(ultimos);
    }
}

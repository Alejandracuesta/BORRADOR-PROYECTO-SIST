using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using QuizArena.Servidor.Hubs;
using QuizArena.Servidor.Services;
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
    private readonly CoordinadorSalas coordinador;
    private readonly IHubContext<SalaHub> hub;

    public ChatController(QuizArenaDbContext contexto, CoordinadorSalas coordinadorSalas, IHubContext<SalaHub> contextoHub)
    {
        db = contexto;
        coordinador = coordinadorSalas;
        hub = contextoHub;
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
                m.EquipoPartidaId, m.Eliminado, m.EditadoUtc))
            .ToListAsync(ct);

        ultimos.Reverse();
        return Ok(ultimos);
    }
    [HttpPut("mensajes/{id:int}")]
    public Task<IActionResult> Editar(int id, EditarMensajeDto datos, CancellationToken ct)
        => CambiarMensajeAsync(id, datos.Texto.Trim(), false, ct);

    [HttpDelete("mensajes/{id:int}")]
    public Task<IActionResult> Eliminar(int id, CancellationToken ct)
        => CambiarMensajeAsync(id, "[Mensaje eliminado]", true, ct);

    private async Task<IActionResult> CambiarMensajeAsync(int id, string texto, bool eliminar, CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int jugadorId)) return Unauthorized();
        if (texto.Length is < 1 or > 250) return BadRequest(new { mensaje = "El texto debe tener entre 1 y 250 caracteres" });
        await coordinador.Semaforo.WaitAsync(ct);
        try
        {
            var mensaje = await db.MensajesSala.Include(m => m.Sala).SingleOrDefaultAsync(m => m.Id == id, ct);
            if (mensaje is null) return NotFound();
            if (mensaje.JugadorId != jugadorId) return Forbid();
            if (mensaje.Eliminado) return Conflict(new { mensaje = "El mensaje ya está eliminado" });
            mensaje.Texto = texto;
            mensaje.Eliminado = eliminar;
            mensaje.EditadoUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            var destinatarios = db.ParticipantesSala.Where(p => p.SalaId == mensaje.SalaId && p.Jugador.Activo);
            if (mensaje.EquipoPartidaId is int equipoId)
                destinatarios = destinatarios.Where(p => db.PartidasJugadores.Any(j => j.EquipoPartidaId == equipoId && j.JugadorId == p.JugadorId));
            else if (mensaje.Sala.Modalidad == ModalidadSala.Equipos && mensaje.Sala.Estado == EstadoSala.EnPartida)
                destinatarios = destinatarios.Where(p => false); // El chat general no forma parte del historial privado activo.
            var ids = await destinatarios.Select(p => p.JugadorId.ToString()).ToListAsync(ct);
            await hub.Clients.Users(ids).SendAsync("ChatActualizado", mensaje.Sala.Codigo, ct);
            return NoContent();
        }
        finally { coordinador.Semaforo.Release(); }
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizArena.Servidor.Data;
using QuizArena.Servidor.DTOs;
using QuizArena.Servidor.Models;
using QuizArena.Servidor.Services;

namespace QuizArena.Servidor.Controllers;

[ApiController]
[Authorize]
[Route("api/partidas/equipos")]
public class EquiposController : ControllerBase
{
    private readonly QuizArenaDbContext db;
    private readonly CoordinadorSalas coordinador;
    private readonly NotificadorTiempoReal notificador;

    public EquiposController(
        QuizArenaDbContext contexto,
        CoordinadorSalas coordinadorSalas, NotificadorTiempoReal notificadorTiempoReal)
    {
        db = contexto;
        coordinador = coordinadorSalas;
        notificador = notificadorTiempoReal;
    }

    private int? JugadorActual()
    {
        string? valor =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(valor, out int id) ? id : null;
    }

    // POST /api/partidas/equipos/votar
    [HttpPost("votar")]
    public async Task<IActionResult> Votar(
        ResponderPreguntaDto datos, CancellationToken ct)
    {
        if (JugadorActual() is not int jugadorId)
            return Unauthorized();

        string opcion = (datos.Opcion ?? "")
            .Trim().ToUpperInvariant();

        if (opcion is not ("A" or "B" or "C" or "D"))
            return BadRequest(new { mensaje = "Opción inválida" });

        await coordinador.Semaforo.WaitAsync(ct);

        try
        {
            var partida = await CargarPartidaAsync(jugadorId, ct);

            if (partida == null)
                return NotFound(new { mensaje = "No hay partida" });

            await MotorPartidas.AvanzarAsync(db, partida, ct);

            if (partida.Sala.Modalidad != ModalidadSala.Equipos ||
                partida.Fase != FasePartida.Pregunta)
                return Conflict(new { mensaje = "Votación no disponible" });

            var ronda = partida.Preguntas.Single(
                p => p.Orden == partida.IndicePregunta);

            DateTime ahora = DateTime.UtcNow;
            if (ronda.PreguntaId != datos.PreguntaId ||
                ahora >= partida.FinFaseUtc)
                return Conflict(new { mensaje = "Pregunta fuera de plazo" });

            var equipo = partida.Equipos.SingleOrDefault(
                e => e.Miembros.Any(m => m.JugadorId == jugadorId));

            if (equipo == null)
                return Forbid();

            if (ronda.RespuestasEquipo.Any(
                r => r.EquipoPartidaId == equipo.Id))
                return Conflict(new
                {
                    mensaje = "El equipo ya confirmó su respuesta"
                });

            var voto = ronda.VotosEquipo.SingleOrDefault(
                v => v.JugadorId == jugadorId);

            if (voto == null)
            {
                voto = new VotoEquipo
                {
                    PartidaPregunta = ronda,
                    JugadorId = jugadorId
                };

                ronda.VotosEquipo.Add(voto);
            }

            voto.Opcion = opcion;
            voto.FechaUtc = ahora;

            await db.SaveChangesAsync(ct);
            await notificador.VotosEquipoAsync(equipo.Id, ct);

            return Ok(new
            {
                mensaje = "Voto provisional registrado",
                opcion,
                consenso = MotorEquipos.ObtenerConsenso(ronda, equipo)
            });
        }
        finally
        {
            coordinador.Semaforo.Release();
        }
    }

    // POST /api/partidas/equipos/confirmar
    [HttpPost("confirmar")]
    public async Task<IActionResult> Confirmar(
        ConfirmarEquipoDto datos, CancellationToken ct)
    {
        if (JugadorActual() is not int jugadorId)
            return Unauthorized();

        await coordinador.Semaforo.WaitAsync(ct);

        try
        {
            var partida = await CargarPartidaAsync(jugadorId, ct);

            if (partida == null)
                return NotFound(new { mensaje = "No hay partida" });

            await MotorPartidas.AvanzarAsync(db, partida, ct);

            if (partida.Sala.Modalidad != ModalidadSala.Equipos ||
                partida.Fase != FasePartida.Pregunta)
                return Conflict(new { mensaje = "Pregunta cerrada" });

            var ronda = partida.Preguntas.Single(
                p => p.Orden == partida.IndicePregunta);

            DateTime ahora = DateTime.UtcNow;
            if (ronda.PreguntaId != datos.PreguntaId ||
                ahora >= partida.FinFaseUtc)
                return Conflict(new { mensaje = "Tiempo agotado" });

            var equipo = partida.Equipos.SingleOrDefault(
                e => e.Miembros.Any(m => m.JugadorId == jugadorId));

            if (equipo == null)
                return Forbid();

            if (ronda.RespuestasEquipo.Any(
                r => r.EquipoPartidaId == equipo.Id))
                return Conflict(new { mensaje = "Respuesta ya confirmada" });

            // Se comprueba unanimidad otra vez, bajo el semáforo.
            string? consenso =
                MotorEquipos.ObtenerConsenso(ronda, equipo);

            if (consenso == null)
                return Conflict(new
                {
                    mensaje = "El equipo todavía no tiene unanimidad"
                });

            MotorEquipos.Confirmar(
                ronda, equipo, consenso, ahora);

            await db.SaveChangesAsync(ct);

            await MotorPartidas.AvanzarAsync(db, partida, ct);

            await notificador.PartidaAsync(partida.Sala.Codigo, ct);
            return Ok(new { mensaje = "Respuesta del equipo confirmada" });
        }
        finally
        {
            coordinador.Semaforo.Release();
        }
    }

    private async Task<Partida?> CargarPartidaAsync(
        int jugadorId, CancellationToken ct)
    {
        int? salaId = await db.ParticipantesSala
            .AsNoTracking()
            .Where(p => p.JugadorId == jugadorId)
            .Select(p => (int?)p.SalaId)
            .SingleOrDefaultAsync(ct);

        if (!salaId.HasValue)
            return null;

        return await db.Partidas
            .Include(p => p.Sala)
            .Include(p => p.Preguntas)
                .ThenInclude(pp => pp.Pregunta)
                    .ThenInclude(pr => pr.Categoria)
            .Include(p => p.Preguntas)
                .ThenInclude(pp => pp.Respuestas)
            .Include(p => p.Preguntas)
                .ThenInclude(pp => pp.VotosEquipo)
            .Include(p => p.Preguntas)
                .ThenInclude(pp => pp.RespuestasEquipo)
            .Include(p => p.Jugadores)
                .ThenInclude(j => j.Jugador)
            .Include(p => p.Equipos)
                .ThenInclude(e => e.Miembros)
            .AsSplitQuery()
            .Where(p => p.SalaId == salaId.Value).OrderByDescending(p => p.Id).FirstOrDefaultAsync(ct);
    }
}

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
[Route("api/[controller]")]
public class PartidasController : ControllerBase
{
    private readonly QuizArenaDbContext db;
    private readonly CoordinadorSalas coordinador;
    private readonly NotificadorTiempoReal notificador;

    public PartidasController(
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

    // POST /api/partidas/AB12CD34/iniciar
    [HttpPost("{codigo}/iniciar")]
    public async Task<IActionResult> Iniciar(
        string codigo, CancellationToken ct)
    {
        if (JugadorActual() is not int jugadorId)
            return Unauthorized();

        await coordinador.Semaforo.WaitAsync(ct);

        try
        {
            var sala = await db.Salas
                .Include(s => s.Participantes)
                .SingleOrDefaultAsync(
                    s => s.Codigo == codigo.Trim().ToUpperInvariant(),
                    ct);

            if (sala == null)
                return NotFound(new { mensaje = "Sala no encontrada" });

            if (sala.CreadorId != jugadorId)
                return Forbid();

            if (sala.Estado != EstadoSala.EnEspera)
                return Conflict(new
                {
                    mensaje = "La sala ya no está en espera"
                });

            int minimo = sala.Modalidad == ModalidadSala.Equipos
                ? 4 : 2;

            if (sala.Participantes.Count < minimo)
            {
                return Conflict(new
                {
                    mensaje = $"Se necesitan al menos {minimo} jugadores"
                });
            }

            if (sala.Participantes.Any(p => !p.Preparado))
                return Conflict(new
                {
                    mensaje = "Todos los jugadores deben estar preparados"
                });

            var consulta = db.Preguntas
                .AsNoTracking()
                .Where(p => p.Activa);

            if (sala.CategoriaId.HasValue)
            {
                int categoriaId = sala.CategoriaId.Value;
                consulta = consulta.Where(
                    p => p.CategoriaId == categoriaId);
            }

            if (sala.Dificultad.HasValue)
            {
                var dificultad = sala.Dificultad.Value;
                consulta = consulta.Where(
                    p => p.Dificultad == dificultad);
            }

            int[] identificadores = await consulta
                .Select(p => p.Id)
                .ToArrayAsync(ct);

            if (identificadores.Length < sala.NumeroPreguntas)
                return Conflict(new
                {
                    mensaje = "No hay suficientes preguntas disponibles"
                });

            Random.Shared.Shuffle(identificadores.AsSpan());

            var partida = new Partida
            {
                SalaId = sala.Id,
                Fase = FasePartida.CuentaAtras,
                IndicePregunta = 0,
                CreadaUtc = DateTime.UtcNow,
                FinFaseUtc = DateTime.UtcNow.AddSeconds(
                    MotorPartidas.SegundosCuentaAtras)
            };

            for (int i = 0; i < sala.NumeroPreguntas; i++)
            {
                partida.Preguntas.Add(new PartidaPregunta
                {
                    PreguntaId = identificadores[i],
                    Orden = i
                });
            }

            foreach (var participante in sala.Participantes.OrderBy(p => p.UnidoUtc).ThenBy(p => p.Id))
            {
                partida.Jugadores.Add(new PartidaJugador
                {
                    JugadorId = participante.JugadorId,
                    Puntos = 0,
                    Racha = 0,
                    Aciertos = 0
                });
            }

            if (sala.Modalidad == ModalidadSala.Equipos)
            {
                int cantidadJugadores = partida.Jugadores.Count;

                // Número mínimo de equipos con un máximo de 3 miembros.
                int numeroEquipos = (cantidadJugadores + 2) / 3;

                var equipos = Enumerable.Range(1, numeroEquipos)
                    .Select(i => new EquipoPartida
                    {
                        Nombre = $"Equipo {i}"
                    })
                    .ToList();

                partida.Equipos.AddRange(equipos);

                // Distribución equilibrada y reproducible.
                for (int i = 0; i < cantidadJugadores; i++)
                {
                    int indiceEquipo = i % numeroEquipos;

                    partida.Jugadores[i].Equipo =
                        equipos[indiceEquipo];
                }
            }

            db.Partidas.Add(partida);
            sala.Estado = EstadoSala.EnPartida;

            await db.SaveChangesAsync(ct);

            await notificador.SalaAsync(sala.Codigo, ct);
            await notificador.PartidaAsync(sala.Codigo, ct);
            return Ok(new
            {
                mensaje = "Partida iniciada",
                partidaId = partida.Id,
                inicioUtc = partida.FinFaseUtc
            });
        }
        finally
        {
            coordinador.Semaforo.Release();
        }
    }

    // GET /api/partidas/actual
    [HttpGet("actual")]
    public async Task<IActionResult> Actual(
        CancellationToken ct)
    {
        if (JugadorActual() is not int jugadorId)
            return Unauthorized();

        await coordinador.Semaforo.WaitAsync(ct);

        try
        {
            int? salaId = await ObtenerSalaIdAsync(jugadorId, ct);

            if (!salaId.HasValue)
                return NotFound(new
                {
                    mensaje = "No perteneces a una sala"
                });

            var partida = await CargarPartidaAsync(salaId.Value, ct);

            if (partida == null)
                return NotFound(new
                {
                    mensaje = "Todavía no hay una partida iniciada"
                });

            if (await MotorPartidas.AvanzarAsync(db, partida, ct))
            {
                await notificador.PartidaAsync(partida.Sala.Codigo, ct);
                await notificador.SalaAsync(partida.Sala.Codigo, ct);
            }
            return Ok(Convertir(partida, jugadorId));
        }
        finally
        {
            coordinador.Semaforo.Release();
        }
    }

    // POST /api/partidas/responder
    [HttpPost("responder")]
    public async Task<IActionResult> Responder(
        ResponderPreguntaDto datos,
        CancellationToken ct)
    {
        if (JugadorActual() is not int jugadorId)
            return Unauthorized();

        string opcion = (datos.Opcion ?? "")
            .Trim()
            .ToUpperInvariant();

        if (opcion is not ("A" or "B" or "C" or "D"))
            return BadRequest(new
            {
                mensaje = "Debes elegir A, B, C o D"
            });

        await coordinador.Semaforo.WaitAsync(ct);

        try
        {
            int? salaId = await ObtenerSalaIdAsync(jugadorId, ct);

            if (!salaId.HasValue)
                return NotFound(new
                {
                    mensaje = "No perteneces a una sala"
                });

            var partida = await CargarPartidaAsync(salaId.Value, ct);

            if (partida == null)
                return NotFound(new
                {
                    mensaje = "No hay partida"
                });

            if (partida.Sala.Modalidad == ModalidadSala.Equipos)
            {
                return Conflict(new
                {
                    mensaje = "Utiliza la votación por equipos"
                });
            }

            // Ejecutar primero las transiciones que correspondan.
            await MotorPartidas.AvanzarAsync(db, partida, ct);

            if (partida.Fase != FasePartida.Pregunta)
                return Conflict(new
                {
                    mensaje = "No hay ninguna pregunta abierta"
                });

            var actual = partida.Preguntas.Single(
                p => p.Orden == partida.IndicePregunta);

            if (actual.PreguntaId != datos.PreguntaId)
                return Conflict(new
                {
                    mensaje = "La pregunta ya no está activa"
                });

            // Hora oficial de la respuesta: se usa para el plazo,
            // la bonificación por velocidad y la fecha guardada.
            DateTime ahora = DateTime.UtcNow;

            // El servidor vuelve a comprobar el tiempo.
            if (ahora >= partida.FinFaseUtc)
            {
                await MotorPartidas.AvanzarAsync(db, partida, ct);

                return Conflict(new
                {
                    mensaje = "El tiempo ha terminado"
                });
            }

            var jugador = partida.Jugadores.SingleOrDefault(
                p => p.JugadorId == jugadorId);

            if (jugador == null)
                return Forbid();

            if (actual.Respuestas.Any(
                r => r.JugadorId == jugadorId))
                return Conflict(new
                {
                    mensaje = "Ya has respondido esta pregunta"
                });

            bool correcta = string.Equals(
                actual.Pregunta.RespuestaCorrecta,
                opcion,
                StringComparison.OrdinalIgnoreCase);

            int puntos = 0;

            if (correcta)
            {
                jugador.Racha++;
                jugador.Aciertos++;

                // Cuanto más rápida es la respuesta, más puntos.
                TimeSpan tiempoRespuesta =
                    ahora - (actual.AperturaUtc ?? ahora);

                puntos = MotorPartidas.PuntosPorVelocidad(
                    actual.Pregunta.Dificultad, tiempoRespuesta);

                puntos += MotorPartidas.BonificacionRacha(
                    jugador.Racha);

                jugador.Puntos += puntos;
            }
            else
            {
                jugador.Racha = 0;
            }

            db.RespuestasPartida.Add(new RespuestaPartida
            {
                PartidaPregunta = actual,
                JugadorId = jugadorId,
                Opcion = opcion,
                EsCorrecta = correcta,
                PuntosGanados = puntos,
                EnviadaUtc = ahora
            });

            await db.SaveChangesAsync(ct);

            // Si respondió el último jugador, cerrar la ronda.
            await MotorPartidas.AvanzarAsync(db, partida, ct);
            await notificador.PartidaAsync(partida.Sala.Codigo, ct);

            return Ok(new
            {
                mensaje = "Respuesta registrada"
            });
        }
        finally
        {
            coordinador.Semaforo.Release();
        }
    }

    private async Task<int?> ObtenerSalaIdAsync(
        int jugadorId, CancellationToken ct)
    {
        return await db.ParticipantesSala
            .AsNoTracking()
            .Where(p => p.JugadorId == jugadorId)
            .Select(p => (int?)p.SalaId)
            .SingleOrDefaultAsync(ct);
    }

    private async Task<Partida?> CargarPartidaAsync(
        int salaId, CancellationToken ct)
    {
        return await db.Partidas
            .Include(p => p.Sala)
            .Include(p => p.Preguntas)
                .ThenInclude(pp => pp.Pregunta)
                    .ThenInclude(pr => pr.Categoria)
            .Include(p => p.Preguntas)
                .ThenInclude(pp => pp.Respuestas)
            .Include(p => p.Jugadores)
                .ThenInclude(pj => pj.Jugador)
            .Include(p => p.Preguntas)
                .ThenInclude(pp => pp.VotosEquipo)
            .Include(p => p.Preguntas)
                .ThenInclude(pp => pp.RespuestasEquipo)
            .Include(p => p.Equipos)
                .ThenInclude(e => e.Miembros)
                    .ThenInclude(m => m.Jugador)
            .AsSplitQuery()
            .Where(p => p.SalaId == salaId).OrderByDescending(p => p.Id).FirstOrDefaultAsync(ct);
    }

    private static EstadoPartidaDto Convertir(
        Partida partida, int jugadorId)
    {
        var actual = partida.Preguntas
            .Single(p => p.Orden == partida.IndicePregunta);

        bool mostrarPregunta =
            partida.Fase is FasePartida.Pregunta
                or FasePartida.Resultados;

        PreguntaJuegoDto? pregunta = mostrarPregunta
            ? new PreguntaJuegoDto(
                actual.PreguntaId,
                actual.Pregunta.Enunciado,
                actual.Pregunta.Categoria.Nombre,
                actual.Pregunta.Dificultad.ToString(),
                actual.Pregunta.OpcionA,
                actual.Pregunta.OpcionB,
                actual.Pregunta.OpcionC,
                actual.Pregunta.OpcionD)
            : null;

        string? miRespuesta = actual.Respuestas
            .FirstOrDefault(r => r.JugadorId == jugadorId)?
            .Opcion;

        if (partida.Sala.Modalidad == ModalidadSala.Equipos)
        {
            miRespuesta = actual.VotosEquipo
                .FirstOrDefault(v => v.JugadorId == jugadorId)?
                .Opcion;
        }

        ResultadoRondaDto? resultado = null;

        if (partida.Fase == FasePartida.Resultados)
        {
            var respuestas = partida.Sala.Modalidad == ModalidadSala.Equipos
                ? new List<RespuestaJugadorDto>()
                : partida.Jugadores
                .Select(j =>
                {
                    var r = actual.Respuestas
                        .FirstOrDefault(x => x.JugadorId == j.JugadorId);

                    return new RespuestaJugadorDto(
                        j.JugadorId,
                        j.Jugador.Nombre,
                        r?.Opcion,
                        r?.EsCorrecta ?? false,
                        r?.PuntosGanados ?? 0);
                })
                .ToList();

            resultado = new ResultadoRondaDto(
                actual.Pregunta.RespuestaCorrecta,
                respuestas);
        }

        // No revelar aciertos mediante el marcador mientras la ronda está abierta.
        // Las puntuaciones persistidas incluyen respuestas ya recibidas; el DTO
        // solo publica rondas cerradas, incluida la racha correspondiente.
        var cerradas = partida.Preguntas.Where(p => p.CierreUtc.HasValue)
            .OrderBy(p => p.Orden).ToList();
        var marcador = partida.Jugadores.Select(j =>
        {
            int puntos = 0, aciertos = 0, racha = 0;
            foreach (var ronda in cerradas)
            {
                var respuesta = ronda.Respuestas.FirstOrDefault(r => r.JugadorId == j.JugadorId);
                puntos += respuesta?.PuntosGanados ?? 0;
                if (respuesta?.EsCorrecta == true) { aciertos++; racha++; }
                else racha = 0;
            }
            return new MarcadorJugadorDto(j.JugadorId, j.Jugador.Nombre, puntos, aciertos, racha);
        }).OrderByDescending(j => j.Puntos).ThenBy(j => j.Nombre).ToList();

        EquipoActualDto? miEquipo = null;
        List<MarcadorEquipoDto>? marcadorEquipos = null;
        List<ResultadoEquipoDto>? resultadosEquipos = null;

        if (partida.Sala.Modalidad == ModalidadSala.Equipos)
        {
            // No se utiliza la clasificación individual.
            marcador.Clear();

            var equipoJugador = partida.Equipos.SingleOrDefault(
                e => e.Miembros.Any(m => m.JugadorId == jugadorId));

            if (equipoJugador != null)
            {
                var votos = equipoJugador.Miembros
                    .OrderBy(m => m.Jugador.Nombre)
                    .Select(m => new VotoMiembroEquipoDto(
                        m.JugadorId,
                        m.Jugador.Nombre,
                        actual.VotosEquipo
                            .FirstOrDefault(v => v.JugadorId == m.JugadorId)?
                            .Opcion))
                    .ToList();

                bool confirmada = actual.RespuestasEquipo.Any(
                    r => r.EquipoPartidaId == equipoJugador.Id);

                miEquipo = new EquipoActualDto(
                    equipoJugador.Id,
                    equipoJugador.Nombre,
                    MotorEquipos.ObtenerConsenso(
                        actual, equipoJugador) != null,
                    confirmada,
                    votos);
            }

            // Calcular marcador a partir de rondas ya cerradas.
            marcadorEquipos = partida.Equipos.Select(e =>
            {
                int puntos = 0;
                int aciertos = 0;
                int racha = 0;

                foreach (var ronda in cerradas)
                {
                    var respuestaEquipo = ronda.RespuestasEquipo
                        .FirstOrDefault(r => r.EquipoPartidaId == e.Id);

                    puntos += respuestaEquipo?.PuntosGanados ?? 0;

                    if (respuestaEquipo?.EsCorrecta == true)
                    {
                        aciertos++;
                        racha++;
                    }
                    else
                    {
                        racha = 0;
                    }
                }

                return new MarcadorEquipoDto(
                    e.Id, e.Nombre, puntos, aciertos, racha);
            })
            .OrderByDescending(e => e.Puntos)
            .ThenBy(e => e.Nombre)
            .ToList();

            // No revelar la respuesta definitiva antes de Resultados.
            if (partida.Fase == FasePartida.Resultados)
            {
                resultadosEquipos = partida.Equipos.Select(e =>
                {
                    var respuestaEquipo = actual.RespuestasEquipo
                        .FirstOrDefault(r => r.EquipoPartidaId == e.Id);

                    return new ResultadoEquipoDto(
                        e.Id,
                        e.Nombre,
                        respuestaEquipo?.Opcion,
                        respuestaEquipo?.EsCorrecta ?? false,
                        respuestaEquipo?.PuntosGanados ?? 0);
                }).ToList();
            }
        }

        return new EstadoPartidaDto(
            partida.Id,
            partida.Sala.Codigo,
            partida.Fase.ToString(),
            partida.IndicePregunta + 1,
            partida.Preguntas.Count,
            partida.Fase == FasePartida.Finalizada
                ? null
                : partida.FinFaseUtc,
            DateTime.UtcNow,
            pregunta,
            miRespuesta,
            resultado,
            marcador,
            miEquipo,
            marcadorEquipos,
            resultadosEquipos);
    }
    [HttpGet("historial")]
    public async Task<IActionResult> Historial(CancellationToken ct)
    {
        if (JugadorActual() is not int id) return Unauthorized();
        var partidas = await db.PartidasJugadores.AsNoTracking()
            .Where(j => j.JugadorId == id && j.Partida.Fase == FasePartida.Finalizada)
            .OrderByDescending(j => j.PartidaId)
            .Select(j => new HistorialPartidaDto(j.PartidaId, j.Partida.Sala.Codigo,
                j.Partida.Sala.Nombre, j.Partida.CreadaUtc,
                j.EquipoPartidaId.HasValue ? "Equipos" : "Individual",
                j.EquipoPartidaId.HasValue ? j.Equipo!.Puntos : j.Puntos,
                j.EquipoPartidaId.HasValue ? j.Equipo!.Aciertos : j.Aciertos)).ToListAsync(ct);
        return Ok(partidas);
    }

    [HttpGet("{id:int}/resumen")]
    public async Task<IActionResult> Resumen(int id, CancellationToken ct)
    {
        if (JugadorActual() is not int jugadorId) return Unauthorized();
        var partida = await db.Partidas.AsNoTracking().Include(p => p.Sala)
            .Include(p => p.Jugadores).ThenInclude(j => j.Jugador).Include(p => p.Equipos)
            .SingleOrDefaultAsync(p => p.Id == id, ct);
        if (partida is null) return NotFound();
        if (!partida.Jugadores.Any(j => j.JugadorId == jugadorId)) return Forbid();
        if (partida.Fase != FasePartida.Finalizada) return Conflict(new { mensaje = "La partida todavía no ha terminado" });
        var jugadores = partida.Jugadores.OrderByDescending(j => j.Puntos).ThenByDescending(j => j.Aciertos)
            .Select(j => new ResultadoJugadorDto(j.JugadorId, j.Jugador.Nombre,
                j.EquipoPartidaId.HasValue ? partida.Equipos.Single(e => e.Id == j.EquipoPartidaId).Puntos : j.Puntos,
                j.EquipoPartidaId.HasValue ? partida.Equipos.Single(e => e.Id == j.EquipoPartidaId).Aciertos : j.Aciertos,
                j.EquipoPartidaId)).ToList();
        var equipos = partida.Equipos.OrderByDescending(e => e.Puntos).ThenByDescending(e => e.Aciertos)
            .Select(e => new ResultadoEquipoHistoricoDto(e.Id, e.Nombre, e.Puntos, e.Aciertos, jugadores.Where(j => j.EquipoId == e.Id).ToList())).ToList();
        int maximo = equipos.Count > 0 ? equipos.Max(e => e.Puntos) : jugadores.Max(j => j.Puntos);
        var ganadores = equipos.Count > 0 ? equipos.Where(e => e.Puntos == maximo).Select(e => e.Nombre) : jugadores.Where(j => j.Puntos == maximo).Select(j => j.Nombre);
        return Ok(new ResumenPartidaDto(partida.Id, partida.Sala.Codigo, partida.CreadaUtc,
            equipos.Count > 0 ? "Equipos" : "Individual", string.Join(", ", ganadores) + $" ({maximo} puntos)", jugadores, equipos));
    }
}

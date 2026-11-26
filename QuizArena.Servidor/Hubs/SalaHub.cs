using System.Security.Claims;
using System.Globalization;
using QuizArena.Servidor.DTOs;
using QuizArena.Servidor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuizArena.Servidor.Data;
using QuizArena.Servidor.Models;

namespace QuizArena.Servidor.Hubs;

[Authorize]
public class SalaHub : Hub
{
    private readonly QuizArenaDbContext db;
    private readonly ControlFrecuencia frecuencia;
    private readonly CoordinadorSalas coordinador;

    public SalaHub(QuizArenaDbContext contexto, ControlFrecuencia controlFrecuencia, CoordinadorSalas coordinadorSalas)
    {
        db = contexto;
        frecuencia = controlFrecuencia;
        coordinador = coordinadorSalas;
    }

    public static string GrupoSala(string codigo)
        => $"sala:{codigo.Trim().ToUpperInvariant()}";

    public static string GrupoEquipo(int equipoId)
        => $"equipo:{equipoId}";

    public async Task EntrarSala(string codigo)
    {
        string? identidad =
            Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(identidad, out int jugadorId))
            throw new HubException("Sesión inválida.");

        await ComprobarActivoAsync(jugadorId);

        codigo = codigo.Trim().ToUpperInvariant();

        var sala = await db.Salas
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Codigo == codigo);

        if (sala == null)
            throw new HubException("Sala inexistente.");

        bool pertenece = await db.ParticipantesSala
            .AsNoTracking()
            .AnyAsync(p => p.SalaId == sala.Id &&
                           p.JugadorId == jugadorId);

        if (!pertenece)
            throw new HubException("No perteneces a esa sala.");

        // Retirar los grupos anteriores de esta conexión.
        await SalirSala();

        string grupo = GrupoSala(codigo);

        await Groups.AddToGroupAsync(
            Context.ConnectionId, grupo);

        Context.Items["sala"] = grupo;

        // Si ya comenzó una partida por equipos,
        // incorporamos además al grupo privado.
        if (sala.Modalidad == ModalidadSala.Equipos &&
            sala.Estado == EstadoSala.EnPartida)
        {
            int? equipoId = await db.PartidasJugadores
                .AsNoTracking()
                .Where(p => p.JugadorId == jugadorId &&
                            p.Partida.SalaId == sala.Id)
                .OrderByDescending(p => p.PartidaId)
                .Select(p => p.EquipoPartidaId)
                .FirstOrDefaultAsync();

            if (equipoId.HasValue)
            {
                string grupoEquipo =
                    GrupoEquipo(equipoId.Value);

                await Groups.AddToGroupAsync(
                    Context.ConnectionId, grupoEquipo);

                Context.Items["equipo"] = grupoEquipo;
            }
        }
    }

    public async Task SalirSala()
    {
        if (Context.Items.TryGetValue(
            "sala", out var salaAnterior) &&
            salaAnterior is string grupoSala)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId, grupoSala);

            Context.Items.Remove("sala");
        }

        if (Context.Items.TryGetValue(
            "equipo", out var equipoAnterior) &&
            equipoAnterior is string grupoEquipo)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId, grupoEquipo);

            Context.Items.Remove("equipo");
        }
    }
private int ObtenerJugadorId()
{
    string? valor =
        Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    if (!int.TryParse(valor, out int jugadorId))
        throw new HubException("Sesión inválida.");

    return jugadorId;
}

private async Task<(Sala sala, int? equipoId, List<string> destinos)>
    ObtenerDestinatariosAsync(string codigo)
{
    int jugadorId = ObtenerJugadorId();

    await ComprobarActivoAsync(jugadorId);

        codigo = codigo.Trim().ToUpperInvariant();

    var sala = await db.Salas
        .AsNoTracking()
        .SingleOrDefaultAsync(s => s.Codigo == codigo);

    if (sala == null)
        throw new HubException("Sala inexistente.");

    bool pertenece = await db.ParticipantesSala
        .AsNoTracking()
        .AnyAsync(p => p.SalaId == sala.Id &&
                       p.JugadorId == jugadorId);

    if (!pertenece)
        throw new HubException("No perteneces a la sala.");

    bool privado = sala.Estado == EstadoSala.EnPartida &&
                   sala.Modalidad == ModalidadSala.Equipos;

    int? equipoId = null;
    List<int> jugadoresDestino;

    if (privado)
    {
        equipoId = await db.PartidasJugadores
            .AsNoTracking()
            .Where(p => p.Partida.SalaId == sala.Id &&
                        p.JugadorId == jugadorId)
            .OrderByDescending(p => p.PartidaId)
            .Select(p => p.EquipoPartidaId)
            .FirstOrDefaultAsync();

        if (!equipoId.HasValue)
            throw new HubException("Equipo no encontrado.");

        jugadoresDestino = await db.PartidasJugadores
            .AsNoTracking()
            .Where(p => p.EquipoPartidaId == equipoId.Value)
            .Select(p => p.JugadorId)
            .ToListAsync();
    }
    else
    {
        jugadoresDestino = await db.ParticipantesSala
            .AsNoTracking()
            .Where(p => p.SalaId == sala.Id)
            .Select(p => p.JugadorId)
            .ToListAsync();
    }

    var destinos = jugadoresDestino
        .Select(id => id.ToString(CultureInfo.InvariantCulture))
        .ToList();

    return (sala, equipoId, destinos);
}

// Envía y almacena un mensaje de chat.
public async Task EnviarMensaje(
    string codigo, string texto, bool soloEquipo)
{
    await coordinador.Semaforo.WaitAsync(Context.ConnectionAborted);
    try
    {
    int jugadorId = ObtenerJugadorId();
    await ComprobarActivoAsync(jugadorId);

    texto = texto?.Trim() ?? "";

    if (texto.Length is < 1 or > 250)
        throw new HubException(
            "El mensaje debe tener entre 1 y 250 caracteres.");

    var (sala, equipoId, destinos) =
        await ObtenerDestinatariosAsync(codigo);

    if (soloEquipo != equipoId.HasValue)
        throw new HubException("Ámbito de chat incorrecto.");

    if (!frecuencia.Permitir(
        jugadorId, "chat", TimeSpan.FromSeconds(1)))
    {
        throw new HubException(
            "Debes esperar antes de enviar otro mensaje.");
    }

    var mensaje = new MensajeSala
    {
        SalaId = sala.Id,
        JugadorId = jugadorId,
        EquipoPartidaId = equipoId,
        Texto = texto,
        FechaUtc = DateTime.UtcNow
    };

    db.MensajesSala.Add(mensaje);
    await db.SaveChangesAsync();

    string nombre = await db.Jugadores.Where(j => j.Id == jugadorId).Select(j => j.Nombre).SingleAsync();

    var dto = new MensajeSalaDto(
        mensaje.Id,
        sala.Codigo,
        jugadorId,
        nombre,
        mensaje.Texto,
        mensaje.FechaUtc,
        equipoId);

    // Se envía a los miembros actuales, no a conexiones
    // que hayan abandonado la sala.
    await Clients.Users(destinos).SendAsync(
        "MensajeRecibido", dto);

    }
    finally { coordinador.Semaforo.Release(); }
} 

// Envía una reacción sin almacenarla.
public async Task EnviarReaccion(
    string codigo, string tipo)
{
    await coordinador.Semaforo.WaitAsync(Context.ConnectionAborted);
    try
    {
    int jugadorId = ObtenerJugadorId();
    await ComprobarActivoAsync(jugadorId);

    if (tipo is not ("Aplauso" or "Animo" or "Sorpresa"))
        throw new HubException("Reacción inválida.");

    var (sala, _, destinos) =
        await ObtenerDestinatariosAsync(codigo);

    var reaccion = new ReaccionDto(
        sala.Codigo,
        jugadorId,
        await db.Jugadores.Where(j => j.Id == jugadorId).Select(j => j.Nombre).SingleAsync(),
        tipo,
        DateTime.UtcNow);

    await Clients.Users(destinos).SendAsync(
        "ReaccionRecibida", reaccion);

    }
    finally { coordinador.Semaforo.Release(); }
} 


    private async Task ComprobarActivoAsync(int id)
    {
        if (!await db.Jugadores.AsNoTracking().AnyAsync(j => j.Id == id && j.Activo))
            throw new HubException("La cuenta ya no está activa.");
    }
}

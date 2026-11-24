using Microsoft.EntityFrameworkCore;
using QuizArena.Servidor.Data;
using QuizArena.Servidor.Models;

namespace QuizArena.Servidor.Services;

public sealed class SupervisorPartidas : BackgroundService
{
    private readonly IServiceScopeFactory scopes;
    private readonly CoordinadorSalas coordinador;
    private readonly NotificadorTiempoReal notificador;
    private readonly ILogger<SupervisorPartidas> logger;

    public SupervisorPartidas(
        IServiceScopeFactory scopeFactory,
        CoordinadorSalas coordinadorSalas,
        NotificadorTiempoReal notificadorTiempoReal,
        ILogger<SupervisorPartidas> registro)
    {
        scopes = scopeFactory;
        coordinador = coordinadorSalas;
        notificador = notificadorTiempoReal;
        logger = registro;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var temporizador =
            new PeriodicTimer(TimeSpan.FromSeconds(1));

        try
        {
            while (await temporizador.WaitForNextTickAsync(
                stoppingToken))
            {
                try
                {
                    await RevisarPartidasAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex, "Error al revisar partidas activas");
                }
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            // Parada normal del servidor.
        }
    }

    private async Task RevisarPartidasAsync(
        CancellationToken ct)
    {
        var salasActualizadas = new HashSet<string>();

        await coordinador.Semaforo.WaitAsync(ct);

        try
        {
            await using var scope =
                scopes.CreateAsyncScope();

            var db = scope.ServiceProvider
                .GetRequiredService<QuizArenaDbContext>();

            var ids = await db.Partidas
                .AsNoTracking()
                .Where(p => p.Fase != FasePartida.Finalizada && p.FinFaseUtc <= DateTime.UtcNow)
                .Select(p => p.Id)
                .ToListAsync(ct);

            foreach (int id in ids)
            {
                var partida = await db.Partidas
                    .Include(p => p.Sala)
                    .Include(p => p.Preguntas)
                        .ThenInclude(p => p.Pregunta)
                    .Include(p => p.Preguntas)
                        .ThenInclude(p => p.Respuestas)
                    .Include(p => p.Preguntas)
                        .ThenInclude(p => p.VotosEquipo)
                    .Include(p => p.Preguntas)
                        .ThenInclude(p => p.RespuestasEquipo)
                    .Include(p => p.Jugadores)
                    .Include(p => p.Equipos)
                        .ThenInclude(e => e.Miembros)
                    .AsSplitQuery()
                    .SingleOrDefaultAsync(p => p.Id == id, ct);

                if (partida == null)
                    continue;

                bool cambio = await MotorPartidas
                    .AvanzarAsync(db, partida, ct);

                if (cambio)
                    salasActualizadas.Add(partida.Sala.Codigo);
            }
        }
        finally
        {
            coordinador.Semaforo.Release();
        }

        // Las notificaciones se envían fuera del semáforo.
        foreach (string codigo in salasActualizadas)
        {
            try
            {
                await notificador.PartidaAsync(codigo, ct);
                await notificador.SalaAsync(codigo, ct);
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                logger.LogWarning(
                    ex, "No se pudo notificar la sala {Codigo}",
                    codigo);
            }
        }
    }
}

using Microsoft.AspNetCore.SignalR;
using QuizArena.Servidor.Hubs;

namespace QuizArena.Servidor.Services;

public class NotificadorTiempoReal
{
    private readonly IHubContext<SalaHub> hub;

    public NotificadorTiempoReal(IHubContext<SalaHub> contexto)
    {
        hub = contexto;
    }

    public async Task SalaAsync(
        string codigo, CancellationToken ct = default)
    {
        await hub.Clients.Group(
            SalaHub.GrupoSala(codigo))
            .SendAsync("SalaActualizada", codigo,
                cancellationToken: ct);

        await hub.Clients.All.SendAsync(
            "SalasPublicasActualizadas",
            cancellationToken: ct);
    }

    public Task PartidaAsync(
        string codigo, CancellationToken ct = default)
    {
        return hub.Clients.Group(
            SalaHub.GrupoSala(codigo))
            .SendAsync("PartidaActualizada", codigo,
                cancellationToken: ct);
    }

    public Task VotosEquipoAsync(
        int equipoId, CancellationToken ct = default)
    {
        return hub.Clients.Group(
            SalaHub.GrupoEquipo(equipoId))
            .SendAsync("VotosEquipoActualizados", equipoId,
                cancellationToken: ct);
    }
}

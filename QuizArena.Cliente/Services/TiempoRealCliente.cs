using System.Net;
using QuizArena.Cliente.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace QuizArena.Cliente.Services;

public sealed class TiempoRealCliente : IAsyncDisposable
{
    private readonly HubConnection conexion;
    private readonly SemaphoreSlim operaciones = new(1, 1);
    private readonly CancellationTokenSource cierre = new();
    private string? codigoSala;
    private string? grupoSincronizado;
    private bool cerrado;
    public event Action<string>? SalaActualizada;
    public event Action? SalasPublicasActualizadas;
    public event Action<string>? PartidaActualizada;
    public event Action<int>? VotosEquipoActualizados;
    public event Action? ConexionRestaurada;
    public event Action? ConexionPerdida;
    public event Action<MensajeChatApi>? MensajeRecibido;
    public event Action<ReaccionApi>? ReaccionRecibida;
    public bool Conectado => conexion.State == HubConnectionState.Connected;

    public TiempoRealCliente(Uri baseServidor, CookieContainer cookies)
    {
        conexion = new HubConnectionBuilder()
            .WithUrl(new Uri(baseServidor, "hubs/salas"), o => o.Cookies = cookies)
            .WithAutomaticReconnect().Build();
        conexion.On<string>("SalaActualizada", c => SalaActualizada?.Invoke(c));
        conexion.On("SalasPublicasActualizadas", () => SalasPublicasActualizadas?.Invoke());
        conexion.On<string>("PartidaActualizada", c => PartidaActualizada?.Invoke(c));
        conexion.On<int>("VotosEquipoActualizados", id => VotosEquipoActualizados?.Invoke(id));
        conexion.On<MensajeChatApi>("MensajeRecibido", m => MensajeRecibido?.Invoke(m));
        conexion.On<ReaccionApi>("ReaccionRecibida", r => ReaccionRecibida?.Invoke(r));
        conexion.Reconnecting += _ => { grupoSincronizado = null; ConexionPerdida?.Invoke(); return Task.CompletedTask; };
        conexion.Closed += _ => { grupoSincronizado = null; ConexionPerdida?.Invoke(); return Task.CompletedTask; };
        conexion.Reconnected += async _ =>
        {
            try { await SincronizarSalaAsync(codigoSala, true, cierre.Token); }
            catch (Exception) when (!cerrado) { /* REST reintentará los grupos. */ }
            catch (OperationCanceledException) { }
            if (!cerrado) ConexionRestaurada?.Invoke();
        };
    }

    public async Task IniciarAsync(CancellationToken ct = default)
    {
        using var vinculo = CancellationTokenSource.CreateLinkedTokenSource(ct, cierre.Token);
        vinculo.CancelAfter(TimeSpan.FromSeconds(5));
        await operaciones.WaitAsync(vinculo.Token);
        try
        {
            if (cerrado) return;
            bool iniciada = conexion.State == HubConnectionState.Disconnected;
            if (iniciada) { await conexion.StartAsync(vinculo.Token); grupoSincronizado = null; }
            await ActualizarGrupoAsync(false, vinculo.Token);
            if (iniciada && Conectado) ConexionRestaurada?.Invoke();
        }
        finally { operaciones.Release(); }
    }

    public async Task SincronizarSalaAsync(string? codigo, bool forzar = false, CancellationToken ct = default)
    {
        using var vinculo = CancellationTokenSource.CreateLinkedTokenSource(ct, cierre.Token);
        vinculo.CancelAfter(TimeSpan.FromSeconds(5));
        await operaciones.WaitAsync(vinculo.Token);
        try
        {
            codigoSala = string.IsNullOrWhiteSpace(codigo) ? null : codigo.Trim().ToUpperInvariant();
            if (!cerrado) await ActualizarGrupoAsync(forzar, vinculo.Token);
        }
        finally { operaciones.Release(); }
    }

    private async Task ActualizarGrupoAsync(bool forzar, CancellationToken ct)
    {
        if (!Conectado || !forzar && grupoSincronizado == codigoSala) return;
        if (codigoSala == null) await conexion.InvokeAsync("SalirSala", ct);
        else await conexion.InvokeAsync("EntrarSala", codigoSala, ct);
        grupoSincronizado = codigoSala;
    }


public Task EnviarMensajeAsync(
    string codigo, string texto, bool soloEquipo,
    CancellationToken ct = default)
{
    return conexion.InvokeAsync(
        "EnviarMensaje", codigo, texto, soloEquipo, ct);
}

public Task EnviarReaccionAsync(
    string codigo, string tipo,
    CancellationToken ct = default)
{
    return conexion.InvokeAsync(
        "EnviarReaccion", codigo, tipo, ct);
}


    public async ValueTask DisposeAsync()
    {
        if (cerrado) return;
        cerrado = true;
        cierre.Cancel();
        await conexion.DisposeAsync();
    }
}

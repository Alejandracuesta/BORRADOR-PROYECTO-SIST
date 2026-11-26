using System.Net.Http.Json;
using QuizArena.Cliente.Models;
using QuizArena.Cliente.Services;
namespace QuizArena.Cliente;
public partial class HistorialForm : Form
{
    public HistorialForm() { InitializeComponent(); }
    public HistorialForm(HttpClient cliente) : this()
    {
        api = cliente;
        Shown += async (_, _) => await EjecutarAsync(CargarAsync);
        btnActualizar.Click += async (_, _) => await EjecutarAsync(CargarAsync);
        btnVerResultado.Click += async (_, _) => await EjecutarAsync(async () => {
            if (dgvHistorial.CurrentRow?.DataBoundItem is not HistorialApi partida) { lblResultado.Text = "Selecciona una partida."; return; }
            using var respuesta = await api.GetAsync($"api/partidas/{partida.Id}/resumen", cierre.Token); await RespuestaApi.ComprobarAsync(respuesta, cierre.Token);
            var resumen = await respuesta.Content.ReadFromJsonAsync<ResumenApi>(cancellationToken: cierre.Token);
            if (IsDisposed || Disposing || resumen is null) return;
            var lineas = new List<string> { $"Partida {resumen.Id} — {resumen.Modalidad} — {resumen.CodigoSala}", $"Resultado: {resumen.ResultadoFinal}", "" };
            foreach (var j in resumen.Participantes) lineas.Add($"{j.Nombre}: {j.Puntos} puntos, {j.Aciertos} aciertos");
            foreach (var e in resumen.Equipos) lineas.Add($"{e.Nombre}: {e.Puntos} puntos — {string.Join(", ", e.Integrantes.Select(j => j.Nombre))}");
            txtResultado.Lines = lineas.ToArray();
        });
    }
    private async Task CargarAsync() { var partidas = await api.GetFromJsonAsync<List<HistorialApi>>("api/partidas/historial", cierre.Token); if (!IsDisposed && !Disposing) { dgvHistorial.DataSource = partidas; lblResultado.Text = $"{partidas?.Count ?? 0} partidas finalizadas."; } }
    private readonly HttpClient api = null!;
    private readonly CancellationTokenSource cierre = new();
    private bool operando;
    private async Task EjecutarAsync(Func<Task> accion)
    {
        if (operando || IsDisposed || Disposing) return;
        operando = true;
        UseWaitCursor = true;
        try { await accion(); }
        catch (OperationCanceledException) when (cierre.IsCancellationRequested) { }
        catch (Exception ex) { if (!IsDisposed && !Disposing) lblResultado.Text = ex is OperationCanceledException ? "El servidor no responde." : ex.Message; }
        finally { operando = false; if (!IsDisposed && !Disposing) UseWaitCursor = false; }
    }
    protected override void OnFormClosed(FormClosedEventArgs e) { cierre.Cancel(); base.OnFormClosed(e); }
}

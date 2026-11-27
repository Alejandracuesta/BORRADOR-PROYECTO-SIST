using System.Net.Http.Json;
using QuizArena.Cliente.Services;
namespace QuizArena.Cliente;
public partial class PerfilForm : Form
{
    public PerfilForm() { InitializeComponent(); }
    public PerfilForm(HttpClient cliente) : this()
    {
        api = cliente;
        Shown += async (_, _) => await EjecutarAsync(CargarAsync);
        btnCambiarNombre.Click += async (_, _) => await EjecutarAsync(async () => {
            using var respuesta = await api.PutAsJsonAsync("api/cuenta", new { nuevoNombre = txtNuevoNombre.Text, claveActual = txtClaveNombre.Text }, cierre.Token);
            await RespuestaApi.ComprobarAsync(respuesta, cierre.Token);
            if (IsDisposed || Disposing) return;
            txtClaveNombre.Clear(); await CargarAsync(); lblResultado.Text = "Nombre actualizado.";
        });
        btnCambiarClave.Click += async (_, _) => await EjecutarAsync(async () => {
            using var respuesta = await api.PutAsJsonAsync("api/cuenta/clave", new { claveActual = txtClaveAnterior.Text, claveNueva = txtClaveNueva.Text }, cierre.Token);
            await RespuestaApi.ComprobarAsync(respuesta, cierre.Token);
            if (IsDisposed || Disposing) return;
            txtClaveAnterior.Clear(); txtClaveNueva.Clear(); lblResultado.Text = "Contraseña actualizada.";
        });
        btnEliminarCuenta.Click += async (_, _) => {
            if (operando || MessageBox.Show(this, "¿Eliminar tu cuenta? Los resultados históricos se conservarán de forma anónima.", "Eliminar cuenta", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            await EjecutarAsync(async () => {
                using var solicitud = new HttpRequestMessage(HttpMethod.Delete, "api/cuenta") { Content = JsonContent.Create(new { claveActual = txtClaveEliminar.Text }) };
                using var respuesta = await api.SendAsync(solicitud, cierre.Token); await RespuestaApi.ComprobarAsync(respuesta, cierre.Token);
                if (!IsDisposed && !Disposing) { DialogResult = DialogResult.Abort; Close(); }
            });
        };
    }
    private async Task CargarAsync()
    {
        var perfil = await api.GetFromJsonAsync<PerfilJugador>("api/cuenta", cierre.Token);
        if (IsDisposed || Disposing || perfil is null) return;
        lblUsuarioActual.Text = $"{perfil.Nombre} — ID {perfil.Id}";
        txtNuevoNombre.Text = perfil.Nombre;
    }
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

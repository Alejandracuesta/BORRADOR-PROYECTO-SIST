using System.Net.Http.Json;
using QuizArena.Cliente.Models;
using QuizArena.Cliente.Services;

namespace QuizArena.Cliente;

public partial class ChatForm : Form
{
    private readonly HttpClient api = null!;
    private readonly TiempoRealCliente tiempoReal = null!;
    private readonly string codigoSala = "";
    private bool soloEquipo;
    private bool enviando;
    private bool cargandoHistorial;
    private bool historialPendiente;
    private int jugadorId;
    private readonly CancellationTokenSource cierre = new();
    private readonly HashSet<int> mensajesMostrados = new();

    public ChatForm()
    {
        InitializeComponent();
    }

    public ChatForm(
        HttpClient cliente,
        TiempoRealCliente conexion,
        string codigo,
        bool privado) : this()
    {
        api = cliente;
        tiempoReal = conexion;
        codigoSala = codigo;
        soloEquipo = privado;

        lblAmbitoChat.Text = soloEquipo
            ? "Chat privado del equipo"
            : "Chat general de sala";

        tiempoReal.MensajeRecibido += AlRecibirMensaje;
        tiempoReal.ChatActualizado += AlCambiarSala;
        btnEditarMensaje.Click += async (_, _) => await ModificarMensajeAsync(false);
        btnEliminarMensaje.Click += async (_, _) => {
            if (lstMensajes.SelectedItem is MensajeVisible && MessageBox.Show(this, "¿Eliminar el contenido del mensaje seleccionado?", "Chat", MessageBoxButtons.YesNo) == DialogResult.Yes) await ModificarMensajeAsync(true);
        };
        lstMensajes.SelectedIndexChanged += (_, _) => {
            bool propio = lstMensajes.SelectedItem is MensajeVisible m && m.Mensaje.JugadorId == jugadorId && !m.Mensaje.Eliminado;
            btnEditarMensaje.Enabled = propio; btnEliminarMensaje.Enabled = propio;
            if (propio && lstMensajes.SelectedItem is MensajeVisible seleccionado) txtMensaje.Text = seleccionado.Mensaje.Texto;
        };
        tiempoReal.ReaccionRecibida += AlRecibirReaccion;
        tiempoReal.ConexionRestaurada += SolicitarHistorial;
        tiempoReal.SalaActualizada += AlCambiarSala;
        tiempoReal.PartidaActualizada += AlCambiarSala;

        btnEnviar.Click += async (_, _) =>
            await EnviarAsync();

        txtMensaje.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await EnviarAsync();
            }
        };

        btnAplauso.Click += async (_, _) =>
            await ReaccionarAsync("Aplauso");

        btnAnimo.Click += async (_, _) =>
            await ReaccionarAsync("Animo");

        btnSorpresa.Click += async (_, _) =>
            await ReaccionarAsync("Sorpresa");

        Shown += async (_, _) =>
            await CargarHistorialAsync();
    }

    private async Task CargarHistorialAsync()
    {
        if (cargandoHistorial) { historialPendiente = true; return; }
        cargandoHistorial = true;
        try
        {
            var perfil = await api.GetFromJsonAsync<PerfilJugador>("api/cuenta", cierre.Token);
            jugadorId = perfil?.Id ?? 0;
            var sala = await api.GetFromJsonAsync<SalaDetalleApi>($"api/salas/{codigoSala}", cierre.Token);
            if (IsDisposed || Disposing) return;
            bool privado = sala?.Modalidad == "Equipos" && sala.Estado == "EnPartida";
            if (privado != soloEquipo) { soloEquipo = privado; mensajesMostrados.Clear(); lstMensajes.Items.Clear(); }
            lblAmbitoChat.Text = soloEquipo ? "Chat privado del equipo" : "Chat general de sala";
            var mensajes = await api.GetFromJsonAsync<
                List<MensajeChatApi>>(
                $"api/chat/{codigoSala}", cierre.Token);

            if (IsDisposed || Disposing)
                return;

            int? seleccionadoId = (lstMensajes.SelectedItem as MensajeVisible)?.Mensaje.Id;
            string borrador = txtMensaje.Text;
            mensajesMostrados.Clear(); lstMensajes.Items.Clear();
            foreach (var mensaje in mensajes ?? new())
                AgregarMensaje(mensaje);
            if (seleccionadoId.HasValue)
                lstMensajes.SelectedItem = lstMensajes.Items.OfType<MensajeVisible>().FirstOrDefault(m => m.Mensaje.Id == seleccionadoId.Value);
            txtMensaje.Text = borrador;
        }
        catch (Exception ex)
        {
            if (!IsDisposed && !Disposing)
                MessageBox.Show(this, ex.Message);
        }
        finally {
            cargandoHistorial = false;
            if (historialPendiente && !cierre.IsCancellationRequested) { historialPendiente = false; SolicitarHistorial(); }
        }
    }

    private async Task EnviarAsync()
    {
        if (enviando || IsDisposed || Disposing) return;
        string texto = txtMensaje.Text.Trim();

        if (texto.Length is < 1 or > 250)
            return;

        enviando = true;
        btnEnviar.Enabled = false;

        try
        {
            await tiempoReal.EnviarMensajeAsync(
                codigoSala, texto, soloEquipo);

            if (!IsDisposed && !Disposing)
                txtMensaje.Clear();
        }
        catch (Exception ex)
        {
            if (!IsDisposed && !Disposing)
                MessageBox.Show(this, ex.Message);
        }
        finally
        {
            enviando = false;
            if (!IsDisposed && !Disposing)
                btnEnviar.Enabled = true;
        }
    }

    private async Task ReaccionarAsync(string tipo)
    {
        try
        {
            await tiempoReal.EnviarReaccionAsync(
                codigoSala, tipo);
        }
        catch (Exception ex)
        {
            if (!IsDisposed && !Disposing)
                MessageBox.Show(this, ex.Message);
        }
    }

    private void AlRecibirMensaje(MensajeChatApi mensaje)
    {
        if (IsDisposed || Disposing)
            return;

        if (InvokeRequired)
        {
            try { BeginInvoke(new Action(() => AlRecibirMensaje(mensaje))); } catch (InvalidOperationException) { }
            return;
        }

        if (cargandoHistorial) historialPendiente = true;
        AgregarMensaje(mensaje);
    }

    private void AgregarMensaje(MensajeChatApi mensaje)
    {
        if (mensaje.CodigoSala != codigoSala)
            return;

        if (soloEquipo != mensaje.EquipoPartidaId.HasValue)
            return;

        if (!mensajesMostrados.Add(mensaje.Id))
            return;

        lstMensajes.Items.Add(new MensajeVisible(mensaje));

        lstMensajes.TopIndex = lstMensajes.Items.Count - 1;
    }

    private void AlRecibirReaccion(ReaccionApi reaccion)
    {
        if (IsDisposed || Disposing)
            return;

        if (InvokeRequired)
        {
            try { BeginInvoke(new Action(() => AlRecibirReaccion(reaccion))); } catch (InvalidOperationException) { }
            return;
        }

        if (reaccion.CodigoSala == codigoSala)
        {
            lstMensajes.Items.Add(
                $"{reaccion.Nombre} reaccionó: {reaccion.Tipo}");

            lstMensajes.TopIndex = lstMensajes.Items.Count - 1;
        }
    }

    private void AlCambiarSala(string codigo) { if (codigo == codigoSala) SolicitarHistorial(); }
    private void SolicitarHistorial()
    {
        if (IsDisposed || Disposing || !IsHandleCreated) return;
        if (InvokeRequired) { try { BeginInvoke(new Action(SolicitarHistorial)); } catch (InvalidOperationException) { } return; }
        _ = CargarHistorialAsync();
    }
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        cierre.Cancel();
        if (tiempoReal != null)
        {
            tiempoReal.MensajeRecibido -= AlRecibirMensaje;
            tiempoReal.ChatActualizado -= AlCambiarSala;
            tiempoReal.ReaccionRecibida -= AlRecibirReaccion;
            tiempoReal.ConexionRestaurada -= SolicitarHistorial;
            tiempoReal.SalaActualizada -= AlCambiarSala;
            tiempoReal.PartidaActualizada -= AlCambiarSala;
        }
        base.OnFormClosed(e);
    }
    private sealed record MensajeVisible(MensajeChatApi Mensaje)
    {
        public override string ToString() => $"[{Mensaje.FechaUtc:HH:mm:ss}] {Mensaje.Nombre}: {Mensaje.Texto}" + (Mensaje.EditadoUtc.HasValue && !Mensaje.Eliminado ? " (editado)" : "");
    }
    private async Task ModificarMensajeAsync(bool eliminar)
    {
        if (enviando || lstMensajes.SelectedItem is not MensajeVisible elegido || elegido.Mensaje.JugadorId != jugadorId || elegido.Mensaje.Eliminado) return;
        enviando = true;
        try {
            using var respuesta = eliminar ? await api.DeleteAsync($"api/chat/mensajes/{elegido.Mensaje.Id}", cierre.Token) : await api.PutAsJsonAsync($"api/chat/mensajes/{elegido.Mensaje.Id}", new { texto = txtMensaje.Text }, cierre.Token);
            await RespuestaApi.ComprobarAsync(respuesta, cierre.Token);
            if (!IsDisposed && !Disposing) { txtMensaje.Clear(); await CargarHistorialAsync(); }
        } catch (Exception ex) { if (!IsDisposed && !Disposing) MessageBox.Show(this, ex.Message); }
        finally { enviando = false; }
    }
}

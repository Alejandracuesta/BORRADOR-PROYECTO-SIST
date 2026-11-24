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
        try
        {
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

            foreach (var mensaje in mensajes ?? new())
                AgregarMensaje(mensaje);
        }
        catch (Exception ex)
        {
            if (!IsDisposed && !Disposing)
                MessageBox.Show(this, ex.Message);
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

        lstMensajes.Items.Add(
            $"[{mensaje.FechaUtc:HH:mm:ss}] " +
            $"{mensaje.Nombre}: {mensaje.Texto}");

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
            tiempoReal.ReaccionRecibida -= AlRecibirReaccion;
            tiempoReal.ConexionRestaurada -= SolicitarHistorial;
            tiempoReal.SalaActualizada -= AlCambiarSala;
            tiempoReal.PartidaActualizada -= AlCambiarSala;
        }
        base.OnFormClosed(e);
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace QuizArena.Cliente;

public partial class Form1 : Form
{
    // Cada formulario conserva su propia sesión, incluso dentro del mismo proceso.
    private readonly CookieContainer cookies = new();
    private readonly HttpClient httpClient;

    private readonly CancellationTokenSource cierreFormulario = new();
    private PerfilJugador? jugadorActual;
    private bool solicitudEnCurso;
    private bool formularioCerrado;

    private bool PuedeActualizar => !formularioCerrado && !IsDisposed && !Disposing;

    public Form1()
    {
        InitializeComponent();

        // Aspecto de trivial: colores, título grande y rueda de colores.
        TemaTrivial.Aplicar(this);
        lblTitulo.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
        Controls.Add(new RuedaTrivial
        {
            Name = "ruedaTrivial",
            Location = new Point(480, 50),
            Size = new Size(140, 140)
        });

        httpClient = new HttpClient(new HttpClientHandler
{
    UseCookies = true,
    CookieContainer = cookies,
    AllowAutoRedirect = false
})
{
    BaseAddress = new Uri("http://localhost:5080/"),
    Timeout = TimeSpan.FromSeconds(5)
};

        btnRegistrar.Click += btnRegistrar_Click;
        btnEntrar.Click += btnEntrar_Click;
        btnPerfil.Click += btnPerfil_Click;
        btnSalir.Click += btnSalir_Click;
        btnSalas.Click += btnSalas_Click;
        Disposed += (_, _) => LiberarRecursos();

        ActualizarControles();
    }

    private async void btnRegistrar_Click(object? sender, EventArgs e)
    {
        await EnviarCredencialesAsync("api/acceso/registrar", iniciarSesion: false);
    }

    private async void btnEntrar_Click(object? sender, EventArgs e)
    {
        await EnviarCredencialesAsync("api/acceso/entrar", iniciarSesion: true);
    }

    private async Task EnviarCredencialesAsync(string ruta, bool iniciarSesion)
    {
        if (!PuedeActualizar || solicitudEnCurso || jugadorActual is not null)
            return;

        string nombre = txtNombre.Text.Trim();
        string clave = txtClave.Text;

        if (nombre.Length is < 3 or > 20 || clave.Length is < 6 or > 64)
        {
            MessageBox.Show(this,
                "El nombre debe tener entre 3 y 20 caracteres y la contraseña entre 6 y 64.",
                "Datos incorrectos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        await EjecutarSolicitudAsync(async cancellationToken =>
        {
            using var respuesta = await httpClient.PostAsJsonAsync(
                ruta, new { nombre, clave }, cancellationToken);

            if (!respuesta.IsSuccessStatusCode)
            {
                MostrarError(await LeerErrorAsync(respuesta, cancellationToken));
                return;
            }

            var jugador = await respuesta.Content.ReadFromJsonAsync<PerfilJugador>(
                cancellationToken: cancellationToken);

            if (!PuedeActualizar)
                return;

            if (jugador is null || !EsPerfilValido(jugador))
            {
                MostrarError("El servidor no devolvió un perfil válido.");
                return;
            }

            lblEstado.Text = "Servidor conectado";
            txtClave.Clear();

            if (iniciarSesion)
            {
                jugadorActual = jugador;
                lblJugador.Text = $"Sesión iniciada: {jugador.Nombre} (ID {jugador.Id})";
                ActualizarControles();

                MessageBox.Show(this, $"Bienvenido a Quiz Arena, {jugador.Nombre}.",
                    "Acceso correcto", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(this, "Cuenta registrada. Ya puedes iniciar sesión.",
                    "Registro correcto", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtClave.Focus();
            }
        });
    }

    private async void btnPerfil_Click(object? sender, EventArgs e)
    {
        await EjecutarSolicitudAsync(async cancellationToken =>
        {
            using var respuesta = await httpClient.GetAsync("api/acceso/yo", cancellationToken);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                CerrarSesionLocal();
                MostrarError("La sesión ha terminado. Vuelve a iniciar sesión.");
                return;
            }

            if (!respuesta.IsSuccessStatusCode)
            {
                MostrarError(await LeerErrorAsync(respuesta, cancellationToken));
                return;
            }

            var perfil = await respuesta.Content.ReadFromJsonAsync<PerfilJugador>(
                cancellationToken: cancellationToken);

            if (!PuedeActualizar)
                return;

            if (perfil is null || !EsPerfilValido(perfil))
            {
                MostrarError("No se ha recibido un perfil válido.");
                return;
            }

            jugadorActual = perfil;
            lblJugador.Text = $"Sesión iniciada: {perfil.Nombre} (ID {perfil.Id})";
            lblEstado.Text = "Servidor conectado";

            MessageBox.Show(this,
                $"ID: {perfil.Id}\nJugador: {perfil.Nombre}\nRegistro (UTC): {perfil.FechaRegistroUtc:u}",
                "Mi perfil", MessageBoxButtons.OK, MessageBoxIcon.Information);
        });
    }

    private async void btnSalir_Click(object? sender, EventArgs e)
    {
        await EjecutarSolicitudAsync(async cancellationToken =>
        {
            using var respuesta = await httpClient.PostAsync("api/acceso/salir", null, cancellationToken);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                CerrarSesionLocal();
                MostrarError("La sesión ya había terminado. Puedes volver a iniciar sesión.");
                return;
            }

            if (!respuesta.IsSuccessStatusCode)
            {
                MostrarError(await LeerErrorAsync(respuesta, cancellationToken));
                return;
            }

            if (!PuedeActualizar)
                return;

            CerrarSesionLocal();
            lblEstado.Text = "Servidor conectado";
            MessageBox.Show(this, "Sesión cerrada correctamente.", "Quiz Arena",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        });
    }

    private void btnSalas_Click(object? sender, EventArgs e)
    {
        if (!PuedeActualizar || solicitudEnCurso || jugadorActual is null) return;
        using var ventanaSalas = new SalasForm(httpClient, jugadorActual.Id, cookies);
        if (ventanaSalas.ShowDialog(this) == DialogResult.Abort && PuedeActualizar)
        {
            CerrarSesionLocal();
            lblEstado.Text = "Sesión finalizada";
        }
    }

    // Conserva la comprobación de conexión de la fase 1.
    private async void btnComprobar_Click(object? sender, EventArgs e)
    {
        await EjecutarSolicitudAsync(async cancellationToken =>
        {
            lblEstado.Text = "Conectando...";

            using var respuesta = await httpClient.GetAsync("api/estado", cancellationToken);
            respuesta.EnsureSuccessStatusCode();
            string contenido = await respuesta.Content.ReadAsStringAsync(cancellationToken);

            if (!PuedeActualizar)
                return;

            lblEstado.Text = "Servidor conectado";
            MessageBox.Show(this, contenido, "Respuesta de Quiz Arena",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        });
    }

    private async Task EjecutarSolicitudAsync(Func<CancellationToken, Task> accion)
    {
        if (!PuedeActualizar || solicitudEnCurso)
            return;

        solicitudEnCurso = true;
        ActualizarControles();

        try
        {
            await accion(cierreFormulario.Token);
        }
        catch (OperationCanceledException) when (formularioCerrado)
        {
            // Todas las operaciones se cancelan al cerrar, sin mensajes tardíos.
        }
        catch (OperationCanceledException)
        {
            MostrarError("El servidor no ha respondido en 5 segundos.", "Sin conexión");
        }
        catch (HttpRequestException ex)
        {
            MostrarError(ex.Message, "Sin conexión");
        }
        catch (JsonException)
        {
            MostrarError("La respuesta del servidor no es válida.");
        }
        catch (ObjectDisposedException) when (formularioCerrado)
        {
            // El cliente HTTP ya se ha liberado al cerrar la ventana.
        }
        finally
        {
            solicitudEnCurso = false;
            ActualizarControles();
        }
    }

    private void ActualizarControles()
    {
        if (!PuedeActualizar)
            return;

        bool sinSesion = jugadorActual is null;
        bool disponible = !solicitudEnCurso;
        txtNombre.Enabled = sinSesion && disponible;
        txtClave.Enabled = sinSesion && disponible;
        btnRegistrar.Enabled = sinSesion && disponible;
        btnEntrar.Enabled = sinSesion && disponible;
        btnPerfil.Enabled = !sinSesion && disponible;
        btnSalir.Enabled = !sinSesion && disponible;
        btnSalas.Enabled = !sinSesion && disponible;
        btnComprobar.Enabled = disponible;
        UseWaitCursor = solicitudEnCurso;
    }

    private void CerrarSesionLocal()
    {
        jugadorActual = null;

        if (!PuedeActualizar)
            return;

        lblJugador.Text = "Sin sesión iniciada";
        txtNombre.Clear();
        txtClave.Clear();
        ActualizarControles();
    }

    private static bool EsPerfilValido(PerfilJugador? perfil)
    {
        return perfil is not null && perfil.Id > 0 && !string.IsNullOrWhiteSpace(perfil.Nombre);
    }

    private static async Task<string> LeerErrorAsync(
        HttpResponseMessage respuesta, CancellationToken cancellationToken)
    {
        string contenido = await respuesta.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            using var documento = JsonDocument.Parse(contenido);
            var raiz = documento.RootElement;

            if (raiz.ValueKind == JsonValueKind.Object &&
                raiz.TryGetProperty("mensaje", out var mensaje) &&
                mensaje.ValueKind == JsonValueKind.String)
            {
                return mensaje.GetString() ?? "Error desconocido";
            }
        }
        catch (JsonException)
        {
            // Si no es JSON, conservar el código HTTP y el texto recibido.
        }

        return $"HTTP {(int)respuesta.StatusCode}: {contenido}";
    }

    private void MostrarError(string mensaje, string estado = "Error de comunicación")
    {
        if (!PuedeActualizar)
            return;

        lblEstado.Text = estado;
        MessageBox.Show(this, mensaje, "Quiz Arena",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void LiberarRecursos()
    {
        if (formularioCerrado)
            return;

        formularioCerrado = true;
        cierreFormulario.Cancel();
        httpClient.Dispose();
        cierreFormulario.Dispose();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        LiberarRecursos();
        base.OnFormClosed(e);
    }
}

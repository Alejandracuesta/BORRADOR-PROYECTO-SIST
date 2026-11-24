using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using QuizArena.Cliente.Models;
using QuizArena.Cliente.Services;

namespace QuizArena.Cliente;

public partial class SalasForm : Form
{
    // El cliente autenticado pertenece a Form1; esta ventana no lo libera.
    private readonly HttpClient api = null!;
    private readonly int jugadorId;
    private readonly System.Windows.Forms.Timer temporizador = new() { Interval = 10000 };
    private readonly CancellationTokenSource cierre = new();
    private SalaDetalleApi? salaActual;
    private string? ultimaConfiguracion;
    private TiempoRealCliente? tiempoReal;
    private bool refrescoPendiente;
    private bool reconectando;
    private bool ocupado;
    private bool modificando;
    private bool cerrada;
    private bool categoriasCargadas;
    private bool partidaAbierta;
    private bool aperturaPartidaPendiente;
    private string? codigoPartidaAbiertaAutomaticamente;

    private bool PuedeActualizar => !cerrada && !IsDisposed && !Disposing;

    // Constructor para el diseñador de Windows Forms.
    public SalasForm()
    {
        InitializeComponent();
        Disposed += (_, _) => LiberarRecursos();
    }

    public SalasForm(HttpClient cliente, int idJugador, CookieContainer cookies) : this()
    {
        ArgumentNullException.ThrowIfNull(cliente);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(idJugador);
        api = cliente;
        jugadorId = idJugador;
        tiempoReal = new TiempoRealCliente(api.BaseAddress!, cookies);
        tiempoReal.SalaActualizada += _ => SolicitarRefresco();
        tiempoReal.SalasPublicasActualizadas += SolicitarRefresco;
        tiempoReal.PartidaActualizada += _ => SolicitarRefresco();
        tiempoReal.ConexionRestaurada += SolicitarRefresco;
        tiempoReal.ConexionPerdida += SolicitarRefresco;
        tiempoReal.ConexionRestaurada += MostrarEstadoConexion;
        tiempoReal.ConexionPerdida += MostrarEstadoConexion;
        FormClosed += async (_, _) => { await tiempoReal.DisposeAsync(); };

        cboModalidad.Items.AddRange(new object[] { "Individual", "Equipos" });
        cboModalidad.SelectedIndex = 0;
        cboPreguntas.Items.AddRange(new object[] { 5, 10, 15 });
        cboPreguntas.SelectedItem = 5;
        cboDificultad.Items.AddRange(new object[] { "Todas", "Fácil", "Media", "Difícil" });
        cboDificultad.SelectedIndex = 0;
        cboCategorias.Items.Add(new CategoriaApi(0, "Todas", 0));
        cboCategorias.SelectedIndex = 0;

        cboModalidad.SelectedIndexChanged += (_, _) => AjustarAforo();
        lstPublicas.SelectedIndexChanged += (_, _) => ActualizarControles();
        btnActualizarSalas.Click += async (_, _) => await RefrescarAsync(true);
        btnCrearSala.Click += async (_, _) =>
        {
            var datos = DatosSala();
            await EjecutarCambioAsync(ct => api.PostAsJsonAsync("api/salas", datos, ct));
        };
        btnGuardarConfig.Click += async (_, _) =>
        {
            if (salaActual is null) return;
            string codigo = salaActual.Codigo;
            var datos = DatosSala();
            await EjecutarCambioAsync(ct => api.PutAsJsonAsync($"api/salas/{codigo}/configuracion", datos, ct));
        };
        btnUnirsePublica.Click += async (_, _) =>
        {
            if (lstPublicas.SelectedItem is SalaPublicaApi sala)
                await UnirseAsync(sala.Codigo);
        };
        btnUnirseCodigo.Click += async (_, _) => await UnirseConCodigoAsync();
        txtCodigo.TextChanged += (_, _) => ActualizarValidacionCodigo();
        txtCodigo.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await UnirseConCodigoAsync();
            }
        };
        btnCopiarCodigo.Click += (_, _) =>
        {
            if (salaActual is null) return;
            try
            {
                Clipboard.SetText(salaActual.Codigo);
                lblAviso.Text = "Código copiado al portapapeles";
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
                lblAviso.Text = "No se pudo acceder al portapapeles";
            }
        };
        btnPreparado.Click += async (_, _) =>
        {
            if (salaActual is null) return;
            var yo = salaActual.Participantes.FirstOrDefault(p => p.JugadorId == jugadorId);
            if (yo is null) return;
            string codigo = salaActual.Codigo;
            bool preparado = !yo.Preparado;
            await EjecutarCambioAsync(ct => api.PutAsJsonAsync(
                $"api/salas/{codigo}/preparado", new { preparado }, ct));
        };
        btnSalirSala.Click += async (_, _) =>
        {
            if (salaActual is null) return;
            string codigo = salaActual.Codigo;
            await EjecutarCambioAsync(ct => api.DeleteAsync($"api/salas/{codigo}/salir", ct));
        };
        btnIniciarPartida.Click += async (_, _) =>
        {
            if (salaActual is null) return;
            string codigo = salaActual.Codigo;
            await EjecutarCambioAsync(ct => api.PostAsync($"api/partidas/{codigo}/iniciar", null, ct));
        };
        btnRevancha.Click += async (_, _) =>
{
    if (salaActual == null)
        return;

    string codigo = salaActual.Codigo;

    await EjecutarCambioAsync(ct =>
        api.PostAsync(
            $"api/salas/{codigo}/revancha", null, ct));
};

        btnClasificacion.Click += (_, _) =>
{
    using var ventana = new ClasificacionForm(api);
    ventana.ShowDialog(this);
};

        btnChatSala.Click += (_, _) =>
        {
            if (salaActual == null || tiempoReal == null) return;
            new ChatForm(api, tiempoReal, salaActual.Codigo, salaActual.Modalidad == "Equipos" && salaActual.Estado == "EnPartida").Show(this);
        };
        btnVerPartida.Click += (_, _) => AbrirVentanaPartida();
        temporizador.Tick += async (_, _) => { await RecuperarTiempoRealAsync(); await RefrescarAsync(false); };
        Shown += async (_, _) => { await RecuperarTiempoRealAsync(); await RefrescarAsync(true); };

        AjustarAforo();
        MostrarSala(null);
        ActualizarValidacionCodigo();
    }

    private void MostrarEstadoConexion()
    {
        if (!PuedeActualizar || !IsHandleCreated) return;
        if (InvokeRequired) { try { BeginInvoke(new Action(MostrarEstadoConexion)); } catch (InvalidOperationException) { } return; }
        lblConexion.Text = tiempoReal?.Conectado == true ? "Tiempo real conectado" : "Sin SignalR: utilizando REST de respaldo";
    }

    private void SolicitarRefresco()
    {
        if (!PuedeActualizar || !IsHandleCreated) return;
        if (InvokeRequired)
        {
            try { BeginInvoke(new Action(SolicitarRefresco)); } catch (InvalidOperationException) { }
            return;
        }
        if (partidaAbierta) return;
        if (ocupado) { refrescoPendiente = true; return; }
        _ = RefrescarAsync(false);
    }

    private async Task RecuperarTiempoRealAsync()
    {
        if (reconectando || tiempoReal == null || !PuedeActualizar) return;
        reconectando = true;
        try { await tiempoReal.IniciarAsync(cierre.Token); }
        catch (Exception) when (!cerrada) { lblAviso.Text = "SignalR desconectado. Funcionando por REST."; }
        catch (OperationCanceledException) { }
        finally { reconectando = false; MostrarEstadoConexion(); }
    }

    private bool CodigoValido()
    {
        string codigo = txtCodigo.Text.Trim();
        return codigo.Length == 8 && codigo.All(Uri.IsHexDigit);
    }

    private void ActualizarValidacionCodigo()
    {
        lblCodigoAyuda.Text = CodigoValido()
            ? "Código válido para intentar conectar"
            : "Introduce exactamente 8 caracteres (0-9, A-F)";
        ActualizarControles();
    }

    private Task UnirseConCodigoAsync()
    {
        if (!CodigoValido() || salaActual is not null || ocupado || partidaAbierta || !PuedeActualizar)
            return Task.CompletedTask;
        return UnirseAsync(txtCodigo.Text.Trim().ToUpperInvariant());
    }

    private void AbrirVentanaPartida()
    {
        if (!PuedeActualizar || ocupado || partidaAbierta || salaActual?.Estado is not ("EnPartida" or "Finalizada")) return;
        partidaAbierta = true;
        codigoPartidaAbiertaAutomaticamente = salaActual.Codigo;
        temporizador.Stop();
        try
        {
            using var ventana = new PartidaForm(api, jugadorId, tiempoReal);
            if (ventana.ShowDialog(this) == DialogResult.Abort)
            {
                DialogResult = DialogResult.Abort;
                Close();
            }
        }
        finally
        {
            partidaAbierta = false;
            if (PuedeActualizar) _ = RefrescarAsync(true);
        }
    }

    private void ComprobarEntradaAutomatica()
    {
        if (!PuedeActualizar || ocupado || partidaAbierta || aperturaPartidaPendiente ||
            salaActual?.Estado != "EnPartida" || codigoPartidaAbiertaAutomaticamente == salaActual.Codigo) return;
        aperturaPartidaPendiente = true;
        BeginInvoke(new Action(() =>
        {
            aperturaPartidaPendiente = false;
            if (PuedeActualizar && !ocupado && salaActual?.Estado == "EnPartida") AbrirVentanaPartida();
        }));
    }

    private void AjustarAforo()
    {
        bool equipos = cboModalidad.Text == "Equipos";
        nudMaxJugadores.Maximum = equipos ? 12 : 6;
        nudMaxJugadores.Minimum = equipos ? 4 : 2;
    }

    private object DatosSala()
    {
        int? categoriaId = cboCategorias.SelectedItem is CategoriaApi { Id: > 0 } categoria
            ? categoria.Id : null;
        int? dificultad = cboDificultad.SelectedIndex switch { 1 => 1, 2 => 2, 3 => 3, _ => null };
        return new
        {
            nombre = txtNombreSala.Text.Trim(),
            esPrivada = chkPrivada.Checked,
            modalidad = cboModalidad.Text,
            maxJugadores = (int)nudMaxJugadores.Value,
            numeroPreguntas = cboPreguntas.SelectedItem is int numero ? numero : 5,
            categoriaId,
            dificultad
        };
    }

    private Task UnirseAsync(string codigo)
    {
        string codigoSeguro = Uri.EscapeDataString(codigo);
        return EjecutarCambioAsync(ct => api.PostAsync($"api/salas/{codigoSeguro}/unirse", null, ct));
    }

    private Task RefrescarAsync(bool mostrarErrores)
        => EjecutarOperacionAsync(CargarDatosAsync, mostrarErrores);

    private Task EjecutarCambioAsync(Func<CancellationToken, Task<HttpResponseMessage>> operacion)
        => EjecutarOperacionAsync(async ct =>
        {
            using var respuesta = await operacion(ct);
            await ComprobarRespuestaAsync(respuesta, ct);
            await CargarDatosAsync(ct);
        }, true, true);

    // Una operación por ventana: el sondeo no se solapa con los cambios del usuario.
    private async Task EjecutarOperacionAsync(
        Func<CancellationToken, Task> operacion, bool mostrarErrores, bool cambio = false)
    {
        if (!PuedeActualizar || ocupado || partidaAbierta) return;
        ocupado = true;
        modificando = cambio;
        temporizador.Stop();
        ActualizarControles();
        try
        {
            await operacion(cierre.Token);
        }
        catch (OperationCanceledException) when (cerrada)
        {
            // Cerrar la ventana cancela las peticiones sin abandonar la sala.
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            if (PuedeActualizar)
            {
                MostrarError("La sesión ha terminado. Vuelve a iniciar sesión.");
                DialogResult = DialogResult.Abort;
                Close();
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException)
        {
            if (PuedeActualizar)
            {
                lblEstadoSala.Text = "Error de comunicación";
                if (mostrarErrores)
                    MostrarError(ex is OperationCanceledException ? "El servidor no ha respondido a tiempo." : ex.Message);
            }
        }
        catch (ObjectDisposedException) when (cerrada)
        {
        }
        finally
        {
            ocupado = false;
            modificando = false;
            if (PuedeActualizar)
            {
                ActualizarControles();
                temporizador.Start();
                // Abrir solo después de completar TODAS las peticiones de la sala;
                // BeginInvoke dentro de MostrarSala podría adelantarse al await siguiente.
                ComprobarEntradaAutomatica();
                if (refrescoPendiente) { refrescoPendiente = false; BeginInvoke(new Action(SolicitarRefresco)); }
            }
        }
    }

    private async Task ComprobarRespuestaAsync(HttpResponseMessage respuesta, CancellationToken ct)
    {
        if (!respuesta.IsSuccessStatusCode)
            throw new HttpRequestException(await LeerErrorAsync(respuesta, ct), null, respuesta.StatusCode);
    }

    private async Task CargarDatosAsync(CancellationToken ct)
    {
        if (!categoriasCargadas)
        {
            using var respuesta = await api.GetAsync("api/categorias", ct);
            await ComprobarRespuestaAsync(respuesta, ct);
            var categorias = await respuesta.Content.ReadFromJsonAsync<List<CategoriaApi>>(cancellationToken: ct) ?? new();
            if (!PuedeActualizar) return;
            cboCategorias.Items.Clear();
            cboCategorias.Items.Add(new CategoriaApi(0, "Todas", 0));
            foreach (var categoria in categorias) cboCategorias.Items.Add(categoria);
            cboCategorias.SelectedIndex = 0;
            categoriasCargadas = true;
        }

        using (var respuesta = await api.GetAsync("api/salas/mi-sala", ct))
        {
            if (respuesta.StatusCode == HttpStatusCode.NotFound)
            {
                MostrarSala(null);
            }
            else
            {
                await ComprobarRespuestaAsync(respuesta, ct);
                var sala = await respuesta.Content.ReadFromJsonAsync<SalaDetalleApi>(cancellationToken: ct);
                MostrarSala(sala);
            }
        }
        if (!PuedeActualizar) return;

        if (tiempoReal != null)
        {
            try { await tiempoReal.SincronizarSalaAsync(salaActual?.Codigo, ct: ct); }
            catch (Exception) when (!cerrada) { /* REST sigue disponible. */ }
        }
        using var publicas = await api.GetAsync("api/salas/publicas", ct);
        await ComprobarRespuestaAsync(publicas, ct);
        var salas = await publicas.Content.ReadFromJsonAsync<List<SalaPublicaApi>>(cancellationToken: ct) ?? new();
        if (!PuedeActualizar) return;
        string? seleccion = (lstPublicas.SelectedItem as SalaPublicaApi)?.Codigo;
        lstPublicas.BeginUpdate();
        try
        {
            lstPublicas.Items.Clear();
            foreach (var sala in salas)
            {
                lstPublicas.Items.Add(sala);
                if (sala.Codigo == seleccion) lstPublicas.SelectedItem = sala;
            }
        }
        finally { lstPublicas.EndUpdate(); }
    }

    private void ActualizarControles()
    {
        if (!PuedeActualizar) return;
        bool enSala = salaActual is not null;
        bool enEspera = salaActual?.Estado == "EnEspera";
        bool anfitrion = salaActual?.CreadorId == jugadorId;
        bool enPartida = salaActual?.Estado == "EnPartida";
        bool finalizada = salaActual?.Estado == "Finalizada";
        int minimo = salaActual?.Modalidad == "Equipos" ? 4 : 2;
        bool preparados = salaActual is not null && salaActual.Participantes.Count >= minimo &&
            salaActual.Participantes.All(p => p.Preparado);
        bool disponible = !ocupado;
        // Los sondeos no quitan el foco a los campos que se están editando.
        grpConfiguracion.Enabled = !modificando && (!enSala || anfitrion && enEspera);
        btnCrearSala.Enabled = disponible && !enSala;
        btnGuardarConfig.Enabled = disponible && anfitrion && enEspera;
        btnActualizarSalas.Enabled = disponible;
        btnUnirsePublica.Enabled = disponible && !enSala && lstPublicas.SelectedItem is SalaPublicaApi;
        btnUnirseCodigo.Enabled = disponible && !enSala && CodigoValido();
        txtCodigo.Enabled = !modificando && !enSala;
        btnPreparado.Enabled = disponible && enSala && enEspera;
        btnSalirSala.Enabled = disponible && enSala && !enPartida;
        btnIniciarPartida.Enabled = disponible && anfitrion && enEspera && preparados;
        btnVerPartida.Enabled = disponible && enSala && (enPartida || finalizada);
        btnCopiarCodigo.Enabled = enSala;
        int minimoRevancha =
    salaActual?.Modalidad == "Equipos" ? 4 : 2;

btnRevancha.Enabled =
    !ocupado &&
    salaActual?.Estado == "Finalizada" &&
    salaActual.CreadorId == jugadorId &&
    salaActual.Participantes.Count >= minimoRevancha;

        btnChatSala.Enabled = enSala && tiempoReal?.Conectado == true;
        UseWaitCursor = modificando;
    }

    private void MostrarSala(SalaDetalleApi? sala)
    {
        if (!PuedeActualizar) return;
        salaActual = sala;
        lstParticipantes.Items.Clear();
        if (sala is null)
        {
            lblSala.Text = "Ninguna sala activa";
            lblEstadoSala.Text = "Crea o busca una sala";
            btnPreparado.Text = "Estoy preparado";
            ultimaConfiguracion = null;
            codigoPartidaAbiertaAutomaticamente = null;
            return;
        }

        if (sala.Estado == "EnEspera") codigoPartidaAbiertaAutomaticamente = null;
        lblSala.Text = $"{sala.Nombre} — Código: {sala.Codigo}";
        foreach (var participante in sala.Participantes)
        {
            string anfitrion = participante.EsAnfitrion ? " [Anfitrión]" : "";
            string estado = participante.Preparado ? "Preparado" : "Pendiente";
            lstParticipantes.Items.Add($"{participante.Nombre}{anfitrion} — {estado}");
        }
        var yo = sala.Participantes.FirstOrDefault(p => p.JugadorId == jugadorId);
        btnPreparado.Text = yo?.Preparado == true ? "Cancelar preparado" : "Estoy preparado";
        int minimo = sala.Modalidad == "Equipos" ? 4 : 2;
        lblEstadoSala.Text = sala.Estado switch
        {
            "EnPartida" => "Partida en curso. Pulsa Entrar en partida.",
            "Finalizada" => "Partida terminada. Puedes consultar los resultados.",
            _ when sala.Participantes.Count >= minimo && sala.Participantes.All(p => p.Preparado)
                => "Todos preparados. El anfitrión puede iniciar.",
            _ => $"Esperando jugadores — {sala.Participantes.Count}/{sala.MaxJugadores}"
        };

        string configuracion = JsonSerializer.Serialize(new
        {
            sala.Nombre, sala.Modalidad, sala.MaxJugadores, sala.NumeroPreguntas,
            sala.CategoriaId, sala.Dificultad, sala.EsPrivada
        });
        // El sondeo no sobrescribe los cambios que el anfitrión está editando.
        if (configuracion != ultimaConfiguracion)
        {
            txtNombreSala.Text = sala.Nombre;
            cboModalidad.SelectedItem = sala.Modalidad;
            AjustarAforo();
            nudMaxJugadores.Value = sala.MaxJugadores;
            cboPreguntas.SelectedItem = sala.NumeroPreguntas;
            chkPrivada.Checked = sala.EsPrivada;
            cboDificultad.SelectedIndex = sala.Dificultad switch { "Facil" => 1, "Media" => 2, "Dificil" => 3, _ => 0 };
            cboCategorias.SelectedIndex = 0;
            foreach (var item in cboCategorias.Items)
                if (item is CategoriaApi categoria && categoria.Id == sala.CategoriaId)
                    cboCategorias.SelectedItem = item;
            ultimaConfiguracion = configuracion;
        }
    }

    private static async Task<string> LeerErrorAsync(HttpResponseMessage respuesta, CancellationToken ct)
    {
        string contenido = await respuesta.Content.ReadAsStringAsync(ct);
        try
        {
            using var documento = JsonDocument.Parse(contenido);
            var raiz = documento.RootElement;
            if (raiz.ValueKind == JsonValueKind.Object)
            {
                if (raiz.TryGetProperty("mensaje", out var mensaje) && mensaje.ValueKind == JsonValueKind.String)
                    return mensaje.GetString() ?? "Error desconocido";
                if (raiz.TryGetProperty("errors", out var errores) && errores.ValueKind == JsonValueKind.Object)
                    foreach (var error in errores.EnumerateObject())
                        if (error.Value.ValueKind == JsonValueKind.Array && error.Value.GetArrayLength() > 0)
                            return error.Value[0].GetString() ?? "Datos inválidos";
            }
        }
        catch (JsonException) { }
        return respuesta.StatusCode == HttpStatusCode.Forbidden
            ? "No tienes permiso para realizar esta operación."
            : $"El servidor devolvió un error (HTTP {(int)respuesta.StatusCode}).";
    }

    private void MostrarError(string mensaje)
    {
        if (PuedeActualizar)
            MessageBox.Show(this, mensaje, "Quiz Arena", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void LiberarRecursos()
    {
        if (cerrada) return;
        cerrada = true;
        temporizador.Stop();
        temporizador.Dispose();
        cierre.Cancel();
        cierre.Dispose();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        LiberarRecursos();
        base.OnFormClosed(e);
    }
}

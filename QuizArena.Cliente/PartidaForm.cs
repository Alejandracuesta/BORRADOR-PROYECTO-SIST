using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using QuizArena.Cliente.Models;

namespace QuizArena.Cliente;

public partial class PartidaForm : Form
{
    private readonly HttpClient api = null!;
    private readonly int jugadorId;
    private readonly System.Windows.Forms.Timer temporizador = new() { Interval = 1000 };
    private readonly CancellationTokenSource cierre = new();
    private readonly Stopwatch desdeEstado = new();
    private EstadoPartidaApi? estadoActual;
    private TimeSpan restanteAlRecibir;
    private bool ocupado;
    private bool estadoConfirmado;
    private bool cerrada;
    private bool PuedeActualizar => !cerrada && !IsDisposed && !Disposing;

    // Constructor sin peticiones HTTP, compatible con el diseñador.
    public PartidaForm()
    {
        InitializeComponent();
        Disposed += (_, _) => LiberarRecursos();
    }

    public PartidaForm(HttpClient cliente, int idJugador) : this()
    {
        ArgumentNullException.ThrowIfNull(cliente);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(idJugador);
        api = cliente;
        jugadorId = idJugador;
        btnA.Click += async (_, _) => await ResponderAsync("A");
        btnB.Click += async (_, _) => await ResponderAsync("B");
        btnC.Click += async (_, _) => await ResponderAsync("C");
        btnD.Click += async (_, _) => await ResponderAsync("D");
        btnCerrarPartida.Click += (_, _) => Close();
        temporizador.Tick += async (_, _) =>
        {
            ActualizarTiempo();
            await EjecutarAsync(CargarEstadoAsync);
        };
        Shown += async (_, _) => await EjecutarAsync(CargarEstadoAsync);
        HabilitarRespuestas(false);
    }

    // Consultas y respuestas comparten una única operación; el sondeo no puede
    // restaurar botones a partir de un estado anterior al envío.
    private async Task EjecutarAsync(Func<CancellationToken, Task> operacion)
    {
        if (!PuedeActualizar || ocupado) return;
        ocupado = true;
        temporizador.Stop();
        HabilitarRespuestas(false);
        try
        {
            await operacion(cierre.Token);
        }
        catch (OperationCanceledException) when (cerrada) { }
        catch (ObjectDisposedException) when (cerrada) { }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            if (PuedeActualizar)
            {
                DialogResult = DialogResult.Abort;
                Close();
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException)
        {
            estadoConfirmado = false;
            if (PuedeActualizar)
                lblInfo.Text = ex is OperationCanceledException
                    ? "El servidor no ha respondido a tiempo. Reintentando..."
                    : "No se ha podido actualizar la partida: " + ex.Message;
        }
        finally
        {
            ocupado = false;
            if (PuedeActualizar)
            {
                ActualizarTiempo();
                ActualizarPermisos();
                if (estadoActual?.Fase != "Finalizada") temporizador.Start();
            }
        }
    }

    private async Task CargarEstadoAsync(CancellationToken ct)
    {
        var viaje = Stopwatch.StartNew();
        using var respuesta = await api.GetAsync("api/partidas/actual", ct);
        await ComprobarRespuestaAsync(respuesta, ct);
        var estado = await respuesta.Content.ReadFromJsonAsync<EstadoPartidaApi>(cancellationToken: ct);
        if (!PuedeActualizar || estado is null) return;
        estadoActual = estado;
        estadoConfirmado = true;
        // Hora del servidor y tiempo monotónico: un reloj local adelantado o un
        // cambio de zona horaria no modifica el plazo mostrado.
        restanteAlRecibir = estado.FinFaseUtc.HasValue
            ? estado.FinFaseUtc.Value - estado.ServidorUtc - viaje.Elapsed / 2
            : TimeSpan.Zero;
        desdeEstado.Restart();
        MostrarEstado(estado);
    }

    private Task ResponderAsync(string opcion)
    {
        if (!PuedeResponder()) return Task.CompletedTask;
        int preguntaId = estadoActual!.Pregunta!.Id;
        return EjecutarAsync(async ct =>
        {
            estadoConfirmado = false;
            using var respuesta = await api.PostAsJsonAsync("api/partidas/responder", new { preguntaId, opcion }, ct);
            if (respuesta.StatusCode == HttpStatusCode.Conflict)
            {
                // El servidor puede haber cerrado la ronda o recibido un envío
                // cuyo acuse no llegó al cliente. Recuperamos el estado oficial.
                await CargarEstadoAsync(ct);
                return;
            }
            await ComprobarRespuestaAsync(respuesta, ct);
            await CargarEstadoAsync(ct);
        });
    }

    private void MostrarEstado(EstadoPartidaApi estado)
    {
        lblFase.Text = $"Pregunta {estado.NumeroPregunta} de {estado.TotalPreguntas}";
        if (estado.Fase == "CuentaAtras")
        {
            lblFase.Text = "Cuenta atrás";
            lblPregunta.Text = "La partida está a punto de comenzar.";
            lblInfo.Text = "Esperando la primera pregunta...";
            LimpiarOpciones();
        }
        else if (estado.Fase == "Finalizada")
        {
            lblFase.Text = "Clasificación final";
            lblPregunta.Text = "¡Partida terminada!";
            lblInfo.Text = "Puedes volver a la sala y abandonarla.";
            LimpiarOpciones();
        }
        else if (estado.Pregunta is { } p)
        {
            lblPregunta.Text = $"{p.Enunciado}\n{p.Categoria} · {p.Dificultad}";
            btnA.Text = $"A. {p.OpcionA}";
            btnB.Text = $"B. {p.OpcionB}";
            btnC.Text = $"C. {p.OpcionC}";
            btnD.Text = $"D. {p.OpcionD}";
            lblInfo.Text = estado.Fase == "Resultados"
                ? $"Respuesta correcta: {estado.Resultado?.RespuestaCorrecta ?? "?"}. Resultados de la ronda."
                : estado.MiRespuesta is null
                    ? "Selecciona una respuesta antes de que termine el tiempo."
                    : $"Respuesta enviada: {estado.MiRespuesta}. Esperando al resto de jugadores.";
        }
        MostrarMarcador(estado);
        ActualizarTiempo();
    }

    private void MostrarMarcador(EstadoPartidaApi estado)
    {
        var lineas = new List<string>();
        if (estado.Fase == "Resultados" && estado.Resultado is { } resultado)
        {
            lineas.Add("RESULTADOS DE LA PREGUNTA");
            foreach (var r in resultado.Respuestas)
                lineas.Add($"{r.Nombre}: {r.Opcion ?? "Sin respuesta"} — " +
                    $"{(r.EsCorrecta ? "Correcta" : "Incorrecta o sin responder")} (+{r.PuntosGanados})");
            lineas.Add("");
        }
        lineas.Add(estado.Fase == "Finalizada" ? "CLASIFICACIÓN FINAL" : "CLASIFICACIÓN");
        int posicion = 0;
        int? puntosAnteriores = null;
        for (int i = 0; i < estado.Marcador.Count; i++)
        {
            var jugador = estado.Marcador[i];
            if (puntosAnteriores != jugador.Puntos) posicion = i + 1;
            puntosAnteriores = jugador.Puntos;
            string marca = jugador.JugadorId == jugadorId ? " [TÚ]" : "";
            lineas.Add($"{posicion}. {jugador.Nombre}{marca} — {jugador.Puntos} puntos · " +
                $"{jugador.Aciertos} aciertos · racha {jugador.Racha}");
        }
        string texto = string.Join(Environment.NewLine, lineas);
        if (txtMarcador.Text != texto) txtMarcador.Text = texto;
    }

    private TimeSpan TiempoRestante => restanteAlRecibir - desdeEstado.Elapsed;
    private void ActualizarTiempo()
    {
        if (!PuedeActualizar) return;
        if (estadoActual?.FinFaseUtc is null) lblTiempo.Text = "--:--";
        else
        {
            int segundos = (int)Math.Max(0, Math.Ceiling(TiempoRestante.TotalSeconds));
            lblTiempo.Text = $"{segundos / 60:00}:{segundos % 60:00}";
        }
        ActualizarPermisos();
    }

    private bool PuedeResponder() => PuedeActualizar && !ocupado && estadoConfirmado &&
        estadoActual is { Fase: "Pregunta", Pregunta: not null, MiRespuesta: null, FinFaseUtc: not null } &&
        TiempoRestante > TimeSpan.Zero;
    private void ActualizarPermisos() => HabilitarRespuestas(PuedeResponder());
    private void HabilitarRespuestas(bool habilitar)
    {
        if (!PuedeActualizar) return;
        btnA.Enabled = btnB.Enabled = btnC.Enabled = btnD.Enabled = habilitar;
    }
    private void LimpiarOpciones()
    {
        btnA.Text = "A"; btnB.Text = "B"; btnC.Text = "C"; btnD.Text = "D";
    }
    private static async Task ComprobarRespuestaAsync(HttpResponseMessage respuesta, CancellationToken ct)
    {
        if (respuesta.IsSuccessStatusCode) return;
        string mensaje = $"HTTP {(int)respuesta.StatusCode}";
        try
        {
            using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync(ct));
            if (json.RootElement.TryGetProperty("mensaje", out var valor)) mensaje = valor.GetString() ?? mensaje;
        }
        catch (JsonException) { }
        throw new HttpRequestException(mensaje, null, respuesta.StatusCode);
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

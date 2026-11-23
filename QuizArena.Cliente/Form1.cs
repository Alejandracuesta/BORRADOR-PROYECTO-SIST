using System.Net.Http;

namespace QuizArena.Cliente;

public partial class Form1 : Form
{
    // Se reutiliza durante toda la vida del cliente, también entre comprobaciones.
    private static readonly HttpClient httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(5)
    };

    private CancellationTokenSource? consultaActual;

    public Form1()
    {
        InitializeComponent();
    }

    private async void btnComprobar_Click(object? sender, EventArgs e)
    {
        btnComprobar.Enabled = false;
        lblEstado.Text = "Conectando...";

        using var cancelacion = new CancellationTokenSource();
        consultaActual = cancelacion;

        try
        {
            const string url = "http://localhost:5080/api/estado";
            using HttpResponseMessage respuesta =
                await httpClient.GetAsync(url, cancelacion.Token);

            respuesta.EnsureSuccessStatusCode();
            string contenido = await respuesta.Content.ReadAsStringAsync(cancelacion.Token);

            if (IsDisposed || Disposing)
                return;

            lblEstado.Text = "Servidor conectado";
            MessageBox.Show(this, contenido, "Respuesta de Quiz Arena",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (OperationCanceledException) when (cancelacion.IsCancellationRequested)
        {
            // Al cerrar el formulario se cancela la petición sin mostrar mensajes.
        }
        catch (OperationCanceledException)
        {
            MostrarError("El servidor no ha respondido en 5 segundos. Comprueba que QuizArena.Servidor esté iniciado.");
        }
        catch (HttpRequestException ex)
        {
            MostrarError(ex.Message);
        }
        finally
        {
            consultaActual = null;

            if (!IsDisposed && !Disposing)
                btnComprobar.Enabled = true;
        }
    }

    private void MostrarError(string mensaje)
    {
        if (IsDisposed || Disposing)
            return;

        lblEstado.Text = "Sin conexión";
        MessageBox.Show(this, mensaje, "Error de conexión",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        consultaActual?.Cancel();
        base.OnFormClosed(e);
    }
}

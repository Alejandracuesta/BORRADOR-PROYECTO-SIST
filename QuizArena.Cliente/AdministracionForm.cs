using System.Net.Http.Json;
using QuizArena.Cliente.Models;
using QuizArena.Cliente.Services;
namespace QuizArena.Cliente;
public partial class AdministracionForm : Form
{
    private int? categoriaId;
    private int? preguntaId;
    private bool cargando;
    public AdministracionForm() { InitializeComponent(); }
    public AdministracionForm(HttpClient cliente) : this()
    {
        api = cliente;
        cboDificultadPregunta.Items.AddRange(new object[] { "Facil", "Media", "Dificil" }); cboDificultadPregunta.SelectedIndex = 1;
        cboCorrecta.Items.AddRange(new object[] { "A", "B", "C", "D" }); cboCorrecta.SelectedIndex = 0;
        dgvPreguntas.AutoGenerateColumns = false;
        foreach (var campo in new[] { "Id", "Enunciado", "Activa" }) dgvPreguntas.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = campo, DataPropertyName = campo });
        Shown += async (_, _) => await EjecutarAsync(CargarAsync);
        btnActualizarCategorias.Click += async (_, _) => await EjecutarAsync(CargarAsync);
        btnActualizarPreguntas.Click += async (_, _) => await EjecutarAsync(CargarAsync);
        btnNuevaCategoria.Click += (_, _) => { categoriaId = null; dgvCategorias.ClearSelection(); txtNombreCategoria.Clear(); };
        btnNuevaPregunta.Click += (_, _) => NuevaPregunta();
        dgvCategorias.SelectionChanged += (_, _) => { if (!cargando && dgvCategorias.CurrentRow?.DataBoundItem is CategoriaAdminApi c) { categoriaId = c.Id; txtNombreCategoria.Text = c.Nombre; } };
        dgvPreguntas.SelectionChanged += (_, _) => { if (!cargando && dgvPreguntas.CurrentRow?.DataBoundItem is PreguntaAdminApi p) MostrarPregunta(p); };
        btnGuardarCategoria.Click += async (_, _) => await EjecutarAsync(async () => {
            using var respuesta = categoriaId is int id ? await api.PutAsJsonAsync($"api/categorias/{id}", new { nombre = txtNombreCategoria.Text }, cierre.Token) : await api.PostAsJsonAsync("api/categorias", new { nombre = txtNombreCategoria.Text }, cierre.Token);
            await RespuestaApi.ComprobarAsync(respuesta, cierre.Token); await CargarAsync(); lblResultado.Text = "Categoría guardada.";
        });
        btnEliminarCategoria.Click += async (_, _) => {
            if (categoriaId is not int id || operando || MessageBox.Show(this, "¿Eliminar la categoría seleccionada?", "Categorías", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            await EjecutarAsync(async () => { using var respuesta = await api.DeleteAsync($"api/categorias/{id}", cierre.Token); await RespuestaApi.ComprobarAsync(respuesta, cierre.Token); await CargarAsync(); lblResultado.Text = "Categoría eliminada."; });
        };
        btnGuardarPregunta.Click += async (_, _) => await EjecutarAsync(async () => {
            if (cboCategoriaPregunta.SelectedValue is not int cat) { lblResultado.Text = "Selecciona una categoría."; return; }
            var datos = new { categoriaId = cat, dificultad = cboDificultadPregunta.Text, enunciado = txtEnunciado.Text, opcionA = txtOpcionA.Text, opcionB = txtOpcionB.Text, opcionC = txtOpcionC.Text, opcionD = txtOpcionD.Text, correcta = cboCorrecta.Text, fuente = txtFuente.Text, licencia = txtLicencia.Text };
            using var respuesta = preguntaId is int id ? await api.PutAsJsonAsync($"api/preguntas/{id}", datos, cierre.Token) : await api.PostAsJsonAsync("api/preguntas", datos, cierre.Token);
            await RespuestaApi.ComprobarAsync(respuesta, cierre.Token); await CargarAsync(); lblResultado.Text = "Pregunta guardada.";
        });
        btnDesactivarPregunta.Click += async (_, _) => { if (operando || preguntaId is not int id || MessageBox.Show(this, "¿Desactivar la pregunta seleccionada? Se conservará su historial.", "Preguntas", MessageBoxButtons.YesNo) != DialogResult.Yes) return; await EjecutarAsync(() => CambiarActivaAsync(id, false)); };
        btnActivarPregunta.Click += async (_, _) => { if (preguntaId is int id) await EjecutarAsync(() => CambiarActivaAsync(id, true)); };
    }
    private async Task CambiarActivaAsync(int id, bool activa)
    {
        using var respuesta = activa ? await api.PutAsync($"api/preguntas/{id}/activar", null, cierre.Token) : await api.DeleteAsync($"api/preguntas/{id}", cierre.Token);
        await RespuestaApi.ComprobarAsync(respuesta, cierre.Token); await CargarAsync(); lblResultado.Text = activa ? "Pregunta reactivada." : "Pregunta desactivada.";
    }
    private async Task CargarAsync()
    {
        var categorias = await api.GetFromJsonAsync<List<CategoriaAdminApi>>("api/categorias", cierre.Token);
        var preguntas = new List<PreguntaAdminApi>();
        for (int pagina = 1; ; pagina++) {
            using var respuesta = await api.GetAsync($"api/preguntas/admin?pagina={pagina}&tamano=100", cierre.Token);
            await RespuestaApi.ComprobarAsync(respuesta, cierre.Token);
            var lote = await respuesta.Content.ReadFromJsonAsync<List<PreguntaAdminApi>>(cancellationToken: cierre.Token) ?? new(); preguntas.AddRange(lote); if (lote.Count < 100) break;
        }
        if (IsDisposed || Disposing) return;
        cargando = true;
        try { dgvCategorias.DataSource = categorias; cboCategoriaPregunta.DataSource = categorias?.ToList(); cboCategoriaPregunta.DisplayMember = "Nombre"; cboCategoriaPregunta.ValueMember = "Id"; dgvPreguntas.DataSource = preguntas; dgvCategorias.ClearSelection(); dgvPreguntas.ClearSelection(); categoriaId = null; txtNombreCategoria.Clear(); NuevaPregunta(); }
        finally { cargando = false; }
    }
    private void NuevaPregunta() { preguntaId = null; dgvPreguntas.ClearSelection(); foreach (var t in new[] { txtEnunciado, txtOpcionA, txtOpcionB, txtOpcionC, txtOpcionD, txtFuente, txtLicencia }) t.Clear(); txtFuente.Text = "Quiz Arena"; txtLicencia.Text = "Propias"; }
    private void MostrarPregunta(PreguntaAdminApi p) { preguntaId = p.Id; cboCategoriaPregunta.SelectedValue = p.CategoriaId; cboDificultadPregunta.SelectedIndex = Math.Clamp(p.Dificultad - 1, 0, 2); cboCorrecta.Text = p.Correcta; txtEnunciado.Text = p.Enunciado; txtOpcionA.Text = p.OpcionA; txtOpcionB.Text = p.OpcionB; txtOpcionC.Text = p.OpcionC; txtOpcionD.Text = p.OpcionD; txtFuente.Text = p.Fuente; txtLicencia.Text = p.Licencia; }
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

using System.Net.Http.Json;
using QuizArena.Cliente.Models;

namespace QuizArena.Cliente;

public partial class ClasificacionForm : Form
{
    private readonly HttpClient api = null!;
    private bool cargando;

    public ClasificacionForm()
    {
        InitializeComponent();
        TemaTrivial.Aplicar(this);
    }

    public ClasificacionForm(HttpClient cliente) : this()
    {
        api = cliente;

        lvIndividual.Columns.Add("Jugador", 220);
        lvIndividual.Columns.Add("Partidas", 100);
        lvIndividual.Columns.Add("Puntos", 120);
        lvIndividual.Columns.Add("Aciertos", 120);

        lvEquipos.Columns.Add("Equipo", 170);
        lvEquipos.Columns.Add("Sala", 140);
        lvEquipos.Columns.Add("Partida", 90);
        lvEquipos.Columns.Add("Puntos", 100);
        lvEquipos.Columns.Add("Aciertos", 100);

        btnActualizar.Click += async (_, _) =>
            await ActualizarAsync();

        Shown += async (_, _) =>
            await ActualizarAsync();
    }

    private async Task ActualizarAsync()
    {
        if (cargando || IsDisposed || Disposing)
            return;

        cargando = true;
        btnActualizar.Enabled = false;

        try
        {
            var individual = await api.GetFromJsonAsync<
                List<ClasificacionIndividualApi>>(
                "api/clasificacion/individual");

            var equipos = await api.GetFromJsonAsync<
                List<ClasificacionEquipoApi>>(
                "api/clasificacion/equipos");

            if (IsDisposed || Disposing)
                return;

            lvIndividual.Items.Clear();

            foreach (var jugador in individual ?? new())
            {
                lvIndividual.Items.Add(new ListViewItem(
                    new[]
                    {
                        jugador.Nombre,
                        jugador.Partidas.ToString(),
                        jugador.Puntos.ToString(),
                        jugador.Aciertos.ToString()
                    }));
            }

            lvEquipos.Items.Clear();

            foreach (var equipo in equipos ?? new())
            {
                lvEquipos.Items.Add(new ListViewItem(
                    new[]
                    {
                        equipo.Nombre,
                        equipo.CodigoSala,
                        equipo.PartidaId.ToString(),
                        equipo.Puntos.ToString(),
                        equipo.Aciertos.ToString()
                    }));
            }
        }
        catch (Exception ex)
        {
            if (!IsDisposed && !Disposing)
                MessageBox.Show(
                    this, ex.Message,
                    "Error al consultar clasificaciones");
        }
        finally
        {
            cargando = false;

            if (!IsDisposed && !Disposing)
                btnActualizar.Enabled = true;
        }
    }
}

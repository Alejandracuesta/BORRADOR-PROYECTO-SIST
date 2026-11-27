using QuizArena.Cliente.Models;

namespace QuizArena.Cliente;

// Podio que aparece al terminar la partida (fase "Finalizada").
// Los controles se crean por código para no tocar PartidaForm.Designer.cs.
public partial class PartidaForm
{
    private static readonly Color ColorOro = Color.FromArgb(255, 200, 40);
    private static readonly Color ColorPlata = Color.FromArgb(190, 195, 205);
    private static readonly Color ColorBronce = Color.FromArgb(205, 127, 50);

    // Índice = posición en el marcador (0 = primero). El primero va en el
    // centro y es la columna más alta; el segundo a la izquierda; el tercero
    // a la derecha.
    private static readonly (int X, int Alto)[] ColumnasPodio =
    {
        (270, 150),
        (40, 110),
        (500, 80)
    };

    private const int AnchoColumnaPodio = 200;
    private const int AltoNombrePodio = 50;

    private Panel? pnlPodio;
    private Label? lblEnhorabuena;
    private readonly Label[] lblNombresPodio = new Label[3];
    private readonly Label[] lblBloquesPodio = new Label[3];
    private bool enhorabuenaMostrada;

    private void CrearPodio()
    {
        pnlPodio = new Panel
        {
            Name = "pnlPodio",
            Location = new Point(25, 60),
            Size = new Size(740, 295),
            Visible = false
        };

        lblEnhorabuena = new Label
        {
            Name = "lblEnhorabuena",
            AutoSize = false,
            Location = new Point(0, 0),
            Size = new Size(740, 60),
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = TemaTrivial.Amarillo,
            TextAlign = ContentAlignment.MiddleCenter
        };
        pnlPodio.Controls.Add(lblEnhorabuena);

        for (int i = 0; i < ColumnasPodio.Length; i++)
        {
            var (x, alto) = ColumnasPodio[i];
            int yBloque = pnlPodio.Height - alto;

            lblBloquesPodio[i] = new Label
            {
                AutoSize = false,
                Location = new Point(x, yBloque),
                Size = new Size(AnchoColumnaPodio, alto),
                Font = new Font("Segoe UI", 28F, FontStyle.Bold),
                ForeColor = TemaTrivial.TextoOscuro,
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblNombresPodio[i] = new Label
            {
                AutoSize = false,
                AutoEllipsis = true,
                Location = new Point(x, yBloque - AltoNombrePodio),
                Size = new Size(AnchoColumnaPodio, AltoNombrePodio),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                TextAlign = ContentAlignment.BottomCenter
            };

            pnlPodio.Controls.Add(lblBloquesPodio[i]);
            pnlPodio.Controls.Add(lblNombresPodio[i]);
        }

        Controls.Add(pnlPodio);
        pnlPodio.BringToFront();
    }

    private void MostrarPodio(EstadoPartidaApi estado)
    {
        if (pnlPodio is null || lblEnhorabuena is null) return;

        bool finalizada = estado.Fase == "Finalizada";

        // El podio ocupa la zona de la pregunta y las opciones.
        pnlPodio.Visible = finalizada;
        lblPregunta.Visible = !finalizada;
        lblInfo.Visible = !finalizada;
        btnA.Visible = btnB.Visible = btnC.Visible = btnD.Visible = !finalizada;

        if (!finalizada) return;

        var marcador = estado.Marcador;
        int[] posiciones = CalcularPosiciones(marcador);

        for (int i = 0; i < ColumnasPodio.Length; i++)
        {
            bool hayJugador = i < marcador.Count;
            lblNombresPodio[i].Visible = hayJugador;
            lblBloquesPodio[i].Visible = hayJugador;
            if (!hayJugador) continue;

            var jugador = marcador[i];
            int posicion = posiciones[i];
            string tu = jugador.JugadorId == jugadorId ? " (tú)" : "";

            lblNombresPodio[i].Text =
                $"{jugador.Nombre}{tu}{Environment.NewLine}{jugador.Puntos} puntos";
            lblBloquesPodio[i].Text = $"{posicion}º";
            lblBloquesPodio[i].BackColor = posicion switch
            {
                1 => ColorOro,
                2 => ColorPlata,
                _ => ColorBronce
            };
        }

        // Ganadores: todos los empatados en el primer puesto con más de 0 puntos.
        var ganadores = marcador
            .Where((j, i) => posiciones[i] == 1 && j.Puntos > 0)
            .ToList();

        var yo = ganadores.FirstOrDefault(g => g.JugadorId == jugadorId);

        if (ganadores.Count == 0)
        {
            lblEnhorabuena.Text = "Partida terminada: nadie ha sumado puntos";
        }
        else if (yo is not null)
        {
            lblEnhorabuena.Text = ganadores.Count == 1
                ? "¡ENHORABUENA! Has quedado en primer puesto"
                : "¡ENHORABUENA! Has empatado en el primer puesto";
        }
        else
        {
            lblEnhorabuena.Text = ganadores.Count == 1
                ? $"¡Enhorabuena, {ganadores[0].Nombre}! Primer puesto"
                : $"¡Enhorabuena, {UnirNombres(ganadores.Select(g => g.Nombre))}! Empate en el primer puesto";
        }

        // Mensaje emergente solo para quien ha quedado primero, una vez.
        if (yo is not null && !enhorabuenaMostrada)
        {
            enhorabuenaMostrada = true;
            string texto = ganadores.Count == 1
                ? $"¡Enhorabuena, {yo.Nombre}!{Environment.NewLine}{Environment.NewLine}" +
                  $"Has quedado en primer puesto con {yo.Puntos} puntos."
                : $"¡Enhorabuena, {yo.Nombre}!{Environment.NewLine}{Environment.NewLine}" +
                  $"Has empatado en el primer puesto con {yo.Puntos} puntos.";

            // Se muestra después de terminar la actualización en curso.
            BeginInvoke(new Action(() =>
            {
                if (PuedeActualizar)
                    MessageBox.Show(this, texto, "¡Primer puesto!",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
            }));
        }
    }

    // Misma regla que la clasificación: los empatados a puntos comparten puesto.
    private static int[] CalcularPosiciones(IReadOnlyList<MarcadorJugadorApi> marcador)
    {
        var posiciones = new int[marcador.Count];
        int posicion = 0;
        int? puntosAnteriores = null;

        for (int i = 0; i < marcador.Count; i++)
        {
            if (puntosAnteriores != marcador[i].Puntos) posicion = i + 1;
            puntosAnteriores = marcador[i].Puntos;
            posiciones[i] = posicion;
        }

        return posiciones;
    }

    private static string UnirNombres(IEnumerable<string> nombres)
    {
        var lista = nombres.ToList();
        return lista.Count <= 1
            ? string.Join("", lista)
            : string.Join(", ", lista.Take(lista.Count - 1)) + " y " + lista[^1];
    }
}

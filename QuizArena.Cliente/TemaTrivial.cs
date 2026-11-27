using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace QuizArena.Cliente;

/// <summary>Cómo se marca una opción de respuesta en la pantalla de juego.</summary>
public enum MarcaOpcion
{
    Ninguna,
    Elegida,
    Correcta,
    Fallada,
    Atenuada
}

/// <summary>
/// Aspecto visual de juego de trivial para todos los formularios.
/// Solo cambia colores, letras y el dibujo de los botones: no toca eventos,
/// posiciones, textos ni lógica de los formularios.
/// </summary>
public static class TemaTrivial
{
    // Fondo y textos
    public static readonly Color Fondo = Color.FromArgb(27, 31, 59);
    public static readonly Color FondoTarjeta = Color.FromArgb(39, 44, 82);
    public static readonly Color FondoCampo = Color.FromArgb(50, 56, 102);
    public static readonly Color Texto = Color.FromArgb(240, 242, 255);
    public static readonly Color TextoSuave = Color.FromArgb(170, 176, 214);
    public static readonly Color TextoOscuro = Color.FromArgb(27, 31, 59);

    // Colores de los «quesitos»
    public static readonly Color Azul = Color.FromArgb(46, 134, 222);
    public static readonly Color Rosa = Color.FromArgb(232, 67, 147);
    public static readonly Color Amarillo = Color.FromArgb(247, 183, 49);
    public static readonly Color Marron = Color.FromArgb(165, 105, 63);
    public static readonly Color Verde = Color.FromArgb(32, 191, 107);
    public static readonly Color Naranja = Color.FromArgb(250, 130, 49);
    public static readonly Color Morado = Color.FromArgb(136, 84, 208);
    public static readonly Color Rojo = Color.FromArgb(235, 59, 90);

    public static readonly Color[] ColoresQuesito =
        { Azul, Rosa, Amarillo, Marron, Verde, Naranja };

    // Color de cada botón según su nombre (Name). El resto usa morado.
    private static readonly Dictionary<string, Color> ColoresBotones =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Respuestas
            ["btnA"] = Azul,
            ["btnB"] = Rosa,
            ["btnC"] = Amarillo,
            ["btnD"] = Verde,

            // Acceso
            ["btnEntrar"] = Verde,
            ["btnRegistrar"] = Azul,
            ["btnSalas"] = Naranja,
            ["btnPerfil"] = Morado,
            ["btnSalir"] = Rojo,
            ["btnComprobar"] = Morado,

            // Salas
            ["btnCrearSala"] = Verde,
            ["btnGuardarConfig"] = Morado,
            ["btnActualizarSalas"] = Morado,
            ["btnUnirsePublica"] = Azul,
            ["btnUnirseCodigo"] = Verde,
            ["btnCopiarCodigo"] = Amarillo,
            ["btnPreparado"] = Azul,
            ["btnSalirSala"] = Rojo,
            ["btnIniciarPartida"] = Verde,
            ["btnVerPartida"] = Naranja,
            ["btnChatSala"] = Rosa,
            ["btnRevancha"] = Naranja,
            ["btnClasificacion"] = Amarillo,

            // Partida
            ["btnConfirmarEquipo"] = Verde,
            ["btnCerrarPartida"] = Morado,
            ["btnChat"] = Rosa,

            // Chat y clasificación
            ["btnEnviar"] = Verde,
            ["btnAplauso"] = Amarillo,
            ["btnAnimo"] = Rosa,
            ["btnSorpresa"] = Azul,
            ["btnActualizar"] = Morado
        };

    // Etiquetas que hacen de título (amarillo) o de estado (gris suave).
    private static readonly HashSet<string> EtiquetasTitulo =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "lblTitulo", "lblFase", "lblSala", "lblTituloIndividual",
            "lblTituloEquipos", "lblAmbitoChat", "lblAviso"
        };

    private static readonly HashSet<string> EtiquetasSuaves =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "lblConexion", "lblEstado", "lblCodigoAyuda"
        };

    private sealed class EstadoBoton
    {
        public Color Color;
        public bool Encima;
        public bool Pulsado;
        public MarcaOpcion Marca;
    }

    private static readonly ConditionalWeakTable<Button, EstadoBoton> estados = new();

    /// <summary>Aplica el tema a un formulario y a todos sus controles.</summary>
    public static void Aplicar(Form formulario)
    {
        formulario.BackColor = Fondo;
        formulario.ForeColor = Texto;
        AplicarControles(formulario.Controls);
        ActivarBarraOscura(formulario);
    }

    /// <summary>Marca una opción de respuesta (elegida, correcta, fallada...).</summary>
    public static void MarcarOpcion(Button boton, MarcaOpcion marca)
    {
        if (!estados.TryGetValue(boton, out var estado) || estado.Marca == marca)
            return;

        estado.Marca = marca;
        boton.Invalidate();
    }

    private static void AplicarControles(Control.ControlCollection controles)
    {
        foreach (Control control in controles)
        {
            switch (control)
            {
                case Button boton:
                    EstilizarBoton(boton);
                    break;

                case TextBox caja:
                    caja.BackColor = FondoCampo;
                    caja.ForeColor = Texto;
                    caja.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case ComboBox combo:
                    combo.FlatStyle = FlatStyle.Flat;
                    combo.BackColor = FondoCampo;
                    combo.ForeColor = Texto;
                    break;

                case NumericUpDown numero:
                    numero.BackColor = FondoCampo;
                    numero.ForeColor = Texto;
                    numero.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case ListBox lista:
                    lista.BackColor = FondoTarjeta;
                    lista.ForeColor = Texto;
                    lista.BorderStyle = BorderStyle.None;
                    break;

                case ListView vista:
                    vista.BackColor = FondoTarjeta;
                    vista.ForeColor = Texto;
                    vista.BorderStyle = BorderStyle.None;
                    break;

                case GroupBox grupo:
                    // El título del grupo en amarillo; su contenido sigue en blanco.
                    grupo.ForeColor = Amarillo;
                    break;

                case CheckBox casilla:
                    casilla.ForeColor = Texto;
                    break;

                case Label etiqueta:
                    if (EtiquetasTitulo.Contains(etiqueta.Name))
                        etiqueta.ForeColor = Amarillo;
                    else if (EtiquetasSuaves.Contains(etiqueta.Name))
                        etiqueta.ForeColor = TextoSuave;
                    else if (etiqueta.Parent is GroupBox)
                        etiqueta.ForeColor = Texto;
                    break;
            }

            if (control.HasChildren)
                AplicarControles(control.Controls);
        }
    }

    private static void EstilizarBoton(Button boton)
    {
        if (estados.TryGetValue(boton, out _))
            return;

        var estado = new EstadoBoton
        {
            Color = ColoresBotones.TryGetValue(boton.Name, out var color) ? color : Morado
        };
        estados.Add(boton, estado);

        boton.FlatStyle = FlatStyle.Flat;
        boton.FlatAppearance.BorderSize = 0;
        boton.UseVisualStyleBackColor = false;
        boton.BackColor = estado.Color;
        boton.ForeColor = Color.White;
        boton.Font = new Font(boton.Font, FontStyle.Bold);
        boton.Cursor = Cursors.Hand;

        boton.MouseEnter += (_, _) => { estado.Encima = true; boton.Invalidate(); };
        boton.MouseLeave += (_, _) =>
        {
            estado.Encima = false;
            estado.Pulsado = false;
            boton.Invalidate();
        };
        boton.MouseDown += (_, e) =>
        {
            if (e.Button != MouseButtons.Left) return;
            estado.Pulsado = true;
            boton.Invalidate();
        };
        boton.MouseUp += (_, _) => { estado.Pulsado = false; boton.Invalidate(); };
        boton.EnabledChanged += (_, _) => boton.Invalidate();
        boton.GotFocus += (_, _) => boton.Invalidate();
        boton.LostFocus += (_, _) => boton.Invalidate();
        boton.Paint += (_, e) => DibujarBoton(boton, estado, e.Graphics);
    }

    // Dibuja el botón entero encima del dibujo normal de Windows.
    private static void DibujarBoton(Button boton, EstadoBoton estado, Graphics g)
    {
        Color fondoPadre = ColorDeFondo(boton.Parent);
        g.Clear(fondoPadre);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        bool apagado = estado.Marca is MarcaOpcion.Atenuada or MarcaOpcion.Fallada ||
                       (!boton.Enabled && estado.Marca == MarcaOpcion.Ninguna);

        Color color = estado.Color;
        if (apagado)
            color = Mezclar(color, fondoPadre, 0.6);
        else if (estado.Pulsado)
            color = Mezclar(color, Color.Black, 0.2);
        else if (estado.Encima && boton.Enabled)
            color = Mezclar(color, Color.White, 0.15);

        var rect = new Rectangle(1, 1, boton.Width - 3, boton.Height - 3);
        if (rect.Width <= 0 || rect.Height <= 0)
            return;

        using (var ruta = Redondeado(rect, Math.Min(12, rect.Height / 2)))
        {
            using (var pincel = new SolidBrush(color))
                g.FillPath(pincel, ruta);

            Color? colorBorde = estado.Marca switch
            {
                MarcaOpcion.Elegida => (Color?)Color.White,
                MarcaOpcion.Correcta => (Color?)Color.White,
                MarcaOpcion.Fallada => (Color?)Rojo,
                _ => boton.Focused
                    ? (Color?)Mezclar(Color.White, color, 0.4)
                    : (Color?)null
            };

            if (colorBorde is { } borde)
            {
                float grosor = estado.Marca == MarcaOpcion.Ninguna ? 2f : 4f;
                using var lapiz = new Pen(borde, grosor);
                g.DrawPath(lapiz, ruta);
            }
        }

        Color colorTexto = EsClaro(estado.Color) ? TextoOscuro : Color.White;
        if (apagado)
            colorTexto = Mezclar(colorTexto, fondoPadre, 0.4);

        string texto = estado.Marca switch
        {
            MarcaOpcion.Correcta => "✓  " + boton.Text,
            MarcaOpcion.Fallada => "✗  " + boton.Text,
            _ => boton.Text
        };

        var area = Rectangle.Inflate(rect, -10, -4);
        TextRenderer.DrawText(g, texto, boton.Font, area, colorTexto,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
            TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
    }

    private static Color ColorDeFondo(Control? control)
    {
        while (control != null && control.BackColor.A < 255)
            control = control.Parent;
        return control?.BackColor ?? Fondo;
    }

    private static GraphicsPath Redondeado(Rectangle r, int radio)
    {
        int d = Math.Max(2, radio * 2);
        var ruta = new GraphicsPath();
        ruta.AddArc(r.X, r.Y, d, d, 180, 90);
        ruta.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        ruta.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        ruta.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        ruta.CloseFigure();
        return ruta;
    }

    // Mezcla «a» con «b»: cantidad 0 = solo a, 1 = solo b.
    public static Color Mezclar(Color a, Color b, double cantidad)
    {
        int Canal(int x, int y) => (int)Math.Round(x + (y - x) * cantidad);
        return Color.FromArgb(Canal(a.R, b.R), Canal(a.G, b.G), Canal(a.B, b.B));
    }

    private static bool EsClaro(Color c) =>
        (c.R * 299 + c.G * 587 + c.B * 114) / 1000 > 165;

    // Barra de título oscura en Windows 10/11 (si no es posible, no pasa nada).
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int atributo, ref int valor, int tamano);

    private static void ActivarBarraOscura(Form formulario)
    {
        void Activar()
        {
            try
            {
                int activado = 1;
                if (DwmSetWindowAttribute(formulario.Handle, 20, ref activado, sizeof(int)) != 0)
                    DwmSetWindowAttribute(formulario.Handle, 19, ref activado, sizeof(int));
            }
            catch (Exception)
            {
                // Versiones de Windows sin esta opción: se deja la barra normal.
            }
        }

        if (formulario.IsHandleCreated)
            Activar();
        else
            formulario.HandleCreated += (_, _) => Activar();
    }
}

/// <summary>Rueda decorativa de seis colores, como un tablero de trivial.</summary>
public sealed class RuedaTrivial : Control
{
    public RuedaTrivial()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint |
                 ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.ResizeRedraw, true);
        BackColor = Color.Transparent;
        TabStop = false;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        int lado = Math.Min(Width, Height) - 6;
        if (lado <= 10) return;

        var circulo = new Rectangle((Width - lado) / 2, (Height - lado) / 2, lado, lado);
        float cx = circulo.X + lado / 2f;
        float cy = circulo.Y + lado / 2f;

        float angulo = -90;
        foreach (var color in TemaTrivial.ColoresQuesito)
        {
            using var pincel = new SolidBrush(color);
            g.FillPie(pincel, circulo, angulo, 60);
            angulo += 60;
        }

        using (var separador = new Pen(TemaTrivial.Fondo, 4))
        {
            for (int i = 0; i < 6; i++)
            {
                double rad = (-90 + i * 60) * Math.PI / 180;
                g.DrawLine(separador, cx, cy,
                    cx + (float)Math.Cos(rad) * lado / 2f,
                    cy + (float)Math.Sin(rad) * lado / 2f);
            }
        }

        using (var borde = new Pen(Color.White, 3))
            g.DrawEllipse(borde, circulo);

        int centro = lado * 2 / 5;
        var rectCentro = new Rectangle((int)(cx - centro / 2f), (int)(cy - centro / 2f), centro, centro);
        using (var pincelCentro = new SolidBrush(TemaTrivial.Fondo))
            g.FillEllipse(pincelCentro, rectCentro);
        using (var bordeCentro = new Pen(Color.White, 3))
            g.DrawEllipse(bordeCentro, rectCentro);

        using var fuente = new Font("Segoe UI", Math.Max(8f, centro / 1.6f), FontStyle.Bold, GraphicsUnit.Pixel);
        TextRenderer.DrawText(g, "?", fuente, rectCentro, TemaTrivial.Amarillo,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }
}

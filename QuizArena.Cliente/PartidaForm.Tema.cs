using QuizArena.Cliente.Models;

namespace QuizArena.Cliente;

// Aspecto de trivial de la pantalla de juego. Solo cambia la apariencia.
public partial class PartidaForm
{
    private void AplicarTemaPartida()
    {
        TemaTrivial.Aplicar(this);

        // La pregunta, como una tarjeta blanca.
        lblPregunta.BackColor = Color.White;
        lblPregunta.ForeColor = TemaTrivial.TextoOscuro;
        lblPregunta.TextAlign = ContentAlignment.MiddleCenter;
        lblPregunta.Padding = new Padding(14, 6, 14, 6);

        // Reloj en una pastilla amarilla.
        lblTiempo.BackColor = TemaTrivial.Amarillo;
        lblTiempo.ForeColor = TemaTrivial.TextoOscuro;
        lblTiempo.TextAlign = ContentAlignment.MiddleCenter;

        // Opciones A/B/C/D más grandes.
        foreach (var boton in new[] { btnA, btnB, btnC, btnD })
            boton.Font = new Font("Segoe UI", 11F, FontStyle.Bold);

        // Marcador como panel oscuro.
        txtMarcador.BackColor = TemaTrivial.FondoTarjeta;
        txtMarcador.ForeColor = TemaTrivial.Texto;
    }

    // Marca la opción elegida durante la pregunta y, en resultados,
    // la correcta (✓) y la propia si fue incorrecta (✗).
    private void ResaltarOpciones(EstadoPartidaApi estado)
    {
        string? correcta = estado.Fase == "Resultados"
            ? estado.Resultado?.RespuestaCorrecta?.Trim()
            : null;
        string? mia = estado.MiRespuesta?.Trim();

        var botones = new (string Letra, Button Boton)[]
        {
            ("A", btnA), ("B", btnB), ("C", btnC), ("D", btnD)
        };

        foreach (var (letra, boton) in botones)
        {
            bool esMia = string.Equals(letra, mia, StringComparison.OrdinalIgnoreCase);
            MarcaOpcion marca = MarcaOpcion.Ninguna;

            if (estado.Fase == "Resultados" && !string.IsNullOrEmpty(correcta))
            {
                bool esCorrecta = string.Equals(letra, correcta, StringComparison.OrdinalIgnoreCase);
                marca = esCorrecta ? MarcaOpcion.Correcta
                      : esMia ? MarcaOpcion.Fallada
                      : MarcaOpcion.Atenuada;
            }
            else if (estado.Fase == "Pregunta" && esMia)
            {
                marca = MarcaOpcion.Elegida;
            }

            TemaTrivial.MarcarOpcion(boton, marca);
        }
    }
}

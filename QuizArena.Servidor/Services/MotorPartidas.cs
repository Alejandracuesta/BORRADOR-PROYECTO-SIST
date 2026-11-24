using Microsoft.EntityFrameworkCore;
using QuizArena.Servidor.Data;
using QuizArena.Servidor.Models;

namespace QuizArena.Servidor.Services;

public static class MotorPartidas
{
    public const int SegundosCuentaAtras = 3;
    public const int SegundosResultados = 5;

    public static int TiempoPregunta(DificultadPregunta dificultad)
    {
        return dificultad == DificultadPregunta.Dificil ? 30 : 20;
    }

    public static int TiempoPregunta(
        DificultadPregunta dificultad,
        ModalidadSala modalidad)
    {
        if (modalidad == ModalidadSala.Individual)
            return TiempoPregunta(dificultad);

        return dificultad switch
        {
            DificultadPregunta.Facil => 30,
            DificultadPregunta.Media => 45,
            DificultadPregunta.Dificil => 60,
            _ => 30
        };
    }

    public static int PuntosBase(DificultadPregunta dificultad)
    {
        return dificultad switch
        {
            DificultadPregunta.Facil => 100,
            DificultadPregunta.Media => 200,
            DificultadPregunta.Dificil => 300,
            _ => 0
        };
    }

    // Porcentaje de los puntos base que se conserva al responder en el último
    // instante del plazo. Responder al abrir la pregunta da el 100 %.
    public const double FactorMinimoVelocidad = 0.5;

    // Puntos de un acierto según la rapidez: decrecen de forma lineal desde
    // el 100 % de los puntos base (respuesta inmediata) hasta el 50 %
    // (respuesta al agotarse el plazo). No incluye la bonificación de racha.
    public static int PuntosPorVelocidad(
        DificultadPregunta dificultad, TimeSpan tiempoRespuesta)
    {
        int puntosBase = PuntosBase(dificultad);
        double plazo = TiempoPregunta(dificultad);

        double segundos = Math.Clamp(
            tiempoRespuesta.TotalSeconds, 0, plazo);

        double factor = 1.0 -
            (1.0 - FactorMinimoVelocidad) * (segundos / plazo);

        return (int)Math.Round(
            puntosBase * factor, MidpointRounding.AwayFromZero);
    }

    public static int BonificacionRacha(int racha)
    {
        if (racha >= 5)
            return 100;

        if (racha >= 3)
            return 50;

        return 0;
    }

    // Abre una pregunta usando una fecha oficial del servidor.
    private static void AbrirPregunta(
        Partida partida, DateTime fechaApertura)
    {
        var actual = partida.Preguntas.Single(
            p => p.Orden == partida.IndicePregunta);

        actual.AperturaUtc = fechaApertura;

        partida.Fase = FasePartida.Pregunta;

        partida.FinFaseUtc = fechaApertura.AddSeconds(
            TiempoPregunta(actual.Pregunta.Dificultad, partida.Sala.Modalidad));
    }

    // Comprueba y ejecuta las transiciones pendientes.
    public static async Task AvanzarAsync(
        QuizArenaDbContext db,
        Partida partida,
        CancellationToken ct = default)
    {
        bool hayCambios = false;

        // Permite recuperar todas las transiciones pendientes
        // si durante un tiempo ningún cliente consulta el servidor.
        int limiteIteraciones = partida.Preguntas.Count * 3 + 5;

        for (int i = 0; i < limiteIteraciones; i++)
        {
            DateTime ahora = DateTime.UtcNow;

            if (partida.Fase == FasePartida.CuentaAtras)
            {
                if (ahora < partida.FinFaseUtc)
                    break;

                DateTime apertura = partida.FinFaseUtc;

                AbrirPregunta(partida, apertura);
                hayCambios = true;
                continue;
            }

            if (partida.Fase == FasePartida.Pregunta)
            {
                var actual = partida.Preguntas.Single(
                    p => p.Orden == partida.IndicePregunta);

                bool esEquipos =
                    partida.Sala.Modalidad == ModalidadSala.Equipos;

                bool todosRespondieron = esEquipos
                    ? actual.RespuestasEquipo.Count == partida.Equipos.Count
                    : actual.Respuestas.Count == partida.Jugadores.Count;

                bool tiempoAgotado =
                    ahora >= partida.FinFaseUtc;

                if (!todosRespondieron && !tiempoAgotado)
                    break;

                DateTime cierre = tiempoAgotado
                    ? partida.FinFaseUtc
                    : ahora;

                if (esEquipos)
                {
                    if (tiempoAgotado)
                    {
                        // Confirmar automáticamente equipos unánimes.
                        foreach (var equipo in partida.Equipos)
                        {
                            bool respondio = actual.RespuestasEquipo.Any(
                                r => r.EquipoPartidaId == equipo.Id);

                            if (respondio)
                                continue;

                            string? consenso = MotorEquipos.ObtenerConsenso(
                                actual, equipo, partida.FinFaseUtc);

                            if (consenso != null)
                            {
                                MotorEquipos.Confirmar(
                                    actual, equipo, consenso, cierre);
                            }
                        }
                    }

                    // La ausencia de respuesta rompe la racha del equipo.
                    foreach (var equipo in partida.Equipos)
                    {
                        bool respondio = actual.RespuestasEquipo.Any(
                            r => r.EquipoPartidaId == equipo.Id);

                        if (!respondio)
                            equipo.Racha = 0;
                    }
                }
                else
                {
                    // Comportamiento anterior del modo individual.
                    var respondieron = actual.Respuestas
                        .Select(r => r.JugadorId)
                        .ToHashSet();

                    foreach (var jugador in partida.Jugadores)
                    {
                        if (!respondieron.Contains(jugador.JugadorId))
                            jugador.Racha = 0;
                    }
                }

                actual.CierreUtc = cierre;

                partida.Fase = FasePartida.Resultados;
                partida.FinFaseUtc =
                    cierre.AddSeconds(SegundosResultados);

                hayCambios = true;
                continue;
            }

            if (partida.Fase == FasePartida.Resultados)
            {
                if (ahora < partida.FinFaseUtc)
                    break;

                if (partida.IndicePregunta + 1 >=
                    partida.Preguntas.Count)
                {
                    partida.Fase = FasePartida.Finalizada;
                    partida.Sala.Estado = EstadoSala.Finalizada;
                }
                else
                {
                    DateTime apertura = partida.FinFaseUtc;

                    partida.IndicePregunta++;
                    AbrirPregunta(partida, apertura);
                }

                hayCambios = true;
                continue;
            }

            // La partida finalizada ya no avanza.
            break;
        }

        if (hayCambios)
            await db.SaveChangesAsync(ct);
    }
}

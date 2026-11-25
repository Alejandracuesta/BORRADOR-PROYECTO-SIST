using QuizArena.Servidor.Models;

namespace QuizArena.Servidor.Services;

public static class MotorEquipos
{
    // Devuelve la opción acordada, o null si no hay unanimidad.
    public static string? ObtenerConsenso(
        PartidaPregunta ronda,
        EquipoPartida equipo,
        DateTime? limiteUtc = null)
    {
        var miembros = equipo.Miembros
            .Select(m => m.JugadorId)
            .ToHashSet();

        if (miembros.Count is < 2 or > 3)
            return null;

        var votos = ronda.VotosEquipo
            .Where(v => miembros.Contains(v.JugadorId) &&
                        (!limiteUtc.HasValue ||
                         v.FechaUtc < limiteUtc.Value))
            .ToList();

        if (votos.Count != miembros.Count)
            return null;

        string opcion = votos[0].Opcion;

        return votos.All(v => v.Opcion == opcion)
            ? opcion
            : null;
    }

    // Registra una sola decisión definitiva para el equipo.
    public static void Confirmar(
        PartidaPregunta ronda,
        EquipoPartida equipo,
        string opcion,
        DateTime fechaUtc)
    {
        if (ronda.RespuestasEquipo.Any(
            r => r.EquipoPartidaId == equipo.Id))
        {
            return;
        }

        bool correcta = string.Equals(
            ronda.Pregunta.RespuestaCorrecta,
            opcion,
            StringComparison.OrdinalIgnoreCase);

        int puntos = 0;

        if (correcta)
        {
            equipo.Racha++;
            equipo.Aciertos++;

            puntos = MotorPartidas.PuntosBase(
                ronda.Pregunta.Dificultad);

            puntos += MotorPartidas.BonificacionRacha(
                equipo.Racha);

            equipo.Puntos += puntos;
        }
        else
        {
            equipo.Racha = 0;
        }

        ronda.RespuestasEquipo.Add(new RespuestaEquipo
        {
            PartidaPregunta = ronda,
            Equipo = equipo,
            EquipoPartidaId = equipo.Id,
            Opcion = opcion,
            EsCorrecta = correcta,
            PuntosGanados = puntos,
            ConfirmadaUtc = fechaUtc
        });
    }
}

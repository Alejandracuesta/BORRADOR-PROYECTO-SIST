namespace QuizArena.Cliente.Models;

public record PreguntaJuegoApi(
    int Id,
    string Enunciado,
    string Categoria,
    string Dificultad,
    string OpcionA,
    string OpcionB,
    string OpcionC,
    string OpcionD
);

public record MarcadorJugadorApi(
    int JugadorId,
    string Nombre,
    int Puntos,
    int Aciertos,
    int Racha
);

public record RespuestaJugadorApi(
    int JugadorId,
    string Nombre,
    string? Opcion,
    bool EsCorrecta,
    int PuntosGanados
);

public record ResultadoRondaApi(
    string RespuestaCorrecta,
    List<RespuestaJugadorApi> Respuestas
);

public record EstadoPartidaApi(
    int PartidaId,
    string CodigoSala,
    string Fase,
    int NumeroPregunta,
    int TotalPreguntas,
    DateTime? FinFaseUtc,
    DateTime ServidorUtc,
    PreguntaJuegoApi? Pregunta,
    string? MiRespuesta,
    ResultadoRondaApi? Resultado,
    List<MarcadorJugadorApi> Marcador
);

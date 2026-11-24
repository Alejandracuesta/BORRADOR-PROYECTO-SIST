using System.ComponentModel.DataAnnotations;

namespace QuizArena.Servidor.DTOs;

public class ResponderPreguntaDto
{
    public int PreguntaId { get; set; }

    [Required]
    public string Opcion { get; set; } = "";
}

public record PreguntaJuegoDto(
    int Id,
    string Enunciado,
    string Categoria,
    string Dificultad,
    string OpcionA,
    string OpcionB,
    string OpcionC,
    string OpcionD
);

public record MarcadorJugadorDto(
    int JugadorId,
    string Nombre,
    int Puntos,
    int Aciertos,
    int Racha
);

public record RespuestaJugadorDto(
    int JugadorId,
    string Nombre,
    string? Opcion,
    bool EsCorrecta,
    int PuntosGanados
);

public record ResultadoRondaDto(
    string RespuestaCorrecta,
    List<RespuestaJugadorDto> Respuestas
);

public record EstadoPartidaDto(
    int PartidaId,
    string CodigoSala,
    string Fase,
    int NumeroPregunta,
    int TotalPreguntas,
    DateTime? FinFaseUtc,
    DateTime ServidorUtc,
    PreguntaJuegoDto? Pregunta,
    string? MiRespuesta,
    ResultadoRondaDto? Resultado,
    List<MarcadorJugadorDto> Marcador,
    EquipoActualDto? MiEquipo,
    List<MarcadorEquipoDto>? MarcadorEquipos,
    List<ResultadoEquipoDto>? ResultadosEquipos
);

public record VotoMiembroEquipoDto(
    int JugadorId,
    string Nombre,
    string? Opcion
);

public record EquipoActualDto(
    int Id,
    string Nombre,
    bool HayUnanimidad,
    bool Confirmada,
    List<VotoMiembroEquipoDto> Votos
);

public record MarcadorEquipoDto(
    int EquipoId,
    string Nombre,
    int Puntos,
    int Aciertos,
    int Racha
);

public record ResultadoEquipoDto(
    int EquipoId,
    string Nombre,
    string? Opcion,
    bool EsCorrecta,
    int PuntosGanados
);

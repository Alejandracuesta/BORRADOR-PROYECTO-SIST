using System.ComponentModel.DataAnnotations;

namespace QuizArena.Servidor.DTOs;

public class CrearSalaDto
{
    [Required]
    [StringLength(40, MinimumLength = 3)]
    public string Nombre { get; set; } = "";

    public bool EsPrivada { get; set; }

    public string Modalidad { get; set; } = "Individual";

    public int MaxJugadores { get; set; } = 4;

    public int NumeroPreguntas { get; set; } = 5;

    public int? CategoriaId { get; set; }

    // Null = dificultad mixta; 1, 2 y 3 corresponden
    // a Facil, Media y Dificil.
    public int? Dificultad { get; set; }
}

public class PreparadoSalaDto
{
    public bool Preparado { get; set; }
}

public record ParticipanteSalaDto(
    int JugadorId,
    string Nombre,
    bool Preparado,
    bool EsAnfitrion
);

public record SalaDetalleDto(
    string Codigo,
    string Nombre,
    bool EsPrivada,
    string Modalidad,
    string Estado,
    int MaxJugadores,
    int NumeroPreguntas,
    int? CategoriaId,
    string? Categoria,
    string? Dificultad,
    int CreadorId,
    List<ParticipanteSalaDto> Participantes
);

public record SalaPublicaDto(
    string Codigo,
    string Nombre,
    string Modalidad,
    int Ocupacion,
    int MaxJugadores,
    int NumeroPreguntas,
    string? Categoria,
    string? Dificultad
);

namespace QuizArena.Servidor.DTOs;

public record ClasificacionIndividualDto(
    int JugadorId,
    string Nombre,
    int Partidas,
    int Puntos,
    int Aciertos
);

public record ClasificacionEquipoDto(
    int PartidaId,
    string CodigoSala,
    string Nombre,
    int Puntos,
    int Aciertos,
    DateTime FechaUtc
);

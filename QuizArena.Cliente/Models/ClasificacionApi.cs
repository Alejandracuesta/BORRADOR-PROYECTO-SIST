namespace QuizArena.Cliente.Models;

public record ClasificacionIndividualApi(
    int JugadorId,
    string Nombre,
    int Partidas,
    int Puntos,
    int Aciertos
);

public record ClasificacionEquipoApi(
    int PartidaId,
    string CodigoSala,
    string Nombre,
    int Puntos,
    int Aciertos,
    DateTime FechaUtc
);

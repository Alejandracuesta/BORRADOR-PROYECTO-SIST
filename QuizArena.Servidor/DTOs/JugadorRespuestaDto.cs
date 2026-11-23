namespace QuizArena.Servidor.DTOs;

public record JugadorRespuestaDto(
    int Id,
    string Nombre,
    DateTime FechaRegistroUtc);

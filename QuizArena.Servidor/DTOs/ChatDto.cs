namespace QuizArena.Servidor.DTOs;

public record MensajeSalaDto(
    int Id,
    string CodigoSala,
    int JugadorId,
    string Nombre,
    string Texto,
    DateTime FechaUtc,
    int? EquipoPartidaId,
    bool Eliminado = false,
    DateTime? EditadoUtc = null
);

public record ReaccionDto(
    string CodigoSala,
    int JugadorId,
    string Nombre,
    string Tipo,
    DateTime FechaUtc
);

namespace QuizArena.Cliente.Models;

public record MensajeChatApi(
    int Id,
    string CodigoSala,
    int JugadorId,
    string Nombre,
    string Texto,
    DateTime FechaUtc,
    int? EquipoPartidaId
);

public record ReaccionApi(
    string CodigoSala,
    int JugadorId,
    string Nombre,
    string Tipo,
    DateTime FechaUtc
);

namespace QuizArena.Cliente.Models;

public record MensajeChatApi(
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

public record ReaccionApi(
    string CodigoSala,
    int JugadorId,
    string Nombre,
    string Tipo,
    DateTime FechaUtc
);

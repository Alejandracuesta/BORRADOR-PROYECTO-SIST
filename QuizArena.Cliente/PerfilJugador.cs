namespace QuizArena.Cliente;

// Únicamente los datos públicos del perfil que devuelve la API.
public record PerfilJugador(
    int Id,
    string Nombre,
    DateTime FechaRegistroUtc,
    bool EsAdministrador = false);

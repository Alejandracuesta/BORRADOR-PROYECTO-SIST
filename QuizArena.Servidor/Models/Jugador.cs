namespace QuizArena.Servidor.Models;

public class Jugador
{
    public int Id { get; set; }

    public string Nombre { get; set; } = "";

    public string NombreNormalizado { get; set; } = "";

    public DateTime FechaRegistroUtc { get; set; }

    // Los perfiles anteriores a la fase 3 permanecen sin credenciales.
    public string? PasswordHash { get; set; }
}

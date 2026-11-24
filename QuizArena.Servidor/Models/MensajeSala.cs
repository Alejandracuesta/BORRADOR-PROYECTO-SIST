namespace QuizArena.Servidor.Models;

public class MensajeSala
{
    public int Id { get; set; }

    public int SalaId { get; set; }
    public Sala Sala { get; set; } = null!;

    public int JugadorId { get; set; }
    public Jugador Jugador { get; set; } = null!;

    // Null significa mensaje general.
    public int? EquipoPartidaId { get; set; }
    public EquipoPartida? Equipo { get; set; }

    public string Texto { get; set; } = "";

    public DateTime FechaUtc { get; set; }
}

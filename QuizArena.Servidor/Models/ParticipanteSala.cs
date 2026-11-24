namespace QuizArena.Servidor.Models;

public class ParticipanteSala
{
    public int Id { get; set; }

    public int SalaId { get; set; }
    public Sala Sala { get; set; } = null!;

    public int JugadorId { get; set; }
    public Jugador Jugador { get; set; } = null!;

    public bool Preparado { get; set; }

    public DateTime UnidoUtc { get; set; }
}

namespace QuizArena.Servidor.Models;

public class VotoEquipo
{
    public int Id { get; set; }

    public int PartidaPreguntaId { get; set; }
    public PartidaPregunta PartidaPregunta { get; set; } = null!;

    public int JugadorId { get; set; }
    public Jugador Jugador { get; set; } = null!;

    public string Opcion { get; set; } = "";

    public DateTime FechaUtc { get; set; }
}

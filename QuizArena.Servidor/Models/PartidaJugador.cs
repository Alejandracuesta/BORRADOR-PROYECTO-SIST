namespace QuizArena.Servidor.Models;

public class PartidaJugador
{
    public int Id { get; set; }

    public int PartidaId { get; set; }
    public Partida Partida { get; set; } = null!;

    public int JugadorId { get; set; }
    public Jugador Jugador { get; set; } = null!;

    public int Puntos { get; set; }

    public int Racha { get; set; }

    public int Aciertos { get; set; }
}

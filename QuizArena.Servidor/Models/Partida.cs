namespace QuizArena.Servidor.Models;

public class Partida
{
    public int Id { get; set; }

    public int SalaId { get; set; }
    public Sala Sala { get; set; } = null!;

    public FasePartida Fase { get; set; }

    // Comienza en 0 para representar la primera pregunta.
    public int IndicePregunta { get; set; }

    public DateTime CreadaUtc { get; set; }

    // Momento en el que debe finalizar la fase actual.
    public DateTime FinFaseUtc { get; set; }

    public List<PartidaPregunta> Preguntas { get; set; } = new();

    public List<PartidaJugador> Jugadores { get; set; } = new();

    public List<EquipoPartida> Equipos { get; set; } = new();
}

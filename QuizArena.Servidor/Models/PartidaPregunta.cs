namespace QuizArena.Servidor.Models;

public class PartidaPregunta
{
    public int Id { get; set; }

    public int PartidaId { get; set; }
    public Partida Partida { get; set; } = null!;

    public int PreguntaId { get; set; }
    public Pregunta Pregunta { get; set; } = null!;

    // Posición de la pregunta dentro de la partida.
    public int Orden { get; set; }

    public DateTime? AperturaUtc { get; set; }
    public DateTime? CierreUtc { get; set; }

    public List<RespuestaPartida> Respuestas { get; set; } = new();
}

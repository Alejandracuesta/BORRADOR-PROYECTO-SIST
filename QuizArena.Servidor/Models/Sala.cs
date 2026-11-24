namespace QuizArena.Servidor.Models;

public class Sala
{
    public int Id { get; set; }

    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";

    public bool EsPrivada { get; set; }

    public ModalidadSala Modalidad { get; set; }
    public EstadoSala Estado { get; set; } = EstadoSala.EnEspera;

    public int MaxJugadores { get; set; }
    public int NumeroPreguntas { get; set; }

    public int? CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public DificultadPregunta? Dificultad { get; set; }

    public int CreadorId { get; set; }
    public Jugador Creador { get; set; } = null!;

    public DateTime CreadaUtc { get; set; }

    public List<ParticipanteSala> Participantes { get; set; } = new();
}

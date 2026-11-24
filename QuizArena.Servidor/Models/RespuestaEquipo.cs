namespace QuizArena.Servidor.Models;

public class RespuestaEquipo
{
    public int Id { get; set; }

    public int PartidaPreguntaId { get; set; }
    public PartidaPregunta PartidaPregunta { get; set; } = null!;

    public int EquipoPartidaId { get; set; }
    public EquipoPartida Equipo { get; set; } = null!;

    public string Opcion { get; set; } = "";

    public bool EsCorrecta { get; set; }
    public int PuntosGanados { get; set; }

    public DateTime ConfirmadaUtc { get; set; }
}

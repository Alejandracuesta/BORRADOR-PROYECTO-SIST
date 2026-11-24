namespace QuizArena.Servidor.Models;

public class EquipoPartida
{
    public int Id { get; set; }

    public int PartidaId { get; set; }
    public Partida Partida { get; set; } = null!;

    public string Nombre { get; set; } = "";

    public int Puntos { get; set; }
    public int Racha { get; set; }
    public int Aciertos { get; set; }

    public List<PartidaJugador> Miembros { get; set; } = new();
}

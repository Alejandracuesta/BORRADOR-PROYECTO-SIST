namespace QuizArena.Servidor.Models;

public class Categoria
{
    public int Id { get; set; }

    public string Nombre { get; set; } = "";

    public string NombreNormalizado { get; set; } = "";

    public List<Pregunta> Preguntas { get; set; } = new();
}

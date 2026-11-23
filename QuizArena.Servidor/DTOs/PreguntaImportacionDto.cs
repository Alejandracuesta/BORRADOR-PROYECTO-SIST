namespace QuizArena.Servidor.DTOs;

public class PreguntaImportacionDto
{
    public string Categoria { get; set; } = "";
    public string Dificultad { get; set; } = "";
    public string Enunciado { get; set; } = "";

    public string OpcionA { get; set; } = "";
    public string OpcionB { get; set; } = "";
    public string OpcionC { get; set; } = "";
    public string OpcionD { get; set; } = "";

    public string Correcta { get; set; } = "";

    public string Fuente { get; set; } = "";
    public string Licencia { get; set; } = "";
}

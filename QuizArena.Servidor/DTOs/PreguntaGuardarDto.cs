using System.ComponentModel.DataAnnotations;

namespace QuizArena.Servidor.DTOs;

public class PreguntaGuardarDto
{
    public int CategoriaId { get; set; }

    [Required]
    public string Dificultad { get; set; } = "";

    [Required]
    public string Enunciado { get; set; } = "";

    [Required]
    public string OpcionA { get; set; } = "";

    [Required]
    public string OpcionB { get; set; } = "";

    [Required]
    public string OpcionC { get; set; } = "";

    [Required]
    public string OpcionD { get; set; } = "";

    [Required]
    public string Correcta { get; set; } = "";

    [Required]
    public string Fuente { get; set; } = "";

    [Required]
    public string Licencia { get; set; } = "";
}

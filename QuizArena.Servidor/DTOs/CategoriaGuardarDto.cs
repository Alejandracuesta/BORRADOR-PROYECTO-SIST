using System.ComponentModel.DataAnnotations;

namespace QuizArena.Servidor.DTOs;

public class CategoriaGuardarDto
{
    [Required]
    [StringLength(60, MinimumLength = 3)]
    public string Nombre { get; set; } = "";
}

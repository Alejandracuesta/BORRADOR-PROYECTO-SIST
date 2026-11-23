using System.ComponentModel.DataAnnotations;

namespace QuizArena.Servidor.DTOs;

public class CrearJugadorDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(20, MinimumLength = 3,
        ErrorMessage = "El nombre debe tener entre 3 y 20 caracteres")]
    public string Nombre { get; set; } = "";
}

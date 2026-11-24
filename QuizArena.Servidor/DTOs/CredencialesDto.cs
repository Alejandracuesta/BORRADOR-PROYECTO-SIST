using System.ComponentModel.DataAnnotations;

namespace QuizArena.Servidor.DTOs;

public class CredencialesDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(20, MinimumLength = 3,
        ErrorMessage = "El nombre debe tener entre 3 y 20 caracteres")]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(64, MinimumLength = 6,
        ErrorMessage = "La contraseña debe tener entre 6 y 64 caracteres")]
    public string Clave { get; set; } = "";
}

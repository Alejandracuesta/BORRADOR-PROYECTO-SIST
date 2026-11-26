using System.ComponentModel.DataAnnotations;

namespace QuizArena.Servidor.DTOs;

public class EditarMensajeDto
{
    [Required]
    [StringLength(250, MinimumLength = 1)]
    public string Texto { get; set; } = "";
}

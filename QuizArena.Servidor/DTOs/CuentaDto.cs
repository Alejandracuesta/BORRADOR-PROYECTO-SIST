using System.ComponentModel.DataAnnotations;

namespace QuizArena.Servidor.DTOs;

public class ActualizarNombreDto
{
    [Required]
    [StringLength(20, MinimumLength = 3)]
    public string NuevoNombre { get; set; } = "";

    [Required]
    public string ClaveActual { get; set; } = "";
}

public class CambiarClaveDto
{
    [Required]
    public string ClaveActual { get; set; } = "";

    [Required]
    [StringLength(64, MinimumLength = 6)]
    public string ClaveNueva { get; set; } = "";
}

public class EliminarCuentaDto
{
    [Required]
    public string ClaveActual { get; set; } = "";
}

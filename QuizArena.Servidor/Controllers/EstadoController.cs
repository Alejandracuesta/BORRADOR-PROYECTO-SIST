using Microsoft.AspNetCore.Mvc;

namespace QuizArena.Servidor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EstadoController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            aplicacion = "Quiz Arena",
            estado = "Servidor activo",
            fechaServidor = DateTime.UtcNow
        });
    }
}

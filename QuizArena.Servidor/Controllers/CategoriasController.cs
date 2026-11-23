using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizArena.Servidor.Data;

namespace QuizArena.Servidor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly QuizArenaDbContext db;

    public CategoriasController(QuizArenaDbContext contexto)
    {
        db = contexto;
    }

    // GET /api/categorias
    [HttpGet]
    public async Task<IActionResult> Obtener(CancellationToken cancellationToken)
    {
        var categorias = await db.Categorias
            .AsNoTracking()
            .OrderBy(c => c.Nombre)
            .Select(c => new
            {
                c.Id,
                c.Nombre,
                TotalPreguntas = c.Preguntas.Count(p => p.Activa)
            })
            .ToListAsync(cancellationToken);

        return Ok(categorias);
    }
}

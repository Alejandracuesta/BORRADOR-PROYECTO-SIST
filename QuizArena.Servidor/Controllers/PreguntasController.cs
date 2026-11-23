using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizArena.Servidor.Data;
using QuizArena.Servidor.DTOs;
using QuizArena.Servidor.Models;

namespace QuizArena.Servidor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PreguntasController : ControllerBase
{
    private readonly QuizArenaDbContext db;

    public PreguntasController(QuizArenaDbContext contexto)
    {
        db = contexto;
    }

    // GET /api/preguntas
    [HttpGet]
    public async Task<IActionResult> Obtener(
        [FromQuery] int? categoriaId,
        [FromQuery] DificultadPregunta? dificultad,
        CancellationToken cancellationToken)
    {
        if (FiltrosInvalidos(categoriaId, dificultad))
            return BadRequest(new { mensaje = "Filtros inválidos" });

        var preguntas = await Filtrar(categoriaId, dificultad)
            .OrderBy(p => p.Id)
            .Take(50)
            .ToListAsync(cancellationToken);

        return Ok(preguntas.Select(Mapear).ToList());
    }

    // GET /api/preguntas/aleatorias?cantidad=5
    [HttpGet("aleatorias")]
    public async Task<IActionResult> Aleatorias(
        [FromQuery] int cantidad,
        [FromQuery] int? categoriaId,
        [FromQuery] DificultadPregunta? dificultad,
        CancellationToken cancellationToken)
    {
        if (cantidad is < 1 or > 15)
        {
            return BadRequest(new
            {
                mensaje = "La cantidad debe estar entre 1 y 15"
            });
        }

        if (FiltrosInvalidos(categoriaId, dificultad))
            return BadRequest(new { mensaje = "Filtros inválidos" });

        var disponibles = await Filtrar(categoriaId, dificultad)
            .ToListAsync(cancellationToken);

        if (disponibles.Count < cantidad)
        {
            return Conflict(new
            {
                mensaje = "No hay suficientes preguntas disponibles",
                disponibles = disponibles.Count
            });
        }

        // Mezclar índices garantiza que una pregunta solo se elija una vez por petición.
        int[] indices = Enumerable.Range(0, disponibles.Count).ToArray();
        Random.Shared.Shuffle(indices.AsSpan());

        var elegidas = indices
            .Take(cantidad)
            .Select(i => Mapear(disponibles[i]))
            .ToList();

        return Ok(elegidas);
    }

    // GET /api/preguntas/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var pregunta = await db.Preguntas
            .AsNoTracking()
            .Include(p => p.Categoria)
            .SingleOrDefaultAsync(p => p.Id == id && p.Activa, cancellationToken);

        if (pregunta is null)
            return NotFound(new { mensaje = "Pregunta no encontrada" });

        return Ok(Mapear(pregunta));
    }

    private IQueryable<Pregunta> Filtrar(int? categoriaId, DificultadPregunta? dificultad)
    {
        IQueryable<Pregunta> consulta = db.Preguntas
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Where(p => p.Activa);

        if (categoriaId.HasValue)
            consulta = consulta.Where(p => p.CategoriaId == categoriaId.Value);

        if (dificultad.HasValue)
            consulta = consulta.Where(p => p.Dificultad == dificultad.Value);

        return consulta;
    }

    private static bool FiltrosInvalidos(int? categoriaId, DificultadPregunta? dificultad)
    {
        return (categoriaId.HasValue && categoriaId.Value <= 0)
            || (dificultad.HasValue && !Enum.IsDefined(dificultad.Value));
    }

    private static PreguntaPublicaDto Mapear(Pregunta pregunta)
    {
        return new PreguntaPublicaDto(
            pregunta.Id,
            pregunta.Enunciado,
            pregunta.CategoriaId,
            pregunta.Categoria.Nombre,
            pregunta.Dificultad.ToString(),
            pregunta.OpcionA,
            pregunta.OpcionB,
            pregunta.OpcionC,
            pregunta.OpcionD);
    }
}

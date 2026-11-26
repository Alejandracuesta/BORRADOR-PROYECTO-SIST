using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.Sqlite;
using QuizArena.Servidor.Services;
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
    private readonly CoordinadorSalas coordinador;

    public PreguntasController(QuizArenaDbContext contexto, CoordinadorSalas coordinadorSalas)
    {
        db = contexto;
        coordinador = coordinadorSalas;
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
[Authorize(Roles = "Administrador")]
[HttpGet("admin")]
public async Task<IActionResult> ListadoAdmin(
    int pagina = 1,
    int tamano = 50,
    CancellationToken ct = default)
{
    pagina = Math.Max(1, pagina);
    tamano = Math.Clamp(tamano, 1, 100);

    var preguntas = await db.Preguntas
        .AsNoTracking()
        .OrderBy(p => p.Id)
        .Skip((int)Math.Min((long)(pagina - 1) * tamano, int.MaxValue))
        .Take(tamano)
        .Select(p => new
        {
            p.Id,
            p.CategoriaId,
            p.Dificultad,
            p.Enunciado,
            p.OpcionA,
            p.OpcionB,
            p.OpcionC,
            p.OpcionD,
            Correcta = p.RespuestaCorrecta,
            p.Fuente,
            p.Licencia,
            p.Activa
        })
        .ToListAsync(ct);

    return Ok(preguntas);
}
[Authorize(Roles = "Administrador")]
[HttpPost]
public async Task<IActionResult> CrearPregunta(
    PreguntaGuardarDto datos,
    CancellationToken ct)
{
    string? error = ValidadorPreguntas.Validar(datos, out var dificultad);

    if (error != null)
        return BadRequest(new { mensaje = error });

    await coordinador.Semaforo.WaitAsync(ct);

    try
    {
        bool categoriaExiste = await db.Categorias
            .AnyAsync(c => c.Id == datos.CategoriaId, ct);

        if (!categoriaExiste)
            return BadRequest(new { mensaje = "Categoría inexistente" });

        string enunciado = datos.Enunciado.Trim();

        var pregunta = new Pregunta
        {
            CategoriaId = datos.CategoriaId,
            Dificultad = dificultad,
            Enunciado = enunciado,
            EnunciadoNormalizado = enunciado.ToUpperInvariant(),
            OpcionA = datos.OpcionA.Trim(),
            OpcionB = datos.OpcionB.Trim(),
            OpcionC = datos.OpcionC.Trim(),
            OpcionD = datos.OpcionD.Trim(),
            RespuestaCorrecta = datos.Correcta.Trim().ToUpperInvariant(),
            Fuente = datos.Fuente.Trim(),
            Licencia = datos.Licencia.Trim(),
            Activa = true
        };

        db.Preguntas.Add(pregunta);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqliteException sql &&
                  sql.SqliteExtendedErrorCode == 2067)
        {
            return Conflict(new
            {
                mensaje = "Ya existe ese enunciado"
            });
        }

        return CreatedAtAction(
            nameof(ObtenerAdminPorId),
            new { id = pregunta.Id },
            new { pregunta.Id, pregunta.Enunciado });
    }
    finally
    {
        coordinador.Semaforo.Release();
    }
}
[Authorize(Roles = "Administrador")]
[HttpPut("{id:int}")]
public async Task<IActionResult> ActualizarPregunta(
    int id,
    PreguntaGuardarDto datos,
    CancellationToken ct)
{
    string? error = ValidadorPreguntas.Validar(datos, out var dificultad);

    if (error != null)
        return BadRequest(new { mensaje = error });

    await coordinador.Semaforo.WaitAsync(ct);

    try
    {
        var pregunta = await db.Preguntas
            .SingleOrDefaultAsync(p => p.Id == id, ct);

        if (pregunta == null)
            return NotFound();

        bool utilizada = await db.PartidasPreguntas
            .AnyAsync(p => p.PreguntaId == id, ct);

        if (utilizada)
        {
            return Conflict(new
            {
                mensaje = "La pregunta tiene historial. Desactívala y crea una nueva."
            });
        }

        bool categoriaExiste = await db.Categorias
            .AnyAsync(c => c.Id == datos.CategoriaId, ct);

        if (!categoriaExiste)
            return BadRequest(new { mensaje = "Categoría inexistente" });

        pregunta.CategoriaId = datos.CategoriaId;
        pregunta.Dificultad = dificultad;
        pregunta.Enunciado = datos.Enunciado.Trim();
        pregunta.EnunciadoNormalizado =
            pregunta.Enunciado.ToUpperInvariant();

        pregunta.OpcionA = datos.OpcionA.Trim();
        pregunta.OpcionB = datos.OpcionB.Trim();
        pregunta.OpcionC = datos.OpcionC.Trim();
        pregunta.OpcionD = datos.OpcionD.Trim();

        pregunta.RespuestaCorrecta =
            datos.Correcta.Trim().ToUpperInvariant();

        pregunta.Fuente = datos.Fuente.Trim();
        pregunta.Licencia = datos.Licencia.Trim();

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqliteException sql &&
                  sql.SqliteExtendedErrorCode == 2067)
        {
            return Conflict(new
            {
                mensaje = "Ya existe una pregunta con ese enunciado"
            });
        }

        return NoContent();
    }
    finally
    {
        coordinador.Semaforo.Release();
    }
}
[Authorize(Roles = "Administrador")]
[HttpDelete("{id:int}")]
public async Task<IActionResult> DesactivarPregunta(
    int id, CancellationToken ct)
{
    await coordinador.Semaforo.WaitAsync(ct);

    try
    {
        var pregunta = await db.Preguntas
            .SingleOrDefaultAsync(p => p.Id == id, ct);

        if (pregunta == null)
            return NotFound();

        pregunta.Activa = false;

        await db.SaveChangesAsync(ct);

        return NoContent();
    }
    finally
    {
        coordinador.Semaforo.Release();
    }
}
[Authorize(Roles = "Administrador")]
[HttpPut("{id:int}/activar")]
public async Task<IActionResult> ActivarPregunta(
    int id, CancellationToken ct)
{
    await coordinador.Semaforo.WaitAsync(ct);

    try
    {
        var pregunta = await db.Preguntas
            .SingleOrDefaultAsync(p => p.Id == id, ct);

        if (pregunta == null)
            return NotFound();

        pregunta.Activa = true;

        await db.SaveChangesAsync(ct);

        return NoContent();
    }
    finally
    {
        coordinador.Semaforo.Release();
    }
}
    [Authorize(Roles = "Administrador")]
    [HttpGet("admin/{id:int}")]
    public async Task<IActionResult> ObtenerAdminPorId(int id, CancellationToken ct)
    {
        var pregunta = await db.Preguntas.AsNoTracking().Where(p => p.Id == id)
            .Select(p => new { p.Id, p.CategoriaId, p.Dificultad, p.Enunciado,
                p.OpcionA, p.OpcionB, p.OpcionC, p.OpcionD,
                Correcta = p.RespuestaCorrecta, p.Fuente, p.Licencia, p.Activa })
            .SingleOrDefaultAsync(ct);
        return pregunta is null ? NotFound() : Ok(pregunta);
    }

}

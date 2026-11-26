using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.Sqlite;
using QuizArena.Servidor.DTOs;
using QuizArena.Servidor.Models;
using QuizArena.Servidor.Services;
using Microsoft.EntityFrameworkCore;
using QuizArena.Servidor.Data;

namespace QuizArena.Servidor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly QuizArenaDbContext db;
    private readonly CoordinadorSalas coordinador;

    public CategoriasController(QuizArenaDbContext contexto, CoordinadorSalas coordinadorSalas)
    {
        db = contexto;
        coordinador = coordinadorSalas;
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

[HttpGet("{id:int}")]
public async Task<IActionResult> ObtenerPorId(
    int id, CancellationToken ct)
{
    var categoria = await db.Categorias
        .AsNoTracking()
        .Where(c => c.Id == id)
        .Select(c => new
        {
            c.Id,
            c.Nombre,
            TotalPreguntas = c.Preguntas.Count(p => p.Activa)
        })
        .SingleOrDefaultAsync(ct);

    return categoria == null
        ? NotFound()
        : Ok(categoria);
}

[Authorize(Roles = "Administrador")]
[HttpPost]
public async Task<IActionResult> Crear(
    CategoriaGuardarDto datos, CancellationToken ct)
{
    await coordinador.Semaforo.WaitAsync(ct);
    try {

    string nombre = datos.Nombre.Trim();

    if (nombre.Length is < 3 or > 60)
        return BadRequest(new { mensaje = "Nombre inválido" });

    string normalizado = nombre.ToUpperInvariant();

    if (await db.Categorias.AnyAsync(
        c => c.NombreNormalizado == normalizado, ct))
    {
        return Conflict(new { mensaje = "Categoría duplicada" });
    }

    var categoria = new Categoria
    {
        Nombre = nombre,
        NombreNormalizado = normalizado
    };

    db.Categorias.Add(categoria);

    try
    {
        await db.SaveChangesAsync(ct);
    }
    catch (DbUpdateException ex)
        when (ex.InnerException is SqliteException sql &&
              sql.SqliteExtendedErrorCode == 2067)
    {
        return Conflict(new { mensaje = "Categoría duplicada" });
    }

    return CreatedAtAction(
        nameof(ObtenerPorId),
        new { id = categoria.Id },
        new { categoria.Id, categoria.Nombre });

    } finally { coordinador.Semaforo.Release(); }
}
[Authorize(Roles = "Administrador")]
[HttpPut("{id:int}")]
public async Task<IActionResult> Actualizar(
    int id, CategoriaGuardarDto datos, CancellationToken ct)
{
    await coordinador.Semaforo.WaitAsync(ct);
    try {

    var categoria = await db.Categorias
        .SingleOrDefaultAsync(c => c.Id == id, ct);

    if (categoria == null)
        return NotFound();

    string nombre = datos.Nombre.Trim();

    if (nombre.Length is < 3 or > 60)
        return BadRequest(new { mensaje = "Nombre inválido" });

    categoria.Nombre = nombre;
    categoria.NombreNormalizado = nombre.ToUpperInvariant();

    try
    {
        await db.SaveChangesAsync(ct);
    }
    catch (DbUpdateException ex)
        when (ex.InnerException is SqliteException sql &&
              sql.SqliteExtendedErrorCode == 2067)
    {
        return Conflict(new { mensaje = "Categoría duplicada" });
    }

    return Ok(new { categoria.Id, categoria.Nombre });

    } finally { coordinador.Semaforo.Release(); }
}
[Authorize(Roles = "Administrador")]
[HttpDelete("{id:int}")]
public async Task<IActionResult> Eliminar(
    int id, CancellationToken ct)
{
    await coordinador.Semaforo.WaitAsync(ct);
    try {

    var categoria = await db.Categorias
        .SingleOrDefaultAsync(c => c.Id == id, ct);

    if (categoria == null)
        return NotFound();

    bool tienePreguntas = await db.Preguntas
        .AnyAsync(p => p.CategoriaId == id, ct);

    bool tieneSalas = await db.Salas
        .AnyAsync(s => s.CategoriaId == id, ct);

    if (tienePreguntas || tieneSalas)
    {
        return Conflict(new
        {
            mensaje = "No se puede eliminar una categoría referenciada"
        });
    }

    db.Categorias.Remove(categoria);
    await db.SaveChangesAsync(ct);

    return NoContent();

    } finally { coordinador.Semaforo.Release(); }
}
}

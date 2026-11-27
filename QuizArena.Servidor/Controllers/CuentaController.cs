using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QuizArena.Servidor.Data;
using QuizArena.Servidor.DTOs;
using QuizArena.Servidor.Models;
using QuizArena.Servidor.Services;

namespace QuizArena.Servidor.Controllers;

[ApiController]
[Authorize]
[Route("api/cuenta")]
public class CuentaController : ControllerBase
{
    private readonly QuizArenaDbContext db;
    private readonly CoordinadorSalas coordinador;
    private readonly IPasswordHasher<Jugador> hasher;

    public CuentaController(
        QuizArenaDbContext contexto,
        IPasswordHasher<Jugador> passwordHasher, CoordinadorSalas coordinadorSalas)
    {
        db = contexto;
        coordinador = coordinadorSalas;
        hasher = passwordHasher;
    }

    private async Task<Jugador?> JugadorActualAsync(
        CancellationToken ct)
    {
        string? valor =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(valor, out int id))
            return null;

        return await db.Jugadores.SingleOrDefaultAsync(
            j => j.Id == id && j.Activo, ct);
    }

    private bool ComprobarClave(Jugador jugador, string clave)
    {
        if (string.IsNullOrEmpty(jugador.PasswordHash))
            return false;

        return hasher.VerifyHashedPassword(
            jugador, jugador.PasswordHash, clave)
            != PasswordVerificationResult.Failed;
    }

    // GET /api/cuenta
    [HttpGet]
    public async Task<IActionResult> Obtener(CancellationToken ct)
    {
        var jugador = await JugadorActualAsync(ct);

        if (jugador == null)
            return Unauthorized();

        return Ok(new { jugador.Id, jugador.Nombre, jugador.FechaRegistroUtc, jugador.EsAdministrador });
    }

    // PUT /api/cuenta
    [HttpPut]
    public async Task<IActionResult> ActualizarNombre(
        ActualizarNombreDto datos,
        CancellationToken ct)
    {
        await coordinador.Semaforo.WaitAsync(ct);
        try
        {
        var jugador = await JugadorActualAsync(ct);

        if (jugador == null)
            return Unauthorized();

        if (!ComprobarClave(jugador, datos.ClaveActual))
            return Unauthorized(new
            {
                mensaje = "Contraseña actual incorrecta"
            });

        string nuevoNombre = datos.NuevoNombre.Trim();

        if (nuevoNombre.Length is < 3 or > 20 ||
            nuevoNombre.StartsWith(
                "BAJA_", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                mensaje = "Nombre no permitido"
            });
        }

        jugador.Nombre = nuevoNombre;
        jugador.NombreNormalizado =
            nuevoNombre.ToUpperInvariant();

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
                mensaje = "Ese nombre ya está registrado"
            });
        }

        return Ok(new { jugador.Id, jugador.Nombre, jugador.FechaRegistroUtc, jugador.EsAdministrador });
        }
        finally { coordinador.Semaforo.Release(); }
    }

    // PUT /api/cuenta/clave
    [HttpPut("clave")]
    public async Task<IActionResult> CambiarClave(
        CambiarClaveDto datos,
        CancellationToken ct)
    {
        await coordinador.Semaforo.WaitAsync(ct);
        try
        {
        var jugador = await JugadorActualAsync(ct);

        if (jugador == null)
            return Unauthorized();

        if (!ComprobarClave(jugador, datos.ClaveActual))
            return Unauthorized(new
            {
                mensaje = "Contraseña actual incorrecta"
            });

        jugador.PasswordHash =
            hasher.HashPassword(jugador, datos.ClaveNueva);

        await db.SaveChangesAsync(ct);

        return NoContent();
        }
        finally { coordinador.Semaforo.Release(); }
    }

    // DELETE /api/cuenta
    [HttpDelete]
    public async Task<IActionResult> Eliminar(
        EliminarCuentaDto datos,
        CancellationToken ct)
    {
        await coordinador.Semaforo.WaitAsync(ct);
        try
        {
        var jugador = await JugadorActualAsync(ct);

        if (jugador == null)
            return Unauthorized();

        if (!ComprobarClave(jugador, datos.ClaveActual))
            return Unauthorized(new
            {
                mensaje = "Contraseña actual incorrecta"
            });

        bool perteneceSala = await db.ParticipantesSala
            .AnyAsync(p => p.JugadorId == jugador.Id, ct);

        if (perteneceSala)
        {
            return Conflict(new
            {
                mensaje = "Debes abandonar tu sala antes de eliminar la cuenta"
            });
        }

        // Conservamos el registro para mantener
        // las referencias históricas.
        jugador.Activo = false;
        jugador.EsAdministrador = false;
        jugador.EliminadoUtc = DateTime.UtcNow;
        jugador.PasswordHash = null;

        jugador.Nombre = "Cuenta eliminada";
        jugador.NombreNormalizado = $"BAJA_{jugador.Id}";

        await db.SaveChangesAsync(ct);

        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return NoContent();
        }
        finally { coordinador.Semaforo.Release(); }
    }
}

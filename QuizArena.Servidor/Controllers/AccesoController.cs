using System.Globalization;
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

namespace QuizArena.Servidor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccesoController : ControllerBase
{
    private readonly QuizArenaDbContext db;
    private readonly IPasswordHasher<Jugador> hasher;

    public AccesoController(
        QuizArenaDbContext contexto,
        IPasswordHasher<Jugador> passwordHasher)
    {
        db = contexto;
        hasher = passwordHasher;
    }

    // POST /api/acceso/registrar
    [AllowAnonymous]
    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar(
        CredencialesDto datos, CancellationToken cancellationToken)
    {
        string nombre = datos.Nombre.Trim();

        if (nombre.Length < 3)
            return BadRequest(new { mensaje = "Nombre inválido" });

        var jugador = new Jugador
        {
            Nombre = nombre,
            NombreNormalizado = nombre.ToUpperInvariant(),
            FechaRegistroUtc = DateTime.UtcNow
        };

        jugador.PasswordHash = hasher.HashPassword(jugador, datos.Clave);
        db.Jugadores.Add(jugador);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqliteException error &&
                  error.SqliteExtendedErrorCode == 2067)
        {
            return Conflict(new { mensaje = "El nombre de usuario ya existe" });
        }

        return StatusCode(StatusCodes.Status201Created, Convertir(jugador));
    }

    // POST /api/acceso/entrar
    [AllowAnonymous]
    [HttpPost("entrar")]
    public async Task<IActionResult> Entrar(
        CredencialesDto datos, CancellationToken cancellationToken)
    {
        string nombreNormalizado = datos.Nombre.Trim().ToUpperInvariant();

        // Se sigue la entidad para poder actualizar un hash que necesite renovación.
        var jugador = await db.Jugadores.SingleOrDefaultAsync(
            j => j.NombreNormalizado == nombreNormalizado, cancellationToken);

        if (jugador is null || string.IsNullOrEmpty(jugador.PasswordHash))
            return CredencialesIncorrectas();

        var resultado = hasher.VerifyHashedPassword(
            jugador, jugador.PasswordHash, datos.Clave);

        if (resultado == PasswordVerificationResult.Failed)
            return CredencialesIncorrectas();

        if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
        {
            jugador.PasswordHash = hasher.HashPassword(jugador, datos.Clave);
            await db.SaveChangesAsync(cancellationToken);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, jugador.Id.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Name, jugador.Nombre)
        };

        var identidad = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidad));

        return Ok(Convertir(jugador));
    }

    // GET /api/acceso/yo
    [Authorize]
    [HttpGet("yo")]
    public async Task<IActionResult> Yo(CancellationToken cancellationToken)
    {
        string? identificador = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(identificador, NumberStyles.None,
                CultureInfo.InvariantCulture, out int id))
            return Unauthorized();

        var jugador = await db.Jugadores
            .AsNoTracking()
            .SingleOrDefaultAsync(j => j.Id == id, cancellationToken);

        if (jugador is null)
            return Unauthorized();

        return Ok(Convertir(jugador));
    }

    // POST /api/acceso/salir
    [Authorize]
    [HttpPost("salir")]
    public async Task<IActionResult> Salir()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return NoContent();
    }

    private UnauthorizedObjectResult CredencialesIncorrectas()
    {
        return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos" });
    }

    private static JugadorRespuestaDto Convertir(Jugador jugador)
    {
        return new JugadorRespuestaDto(
            jugador.Id, jugador.Nombre, jugador.FechaRegistroUtc);
    }
}

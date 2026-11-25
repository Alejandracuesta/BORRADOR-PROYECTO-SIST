using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QuizArena.Servidor.Data;
using QuizArena.Servidor.Hubs;
using QuizArena.Servidor.Models;
using QuizArena.Servidor.Services;

// Este argumento inicia únicamente la operación local de mantenimiento.
bool importarPreguntas = args.Contains("--importar-preguntas");
string? argumentoArchivo = args.FirstOrDefault(
    a => a.StartsWith("--archivo=", StringComparison.OrdinalIgnoreCase));

string[] argumentosWeb = args
    .Where(a => a != "--importar-preguntas" &&
                !a.StartsWith("--archivo=", StringComparison.OrdinalIgnoreCase))
    .ToArray();

var builder = WebApplication.CreateBuilder(argumentosWeb);

// Registrar los controladores de nuestra API REST.
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<NotificadorTiempoReal>();
builder.Services.AddSingleton<ControlFrecuencia>();
builder.Services.AddHostedService<SupervisorPartidas>();
builder.Services.AddSingleton<CoordinadorSalas>();

// Una única base de datos del servidor, compartida por todos los clientes.
string rutaBaseDatos = Path.Combine(
    builder.Environment.ContentRootPath,
    "quizarena.db");

string conexionSqlite = new SqliteConnectionStringBuilder
{
    DataSource = rutaBaseDatos
}.ToString();

builder.Services.AddDbContext<QuizArenaDbContext>(options =>
    options.UseSqlite(conexionSqlite));

builder.Services.AddScoped<IPasswordHasher<Jugador>, PasswordHasher<Jugador>>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.Cookie.Name = "QuizArena.Sesion";
        opciones.Cookie.HttpOnly = true;
        opciones.Cookie.SameSite = SameSiteMode.Strict;
        opciones.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        opciones.ExpireTimeSpan = TimeSpan.FromHours(4);
        opciones.SlidingExpiration = true;

        // Una API devuelve estados HTTP en lugar de redirigir a formularios web.
        opciones.Events.OnRedirectToLogin = contexto =>
        {
            contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        opciones.Events.OnRedirectToAccessDenied = contexto =>
        {
            contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

await using var app = builder.Build();

// Aplicar las migraciones pendientes al arrancar. Solo añade lo que falte
// (tablas, columnas, índices); no borra jugadores, preguntas ni partidas.
await using (var scopeMigraciones = app.Services.CreateAsyncScope())
{
    var dbMigraciones = scopeMigraciones.ServiceProvider
        .GetRequiredService<QuizArenaDbContext>();
    await dbMigraciones.Database.MigrateAsync();
}

if (importarPreguntas)
{
    string nombreArchivo = argumentoArchivo is null
        ? "preguntas.json"
        : argumentoArchivo["--archivo=".Length..];

    // La importación solo admite un nombre de archivo JSON dentro de Datos.
    if (string.IsNullOrWhiteSpace(nombreArchivo) ||
        nombreArchivo != Path.GetFileName(nombreArchivo) ||
        nombreArchivo.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
        !nombreArchivo.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
    {
        Console.Error.WriteLine("Nombre de archivo JSON inválido.");
        Environment.ExitCode = 1;
        return;
    }

    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<QuizArenaDbContext>();
    string archivoPreguntas = Path.Combine(
        app.Environment.ContentRootPath, "Datos", nombreArchivo);

    try
    {
        await ImportadorPreguntas.ImportarAsync(db, archivoPreguntas);
    }
    catch (Exception ex) when (
        ex is InvalidDataException or IOException or UnauthorizedAccessException)
    {
        Console.Error.WriteLine($"No se han importado preguntas: {ex.Message}");
        Environment.ExitCode = 1;
    }

    return;
}

// Asociar las rutas HTTP con los controladores.
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<SalaHub>("/hubs/salas");

await app.RunAsync();

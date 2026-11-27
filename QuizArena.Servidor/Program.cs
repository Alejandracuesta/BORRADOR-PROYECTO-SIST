using Microsoft.AspNetCore.Authentication;
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

string? argumentoAdmin = args.FirstOrDefault(a => a.StartsWith("--hacer-admin=", StringComparison.OrdinalIgnoreCase));
string[] argumentosWeb = args
    .Where(a => a != "--importar-preguntas" &&
                !a.StartsWith("--archivo=", StringComparison.OrdinalIgnoreCase) &&
                !a.StartsWith("--hacer-admin=", StringComparison.OrdinalIgnoreCase))
    .ToArray();

var builder = WebApplication.CreateBuilder(argumentosWeb);

// Registrar los controladores de nuestra API REST.
builder.Services.AddControllers();
builder.Services.AddOpenApi(opciones => opciones.AddOperationTransformer((operacion, contexto, ct) =>
{
    var permisos = contexto.Description.ActionDescriptor.EndpointMetadata
        .OfType<Microsoft.AspNetCore.Authorization.IAuthorizeData>().ToList();
    string acceso = permisos.Any(p => p.Roles?.Contains("Administrador") == true)
        ? "Solo Administrador" : permisos.Count > 0 ? "Sesión autenticada" : "Público";
    operacion.Description = $"Acceso: {acceso}. " + operacion.Description;
    return Task.CompletedTask;
}));
builder.Services.AddSignalR();
builder.Services.AddSingleton<NotificadorTiempoReal>();
builder.Services.AddSingleton<ControlFrecuencia>();
builder.Services.AddHostedService<SupervisorPartidas>();
builder.Services.AddSingleton<CoordinadorSalas>();

// Una única base de datos del servidor, compartida por todos los clientes.
string rutaBaseDatos = Path.GetFullPath(builder.Configuration["BaseDatos:Ruta"] ?? "quizarena.db",
    builder.Environment.ContentRootPath);

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

opciones.Events.OnValidatePrincipal = async contexto =>
{
    string? valor = contexto.Principal?
        .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?
        .Value;

    if (!int.TryParse(valor, out int id))
    {
        contexto.RejectPrincipal();
        return;
    }

    var db = contexto.HttpContext.RequestServices
        .GetRequiredService<QuizArenaDbContext>();

    var jugador = await db.Jugadores
        .AsNoTracking()
        .SingleOrDefaultAsync(
            j => j.Id == id,
            contexto.HttpContext.RequestAborted);

    if (jugador == null || !jugador.Activo)
    {
        contexto.RejectPrincipal();

        await contexto.HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return;
    }

    // Actualiza el nombre y los permisos incluidos en la cookie.
    bool esAdminEnCookie =
        contexto.Principal!.IsInRole("Administrador");

    if (contexto.Principal.Identity?.Name != jugador.Nombre ||
        esAdminEnCookie != jugador.EsAdministrador)
    {
        var claims = new List<System.Security.Claims.Claim>
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier,
                jugador.Id.ToString()),

            new(System.Security.Claims.ClaimTypes.Name,
                jugador.Nombre)
        };

        if (jugador.EsAdministrador)
        {
            claims.Add(new(
                System.Security.Claims.ClaimTypes.Role,
                "Administrador"));
        }

        var identidad = new System.Security.Claims.ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        contexto.ReplacePrincipal(
            new System.Security.Claims.ClaimsPrincipal(identidad));

        contexto.ShouldRenew = true;
    }
};

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

if (argumentoAdmin != null)
{
    string nombre = argumentoAdmin[
        "--hacer-admin=".Length..].Trim();

    await using var scope = app.Services.CreateAsyncScope();

    var db = scope.ServiceProvider
        .GetRequiredService<QuizArenaDbContext>();

    var jugador = await db.Jugadores.SingleOrDefaultAsync(
        j => j.NombreNormalizado == nombre.ToUpperInvariant()
             && j.Activo);

    if (jugador == null ||
        string.IsNullOrEmpty(jugador.PasswordHash))
    {
        Console.Error.WriteLine(
            "No existe esa cuenta activa con contraseña.");

        Environment.ExitCode = 1;
        return;
    }

    jugador.EsAdministrador = true;
    await db.SaveChangesAsync();

    Console.WriteLine(
        $"Administrador habilitado: {jugador.Nombre}");

    return;
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

app.MapOpenApi();
string paginaSwagger = """
    <!DOCTYPE html>
    <html lang="es">
    <head>
      <meta charset="utf-8">
      <title>Quiz Arena API</title>
      <link rel="stylesheet" href="https://unpkg.com/swagger-ui-dist@5/swagger-ui.css">
    </head>
    <body>
      <div id="swagger-ui"></div>
      <script src="https://unpkg.com/swagger-ui-dist@5/swagger-ui-bundle.js"></script>
      <script>
        window.onload = () => {
          window.ui = SwaggerUIBundle({
            url: '/openapi/v1.json',
            dom_id: '#swagger-ui',
            withCredentials: true
          });
        };
      </script>
    </body>
    </html>
""";
app.MapGet("/swagger/index.html", () => Results.Content(paginaSwagger, "text/html; charset=utf-8")).ExcludeFromDescription();
app.MapGet("/swagger", () => Results.Redirect("/swagger/index.html")).ExcludeFromDescription();
app.MapGet("/", () => Results.Redirect("/swagger/index.html")).ExcludeFromDescription();
await app.RunAsync();
public partial class Program { }

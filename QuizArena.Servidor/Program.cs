using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QuizArena.Servidor.Data;

// Este argumento inicia únicamente la operación local de mantenimiento.
bool importarPreguntas = args.Contains("--importar-preguntas");
string[] argumentosWeb = args
    .Where(a => a != "--importar-preguntas")
    .ToArray();

var builder = WebApplication.CreateBuilder(argumentosWeb);

// Registrar los controladores de nuestra API REST.
builder.Services.AddControllers();

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

await using var app = builder.Build();

if (importarPreguntas)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<QuizArenaDbContext>();
    string archivoPreguntas = Path.Combine(
        app.Environment.ContentRootPath, "Datos", "preguntas.json");

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
app.MapControllers();

await app.RunAsync();

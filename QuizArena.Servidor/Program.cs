using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QuizArena.Servidor.Data;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Asociar las rutas HTTP con los controladores.
app.MapControllers();

app.Run();

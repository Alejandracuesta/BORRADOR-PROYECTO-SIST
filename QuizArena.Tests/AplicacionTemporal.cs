using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuizArena.Servidor.Services;

namespace QuizArena.Tests;

// Nunca utiliza la base real: la ruta única se fija antes de construir Program.
public sealed class AplicacionTemporal : WebApplicationFactory<Program>
{
    public string Directorio { get; } = Path.Combine(Path.GetTempPath(), "QuizArena.Tests", Guid.NewGuid().ToString("N"));
    public string Ruta => Path.Combine(Directorio, "pruebas.db");
    private readonly string? anterior;
    public AplicacionTemporal()
    {
        Directory.CreateDirectory(Directorio);
        anterior = Environment.GetEnvironmentVariable("BaseDatos__Ruta");
        Environment.SetEnvironmentVariable("BaseDatos__Ruta", Ruta);
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("BaseDatos:Ruta", Ruta);
        builder.ConfigureLogging(l => l.ClearProviders());
    }
    public HttpClient Cliente() => CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    public async Task<T> Datos<T>(Func<QuizArenaDbContext, Task<T>> accion)
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<QuizArenaDbContext>();
        Assert.Equal(Path.GetFullPath(Ruta), Path.GetFullPath(db.Database.GetDbConnection().DataSource));
        var coordinador = scope.ServiceProvider.GetRequiredService<CoordinadorSalas>();
        await coordinador.Semaforo.WaitAsync();
        try { return await accion(db); } finally { coordinador.Semaforo.Release(); }
    }
    public async Task<int> Banco()
    {
        return await Datos(async db => {
            var categoria = new Categoria { Nombre = "Categoría temporal", NombreNormalizado = "CATEGORÍA TEMPORAL" };
            db.Categorias.Add(categoria);
            for (int i = 0; i < 6; i++) db.Preguntas.Add(new Pregunta { Categoria = categoria,
                Enunciado = $"¿Pregunta temporal número {i}?", EnunciadoNormalizado = $"¿PREGUNTA TEMPORAL NÚMERO {i}?",
                Dificultad = DificultadPregunta.Media, OpcionA = "Primera", OpcionB = "Segunda", OpcionC = "Tercera", OpcionD = "Cuarta",
                RespuestaCorrecta = "A", Fuente = "Pruebas", Licencia = "Propias", Activa = i != 5 });
            await db.SaveChangesAsync(); return categoria.Id;
        });
    }
    public async Task<(HttpClient cliente, int id)> Cuenta(string nombre, bool admin = false)
    {
        var cliente = Cliente();
        await Api.Es(await cliente.PostAsJsonAsync("/api/acceso/registrar", new { nombre, clave = Api.Clave }), HttpStatusCode.Created);
        var response = await cliente.PostAsJsonAsync("/api/acceso/entrar", new { nombre, clave = Api.Clave });
        var json = await Api.Json(response); int id = json["id"]!.GetValue<int>();
        if (admin) await Datos(async db => { (await db.Jugadores.FindAsync(id))!.EsAdministrador = true; await db.SaveChangesAsync(); return true; });
        return (cliente, id);
    }
    public async Task<string> Sala(HttpClient anfitrion, string modalidad = "Individual", int max = 4)
    {
        var result = await anfitrion.PostAsJsonAsync("/api/salas", new { nombre = "Sala temporal", modalidad, maxJugadores = max, numeroPreguntas = 5, dificultad = 2, esPrivada = true });
        return (await Api.Json(result, HttpStatusCode.Created))["codigo"]!.GetValue<string>();
    }
    public async Task<JsonObject> AbrirPregunta(HttpClient cliente)
    {
        await Datos(async db => { var p = await db.Partidas.OrderByDescending(p => p.Id).FirstAsync(); p.FinFaseUtc = DateTime.UtcNow.AddSeconds(-1); await db.SaveChangesAsync(); return true; });
        return await Api.Json(await cliente.GetAsync("/api/partidas/actual"));
    }
    public async Task Terminar(HttpClient cliente)
    {
        for (int i = 0; i < 20; i++) {
            var state = await Api.Json(await cliente.GetAsync("/api/partidas/actual"));
            if (state["fase"]!.GetValue<string>() == "Finalizada") return;
            await Datos(async db => { var p = await db.Partidas.OrderByDescending(p => p.Id).FirstAsync(); p.FinFaseUtc = DateTime.UtcNow.AddSeconds(-70); await db.SaveChangesAsync(); return true; });
        }
        Assert.Fail("No finalizó la partida temporal");
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Environment.SetEnvironmentVariable("BaseDatos__Ruta", anterior);
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        try { Directory.Delete(Directorio, true); } catch (IOException) { }
    }
}
public static class Api
{
    // Solo credenciales sintéticas de bases temporales.
    public const string Clave = "ClaveTemporal123";
    public static async Task Es(HttpResponseMessage respuesta, HttpStatusCode esperado)
    {
        Assert.True(respuesta.StatusCode == esperado, $"Esperado {(int)esperado}, recibido {(int)respuesta.StatusCode}: {await respuesta.Content.ReadAsStringAsync()}");
    }
    public static async Task<JsonObject> Json(HttpResponseMessage respuesta, HttpStatusCode esperado = HttpStatusCode.OK)
    { await Es(respuesta, esperado); return (await respuesta.Content.ReadFromJsonAsync<JsonObject>())!; }
    public static Task<HttpResponseMessage> BorrarCuenta(HttpClient c, string clave = Clave) => c.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/cuenta") { Content = JsonContent.Create(new { claveActual = clave }) });
    public static object Pregunta(int cat, string enunciado = "¿Pregunta creada desde la API temporal?", string opcionB = "Dos") => new { categoriaId = cat, dificultad = "Media", enunciado, opcionA = "Uno", opcionB, opcionC = "Tres", opcionD = "Cuatro", correcta = "A", fuente = "Pruebas", licencia = "Propias" };
}

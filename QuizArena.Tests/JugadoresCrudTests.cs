namespace QuizArena.Tests;
public class JugadoresCrudTests
{
 [Fact] public async Task NombreClaveYBajaConCookieAnterior()
 {
  using var app = new AplicacionTemporal(); var (c, id) = await app.Cuenta("Cambiante"); var (otro, _) = await app.Cuenta("Ocupado"); using(c) using(otro) using(var vieja = app.Cliente()) {
   await Api.Es(await vieja.PostAsJsonAsync("/api/acceso/entrar", new { nombre = "Cambiante", clave = Api.Clave }), HttpStatusCode.OK);
   await Api.Es(await c.PutAsJsonAsync("/api/cuenta", new { nuevoNombre = "Nuevo", claveActual = "Incorrecta" }), HttpStatusCode.Unauthorized);
   await Api.Es(await c.PutAsJsonAsync("/api/cuenta", new { nuevoNombre = "Ocupado", claveActual = Api.Clave }), HttpStatusCode.Conflict);
   await Api.Es(await c.PutAsJsonAsync("/api/cuenta", new { nuevoNombre = "BAJA_2", claveActual = Api.Clave }), HttpStatusCode.BadRequest);
   Assert.Equal(id, (await Api.Json(await c.PutAsJsonAsync("/api/cuenta", new { nuevoNombre = "Editado", claveActual = Api.Clave })))["id"]!.GetValue<int>());
   Assert.Equal("Editado", (await Api.Json(await vieja.GetAsync("/api/cuenta")))["nombre"]!.GetValue<string>());
   await Api.Es(await c.PutAsJsonAsync("/api/cuenta/clave", new { claveActual = Api.Clave, claveNueva = "ClaveCambiada123" }), HttpStatusCode.NoContent);
   using var login = app.Cliente(); await Api.Es(await login.PostAsJsonAsync("/api/acceso/entrar", new { nombre = "Editado", clave = Api.Clave }), HttpStatusCode.Unauthorized);
   await Api.Es(await Api.BorrarCuenta(c, "Incorrecta"), HttpStatusCode.Unauthorized);
   await Api.Es(await Api.BorrarCuenta(c, "ClaveCambiada123"), HttpStatusCode.NoContent);
   await Api.Es(await vieja.GetAsync("/api/cuenta"), HttpStatusCode.Unauthorized);
   await Api.Es(await login.PostAsJsonAsync("/api/acceso/entrar", new { nombre = "Editado", clave = "ClaveCambiada123" }), HttpStatusCode.Unauthorized);
   await Api.Es(await login.GetAsync($"/api/jugadores/{id}"), HttpStatusCode.NotFound);
   await app.Datos(async db => { var j = await db.Jugadores.FindAsync(id); Assert.False(j!.Activo); Assert.False(j.EsAdministrador); Assert.Null(j.PasswordHash); Assert.Equal("Cuenta eliminada", j.Nombre); Assert.Equal($"BAJA_{id}", j.NombreNormalizado); Assert.NotNull(j.EliminadoUtc); return true; });
  }
 }
 [Fact] public async Task BajaExigeSalirDeSalaYConservaHistorial()
 {
  using var app = new AplicacionTemporal(); await app.Banco(); var (a, id) = await app.Cuenta("HistorialA"); var(b, _) = await app.Cuenta("HistorialB"); using(a) using(b) {
   string code = await app.Sala(a); await Api.Es(await b.PostAsync($"/api/salas/{code}/unirse", null), HttpStatusCode.OK);
   await Api.Es(await Api.BorrarCuenta(a), HttpStatusCode.Conflict);
   foreach(var c in new[]{a,b}) await Api.Es(await c.PutAsJsonAsync($"/api/salas/{code}/preparado",new{preparado=true}),HttpStatusCode.OK);
   await Api.Es(await a.PostAsync($"/api/partidas/{code}/iniciar",null),HttpStatusCode.OK); await app.Terminar(a);
   var history=await a.GetFromJsonAsync<JsonArray>("/api/partidas/historial"); Assert.Single(history!);
   int partida=history![0]!["id"]!.GetValue<int>();
   await Api.Es(await a.DeleteAsync($"/api/salas/{code}/salir"),HttpStatusCode.NoContent);
   await Api.Es(await Api.BorrarCuenta(a),HttpStatusCode.NoContent);
   var resumen=await Api.Json(await b.GetAsync($"/api/partidas/{partida}/resumen"));
   Assert.Contains(resumen["participantes"]!.AsArray(), j=>j!["jugadorId"]!.GetValue<int>()==id && j["nombre"]!.GetValue<string>()=="Cuenta eliminada");
   await app.Datos(async db=> { Assert.Equal(2,await db.PartidasJugadores.CountAsync()); Assert.Equal(5,await db.PartidasPreguntas.CountAsync()); return true; });
  }
 }
}

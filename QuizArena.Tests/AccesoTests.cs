namespace QuizArena.Tests;
public class AccesoTests
{
 [Fact] public async Task RegistroValidacionDuplicadosYCookiesIndependientes()
 {
  using var app = new AplicacionTemporal(); using var anon = app.Cliente();
  await Api.Es(await anon.GetAsync("/api/cuenta"), HttpStatusCode.Unauthorized);
  await Api.Es(await anon.PostAsJsonAsync("/api/acceso/registrar", new { nombre = "BAJA_12", clave = Api.Clave }), HttpStatusCode.BadRequest);
  await Api.Es(await anon.PostAsJsonAsync("/api/jugadores", new { nombre = "Heredado" }), HttpStatusCode.Gone);
  var (a, _) = await app.Cuenta("JugadorA"); var (b, _) = await app.Cuenta("JugadorB"); using(a) using(b) {
   await Api.Es(await anon.PostAsJsonAsync("/api/acceso/registrar", new { nombre = "jugadora", clave = Api.Clave }), HttpStatusCode.Conflict);
   Assert.Equal("JugadorA", (await Api.Json(await a.GetAsync("/api/cuenta")))["nombre"]!.GetValue<string>());
   Assert.False((await Api.Json(await a.GetAsync("/api/cuenta")))["esAdministrador"]!.GetValue<bool>());
   await Api.Es(await a.PostAsync("/api/acceso/salir", null), HttpStatusCode.NoContent);
   await Api.Es(await a.GetAsync("/api/cuenta"), HttpStatusCode.Unauthorized);
   await Api.Es(await b.GetAsync("/api/cuenta"), HttpStatusCode.OK);
  }
 }
 [Fact] public async Task ValidacionRenuevaRolYRevocaCookiesDesactivadas()
 {
  using var app = new AplicacionTemporal(); var (c, id) = await app.Cuenta("Permisos"); using(c) {
   await Api.Es(await c.GetAsync("/api/preguntas/admin"), HttpStatusCode.Forbidden);
   await app.Datos(async db => { (await db.Jugadores.FindAsync(id))!.EsAdministrador = true; await db.SaveChangesAsync(); return true; });
   await Api.Es(await c.GetAsync("/api/preguntas/admin"), HttpStatusCode.OK);
   await app.Datos(async db => { (await db.Jugadores.FindAsync(id))!.Activo = false; await db.SaveChangesAsync(); return true; });
   await Api.Es(await c.GetAsync("/api/cuenta"), HttpStatusCode.Unauthorized);
  }
 }
}

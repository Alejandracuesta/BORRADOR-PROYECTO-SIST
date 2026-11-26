using Microsoft.Extensions.DependencyInjection;
using QuizArena.Servidor.Services;
namespace QuizArena.Tests;
public class IntegracionTests
{
 [Fact] public async Task MigracionesSwaggerYRutasDocumentadas()
 {
  using var app=new AplicacionTemporal();using var c=app.Cliente();await Api.Es(await c.GetAsync("/swagger/index.html"),HttpStatusCode.OK);await Api.Es(await c.GetAsync("/swagger"),HttpStatusCode.Redirect);
  var json=await Api.Json(await c.GetAsync("/openapi/v1.json"));var paths=json["paths"]!.AsObject();
  foreach(var route in new[]{"/api/cuenta","/api/cuenta/clave","/api/categorias/{id}","/api/preguntas/admin/{id}","/api/salas/{codigo}","/api/chat/mensajes/{id}","/api/partidas/historial","/api/partidas/{id}/resumen"})Assert.True(paths.ContainsKey(route),route);
  await app.Datos(async db=>{Assert.Equal(10,(await db.Database.GetAppliedMigrationsAsync()).Count());Assert.False(db.Database.HasPendingModelChanges());return true;});
 }
 [Fact] public async Task SupervisorAvanzaSinSondeoYPersistenciaEnOtroContexto()
 {
  using var app=new AplicacionTemporal();await app.Banco();var(a,_)=await app.Cuenta("SuperA");var(b,_)=await app.Cuenta("SuperB");using(a)using(b){
   string code=await app.Sala(a);await b.PostAsync($"/api/salas/{code}/unirse",null);foreach(var c in new[]{a,b})await c.PutAsJsonAsync($"/api/salas/{code}/preparado",new{preparado=true});await a.PostAsync($"/api/partidas/{code}/iniciar",null);
   await app.Datos(async db=>{(await db.Partidas.SingleAsync()).FinFaseUtc=DateTime.UtcNow.AddMinutes(-10);await db.SaveChangesAsync();return true;});
   bool final=false;for(int i=0;i<30&&!final;i++){await Task.Delay(100);final=await app.Datos(db=>db.Partidas.AllAsync(p=>p.Fase==FasePartida.Finalizada));}
   Assert.True(final);
   using var otro=new QuizArenaDbContext(new DbContextOptionsBuilder<QuizArenaDbContext>().UseSqlite($"Data Source={app.Ruta}").Options);Assert.Equal(1,await otro.Partidas.CountAsync());Assert.Equal(2,await otro.PartidasJugadores.CountAsync());Assert.Equal(5,await otro.PartidasPreguntas.CountAsync());
  }
 }
}

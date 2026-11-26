namespace QuizArena.Tests;
public class SalasTests
{
 [Fact] public async Task UltimaPlazaYCancelacionSegunPermisos()
 {
  using var app=new AplicacionTemporal(); await app.Banco();
  var(a,_)=await app.Cuenta("Anfitrion");var(b,_)=await app.Cuenta("InvitadoB");var(c,_)=await app.Cuenta("InvitadoC");using(a)using(b)using(c){
   string code=await app.Sala(a,max:2);
   var carrera=await Task.WhenAll(b.PostAsync($"/api/salas/{code}/unirse",null),c.PostAsync($"/api/salas/{code}/unirse",null));
   Assert.Single(carrera,x=>x.StatusCode==HttpStatusCode.OK);Assert.Single(carrera,x=>x.StatusCode==HttpStatusCode.Conflict);
   await Api.Es(await b.DeleteAsync($"/api/salas/{code}"),HttpStatusCode.Forbidden);
   await Api.Es(await a.DeleteAsync($"/api/salas/{code}"),HttpStatusCode.NoContent);
   await Api.Es(await a.GetAsync($"/api/salas/{code}"),HttpStatusCode.NotFound);
   await app.Datos(async db=>{Assert.Equal(0,await db.ParticipantesSala.CountAsync());Assert.Equal(0,await db.Salas.CountAsync());return true;});
  }
 }
 [Fact] public async Task CancelarPartidaActivaDa409YConHistorialConservaTodo()
 {
  using var app=new AplicacionTemporal();await app.Banco();var(a,_)=await app.Cuenta("CancelarA");var(b,_)=await app.Cuenta("CancelarB");using(a)using(b){
   string code=await app.Sala(a);await b.PostAsync($"/api/salas/{code}/unirse",null);foreach(var c in new[]{a,b})await c.PutAsJsonAsync($"/api/salas/{code}/preparado",new{preparado=true});
   await Api.Es(await a.PostAsync($"/api/partidas/{code}/iniciar",null),HttpStatusCode.OK);
   await Api.Es(await a.DeleteAsync($"/api/salas/{code}"),HttpStatusCode.Conflict);
   await app.Terminar(a);await Api.Es(await a.DeleteAsync($"/api/salas/{code}"),HttpStatusCode.NoContent);
   await app.Datos(async db=>{Assert.Equal(1,await db.Partidas.CountAsync());Assert.Equal(2,await db.PartidasJugadores.CountAsync());Assert.Equal(0,await db.ParticipantesSala.CountAsync());Assert.Equal(EstadoSala.Finalizada,(await db.Salas.SingleAsync()).Estado);return true;});
  }
 }
}

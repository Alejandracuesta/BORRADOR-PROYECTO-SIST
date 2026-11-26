namespace QuizArena.Tests;
public class PartidasTests
{
 [Fact] public async Task RespuestasSimultaneasDuplicadoRevanchaEHistorialPrivado()
 {
  using var app=new AplicacionTemporal();await app.Banco();var(a,_)=await app.Cuenta("PartidaA");var(b,_)=await app.Cuenta("PartidaB");var(ajeno,_)=await app.Cuenta("Ajeno");using(a)using(b)using(ajeno){
   string code=await app.Sala(a);await b.PostAsync($"/api/salas/{code}/unirse",null);foreach(var c in new[]{a,b})await c.PutAsJsonAsync($"/api/salas/{code}/preparado",new{preparado=true});await a.PostAsync($"/api/partidas/{code}/iniciar",null);
   await Api.Es(await a.PostAsync($"/api/salas/{code}/revancha",null),HttpStatusCode.Conflict);
   var state=await app.AbrirPregunta(a);int q=state["pregunta"]!["id"]!.GetValue<int>();
   Assert.Null(state["pregunta"]!["correcta"]);Assert.Null(state["pregunta"]!["respuestaCorrecta"]);
   var results=await Task.WhenAll(a.PostAsJsonAsync("/api/partidas/responder",new{preguntaId=q,opcion="A"}),b.PostAsJsonAsync("/api/partidas/responder",new{preguntaId=q,opcion="A"}));
   foreach(var response in results)await Api.Es(response,HttpStatusCode.OK);
   await Api.Es(await a.PostAsJsonAsync("/api/partidas/responder",new{preguntaId=q,opcion="B"}),HttpStatusCode.Conflict);
   await app.Terminar(a);
   var history=await a.GetFromJsonAsync<JsonArray>("/api/partidas/historial");Assert.Single(history!);int id=history![0]!["id"]!.GetValue<int>();
   var resumen=await Api.Json(await a.GetAsync($"/api/partidas/{id}/resumen"));Assert.Equal("Individual",resumen["modalidad"]!.GetValue<string>());Assert.Equal(2,resumen["participantes"]!.AsArray().Count);
   await Api.Es(await ajeno.GetAsync($"/api/partidas/{id}/resumen"),HttpStatusCode.Forbidden);Assert.Empty((await ajeno.GetFromJsonAsync<JsonArray>("/api/partidas/historial"))!);
   await Api.Es(await b.PostAsync($"/api/salas/{code}/revancha",null),HttpStatusCode.Forbidden);
   await Api.Es(await a.PostAsync($"/api/salas/{code}/revancha",null),HttpStatusCode.OK);
   var sala=await Api.Json(await a.GetAsync($"/api/salas/{code}"));Assert.All(sala["participantes"]!.AsArray(),p=>Assert.False(p!["preparado"]!.GetValue<bool>()));
   foreach(var c in new[]{a,b})await c.PutAsJsonAsync($"/api/salas/{code}/preparado",new{preparado=true});await a.PostAsync($"/api/partidas/{code}/iniciar",null);await app.Terminar(a);
   Assert.Equal(2,(await a.GetFromJsonAsync<JsonArray>("/api/partidas/historial"))!.Count);
   await app.Datos(async db=>{Assert.Equal(2,await db.RespuestasPartida.CountAsync());return true;});
   await Api.Es(await a.GetAsync("/api/clasificacion/individual"),HttpStatusCode.OK);
  }
 }
 [Theory] [InlineData(1,100,50)] [InlineData(2,200,100)] [InlineData(3,300,150)]
 public void PuntuacionPorRapidezSeConserva(int dificultad,int inicial,int minimo)
 {
  var d=(DificultadPregunta)dificultad;
  Assert.Equal(inicial,QuizArena.Servidor.Services.MotorPartidas.PuntosPorVelocidad(d,TimeSpan.Zero));
  Assert.Equal(minimo,QuizArena.Servidor.Services.MotorPartidas.PuntosPorVelocidad(d,TimeSpan.FromMinutes(1)));
 }
}

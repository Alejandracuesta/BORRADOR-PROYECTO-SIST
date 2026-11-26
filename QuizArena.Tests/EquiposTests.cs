namespace QuizArena.Tests;
public class EquiposTests
{
 [Theory] [InlineData(4)] [InlineData(6)]
 public async Task ConsensoDecisionIrreversibleYPlazos(int cantidad)
 {
  using var app=new AplicacionTemporal();await app.Banco();var clientes=new List<HttpClient>();
  try {
   for(int i=0;i<cantidad;i++)clientes.Add((await app.Cuenta($"Equipo{i}")).cliente);
   string code=await app.Sala(clientes[0],"Equipos",cantidad);
   foreach(var c in clientes.Skip(1))await c.PostAsync($"/api/salas/{code}/unirse",null);
   foreach(var c in clientes)await c.PutAsJsonAsync($"/api/salas/{code}/preparado",new{preparado=true});await Api.Es(await clientes[0].PostAsync($"/api/partidas/{code}/iniciar",null),HttpStatusCode.OK);
   var state=await app.AbrirPregunta(clientes[0]);int q=state["pregunta"]!["id"]!.GetValue<int>();
   await app.Datos(async db=>{var equipos=await db.EquiposPartida.Include(e=>e.Miembros).ToListAsync();Assert.All(equipos,e=>Assert.InRange(e.Miembros.Count,2,3));return true;});
   foreach(var c in clientes)await Api.Es(await c.PostAsJsonAsync("/api/partidas/equipos/votar",new{preguntaId=q,opcion="A"}),HttpStatusCode.OK);
   var confirmar=await Task.WhenAll(clientes.Select(c=>c.PostAsJsonAsync("/api/partidas/equipos/confirmar",new{preguntaId=q})));
   Assert.All(confirmar,response=>Assert.Contains(response.StatusCode,new[]{HttpStatusCode.OK,HttpStatusCode.Conflict}));
   await Api.Es(await clientes[0].PostAsJsonAsync("/api/partidas/equipos/votar",new{preguntaId=q,opcion="B"}),HttpStatusCode.Conflict);
   await app.Terminar(clientes[0]);await app.Datos(async db=>{Assert.Equal(2,await db.RespuestasEquipo.CountAsync());Assert.All(await db.EquiposPartida.ToListAsync(),e=>Assert.True(e.Puntos>0));return true;});
   Assert.True((await clientes[0].GetFromJsonAsync<JsonArray>("/api/partidas/historial"))![0]!["puntos"]!.GetValue<int>()>0);
   int id=(await clientes[0].GetFromJsonAsync<JsonArray>("/api/partidas/historial"))![0]!["id"]!.GetValue<int>();var resumen=await Api.Json(await clientes[0].GetAsync($"/api/partidas/{id}/resumen"));Assert.Equal("Equipos",resumen["modalidad"]!.GetValue<string>());Assert.Equal(2,resumen["equipos"]!.AsArray().Count);
   Assert.All(resumen["participantes"]!.AsArray(), j => Assert.True(j!["puntos"]!.GetValue<int>() > 0));
   await Api.Es(await clientes[0].PostAsync($"/api/salas/{code}/revancha",null),HttpStatusCode.OK);
   foreach(var c in clientes)await c.PutAsJsonAsync($"/api/salas/{code}/preparado",new{preparado=true});
   await Api.Es(await clientes[0].PostAsync($"/api/partidas/{code}/iniciar",null),HttpStatusCode.OK);
   var nueva=await Api.Json(await clientes[0].GetAsync("/api/partidas/actual"));
   Assert.All(nueva["marcadorEquipos"]!.AsArray(), e=>Assert.Equal(0,e!["puntos"]!.GetValue<int>()));
   await app.Terminar(clientes[0]);Assert.Equal(2,(await clientes[0].GetFromJsonAsync<JsonArray>("/api/partidas/historial"))!.Count);
   var antigua=await Api.Json(await clientes[0].GetAsync($"/api/partidas/{id}/resumen"));Assert.Equal(resumen.ToJsonString(),antigua.ToJsonString());
  }finally{foreach(var c in clientes)c.Dispose();}
 }
}

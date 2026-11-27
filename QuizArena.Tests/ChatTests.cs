using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Concurrent;
namespace QuizArena.Tests;
public class ChatTests
{
 public static async Task<HubConnection> Conectar(AplicacionTemporal app,HttpClient cliente,string nombre)
 {
  using var login=await cliente.PostAsJsonAsync("/api/acceso/entrar",new{nombre,clave=Api.Clave});await Api.Es(login,HttpStatusCode.OK);
  string cookie=string.Join("; ",login.Headers.GetValues("Set-Cookie").Select(x=>x.Split(';')[0]));
  var hub=new HubConnectionBuilder().WithUrl("http://localhost/hubs/salas",o=>{o.Transports=HttpTransportType.LongPolling;o.HttpMessageHandlerFactory=_=>app.Server.CreateHandler();o.Headers["Cookie"]=cookie;}).Build();await hub.StartAsync();return hub;
 }
 [Fact] public async Task EdicionPropiaBorradoYNotificacionesPrivadas()
 {
  using var app=new AplicacionTemporal();await app.Banco();var clientes=new List<HttpClient>();var hubs=new List<HubConnection>();
  try{
   for(int i=0;i<4;i++)clientes.Add((await app.Cuenta($"Chat{i}")).cliente);
   string code=await app.Sala(clientes[0],"Equipos");foreach(var c in clientes.Skip(1))await c.PostAsync($"/api/salas/{code}/unirse",null);foreach(var c in clientes)await c.PutAsJsonAsync($"/api/salas/{code}/preparado",new{preparado=true});await clientes[0].PostAsync($"/api/partidas/{code}/iniciar",null);
   var avisos=Enumerable.Range(0,4).Select(_=>new ConcurrentQueue<string>()).ToArray();var reacciones=Enumerable.Range(0,4).Select(_=>new ConcurrentQueue<JsonObject>()).ToArray();
   for(int i=0;i<4;i++){int k=i;var hub=await Conectar(app,clientes[i],$"Chat{i}");hubs.Add(hub);hub.On<string>("ChatActualizado",codigo=>avisos[k].Enqueue(codigo));hub.On<JsonObject>("ReaccionRecibida",r=>reacciones[k].Enqueue(r));await hub.InvokeAsync("EntrarSala",code);}
   await hubs[0].InvokeAsync("EnviarMensaje",code,"Texto privado original",true);
   var historial=(await clientes[0].GetFromJsonAsync<JsonArray>($"/api/chat/{code}"))!;int id=historial[0]!["id"]!.GetValue<int>();Assert.Empty((await clientes[1].GetFromJsonAsync<JsonArray>($"/api/chat/{code}"))!);
   await Api.Es(await clientes[2].PutAsJsonAsync($"/api/chat/mensajes/{id}",new{texto="No es mío"}),HttpStatusCode.Forbidden);
   await Api.Es(await clientes[1].DeleteAsync($"/api/chat/mensajes/{id}"),HttpStatusCode.Forbidden);
   await Api.Es(await clientes[0].PutAsJsonAsync($"/api/chat/mensajes/{id}",new{texto="   "}),HttpStatusCode.BadRequest);
   await Api.Es(await clientes[0].PutAsJsonAsync($"/api/chat/mensajes/{id}",new{texto="Texto corregido"}),HttpStatusCode.NoContent);
   await Esperar(()=>avisos[0].Count==1&&avisos[2].Count==1);Assert.Empty(avisos[1]);Assert.Empty(avisos[3]);
   Assert.Equal("Texto corregido",(await clientes[2].GetFromJsonAsync<JsonArray>($"/api/chat/{code}"))![0]!["texto"]!.GetValue<string>());
   await Api.Es(await clientes[0].DeleteAsync($"/api/chat/mensajes/{id}"),HttpStatusCode.NoContent);
   await Api.Es(await clientes[0].PutAsJsonAsync($"/api/chat/mensajes/{id}",new{texto="Resucitar"}),HttpStatusCode.Conflict);
   Assert.True((await clientes[2].GetFromJsonAsync<JsonArray>($"/api/chat/{code}"))![0]!["eliminado"]!.GetValue<bool>());
   await hubs[0].InvokeAsync("EnviarReaccion",code,"Aplauso");await hubs[0].InvokeAsync("EnviarReaccion",code,"Aplauso");await Esperar(()=>reacciones[0].Count==2&&reacciones[2].Count==2);Assert.Empty(reacciones[1]);Assert.Empty(reacciones[3]);
   await Api.Es(await clientes[0].PutAsJsonAsync("/api/chat/mensajes/999999",new{texto="No existe"}),HttpStatusCode.NotFound);
   await app.Datos(async db=>{var m=await db.MensajesSala.SingleAsync();Assert.Equal("[Mensaje eliminado]",m.Texto);Assert.NotNull(m.EditadoUtc);return true;});
  }finally{foreach(var hub in hubs)await hub.DisposeAsync();foreach(var c in clientes)c.Dispose();}
 }
 [Fact] public async Task HubRechazaCuentaDesactivadaYGrupoAjeno()
 {
  using var app=new AplicacionTemporal();await app.Banco();var(a,id)=await app.Cuenta("Conexion");var(b,_)=await app.Cuenta("Fuera");using(a)using(b){
   string code=await app.Sala(a);await using var hub=await Conectar(app,b,"Fuera");await Assert.ThrowsAsync<Microsoft.AspNetCore.SignalR.HubException>(()=>hub.InvokeAsync("EntrarSala",code));
   await app.Datos(async db=>{(await db.Jugadores.SingleAsync(j=>j.Nombre=="Fuera")).Activo=false;await db.SaveChangesAsync();return true;});
   await Assert.ThrowsAnyAsync<Exception>(()=>hub.InvokeAsync("EnviarReaccion",code,"Aplauso"));
  }
 }
 private static async Task Esperar(Func<bool> condicion){for(int i=0;i<100&&!condicion();i++)await Task.Delay(20);Assert.True(condicion());}
}

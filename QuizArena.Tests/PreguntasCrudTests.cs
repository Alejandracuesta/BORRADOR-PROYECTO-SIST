using QuizArena.Servidor.Data;
namespace QuizArena.Tests;
public class PreguntasCrudTests
{
 [Fact] public async Task CrudSinFiltrarSolucionesNiAlterarHistorial()
 {
  using var app=new AplicacionTemporal(); int cat=await app.Banco(); var(admin,_)=await app.Cuenta("AdminPreg",true); var(normal,_)=await app.Cuenta("NormalPreg"); using(admin) using(normal) {
   await Api.Es(await normal.GetAsync("/api/preguntas/admin"),HttpStatusCode.Forbidden);
   await Api.Es(await normal.PostAsJsonAsync("/api/preguntas",Api.Pregunta(cat)),HttpStatusCode.Forbidden);
   int id=(await Api.Json(await admin.PostAsJsonAsync("/api/preguntas",Api.Pregunta(cat)),HttpStatusCode.Created))["id"]!.GetValue<int>();
   await Api.Es(await admin.PostAsJsonAsync("/api/preguntas",Api.Pregunta(cat)),HttpStatusCode.Conflict);
   await Api.Es(await admin.PostAsJsonAsync("/api/preguntas",Api.Pregunta(cat,"¿Son iguales las opciones?","Uno")),HttpStatusCode.BadRequest);
   await Api.Es(await admin.PostAsJsonAsync("/api/preguntas",Api.Pregunta(99999)),HttpStatusCode.BadRequest);
   await Api.Es(await admin.PutAsJsonAsync($"/api/preguntas/{id}",Api.Pregunta(cat,"¿Enunciado corregido de prueba?")),HttpStatusCode.NoContent);
   var publica=await Api.Json(await normal.GetAsync($"/api/preguntas/{id}")); Assert.Null(publica["correcta"]); Assert.Null(publica["respuestaCorrecta"]);
   Assert.Equal("A",(await Api.Json(await admin.GetAsync($"/api/preguntas/admin/{id}")))["correcta"]!.GetValue<string>());
   await Api.Es(await admin.DeleteAsync($"/api/preguntas/{id}"),HttpStatusCode.NoContent);
   await Api.Es(await normal.GetAsync($"/api/preguntas/{id}"),HttpStatusCode.NotFound);
   Assert.False((await Api.Json(await admin.GetAsync($"/api/preguntas/admin/{id}")))["activa"]!.GetValue<bool>());
   await Api.Es(await admin.PutAsync($"/api/preguntas/{id}/activar",null),HttpStatusCode.NoContent);
   string code=await app.Sala(normal); await Api.Es(await admin.PostAsync($"/api/salas/{code}/unirse",null),HttpStatusCode.OK);
   foreach(var c in new[]{normal,admin}) await c.PutAsJsonAsync($"/api/salas/{code}/preparado",new{preparado=true});
   await Api.Es(await normal.PostAsync($"/api/partidas/{code}/iniciar",null),HttpStatusCode.OK);
   int used=await app.Datos(db=>db.PartidasPreguntas.Select(p=>p.PreguntaId).FirstAsync());
   await Api.Es(await admin.PutAsJsonAsync($"/api/preguntas/{used}",Api.Pregunta(cat,"¿Intento de cambiar el historial?")),HttpStatusCode.Conflict);
   await Api.Es(await admin.DeleteAsync($"/api/preguntas/{used}"),HttpStatusCode.NoContent);
   await app.Datos(async db=>{Assert.True(await db.PartidasPreguntas.AnyAsync(p=>p.PreguntaId==used));return true;});
  }
 }
 [Fact] public async Task ImportadorComparteReglasYEsAtomico()
 {
  using var app=new AplicacionTemporal(); using var c=app.Cliente();
  string archivo=Path.Combine(app.Directorio,"banco.json");
  await File.WriteAllTextAsync(archivo,"""[{"categoria":"Temporal","enunciado":"¿Pregunta importada correcta?","dificultad":"Media","opcionA":"Uno","opcionB":"Dos","opcionC":"Tres","opcionD":"Cuatro","correcta":"A","fuente":"Prueba","licencia":"Propias"}]""");
  await app.Datos(async db=>{await ImportadorPreguntas.ImportarAsync(db,archivo);await ImportadorPreguntas.ImportarAsync(db,archivo);Assert.Equal(1,await db.Preguntas.CountAsync());return true;});
  await File.WriteAllTextAsync(archivo,(await File.ReadAllTextAsync(archivo)).Replace("\"Dos\"","\"Uno\""));
  await app.Datos(async db=>{await Assert.ThrowsAsync<InvalidDataException>(()=>ImportadorPreguntas.ImportarAsync(db,archivo));Assert.Equal(1,await db.Preguntas.CountAsync());return true;});
 }
}

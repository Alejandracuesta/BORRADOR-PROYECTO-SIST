namespace QuizArena.Tests;
public class CategoriasCrudTests
{
 [Fact] public async Task CrudPermisosDuplicadosYReferencias()
 {
  using var app=new AplicacionTemporal(); var(admin,_)=await app.Cuenta("AdminCat",true); var(normal,_)=await app.Cuenta("NormalCat"); using(admin) using(normal) {
   await Api.Es(await normal.PostAsJsonAsync("/api/categorias",new{nombre="Categoría"}),HttpStatusCode.Forbidden);
   await Api.Es(await admin.PostAsJsonAsync("/api/categorias",new{nombre="  "}),HttpStatusCode.BadRequest);
   int id=(await Api.Json(await admin.PostAsJsonAsync("/api/categorias",new{nombre="Categoría"}),HttpStatusCode.Created))["id"]!.GetValue<int>();
   await Api.Es(await admin.GetAsync($"/api/categorias/{id}"),HttpStatusCode.OK);
   await Api.Es(await admin.PostAsJsonAsync("/api/categorias",new{nombre=" categoría "}),HttpStatusCode.Conflict);
   await Api.Es(await admin.PutAsJsonAsync($"/api/categorias/{id}",new{nombre="Renombrada"}),HttpStatusCode.OK);
   await Api.Es(await admin.DeleteAsync($"/api/categorias/{id}"),HttpStatusCode.NoContent);
   await Api.Es(await admin.GetAsync($"/api/categorias/{id}"),HttpStatusCode.NotFound);
   int conPreguntas=await app.Banco(); await Api.Es(await admin.DeleteAsync($"/api/categorias/{conPreguntas}"),HttpStatusCode.Conflict);
   var carrera=await Task.WhenAll(Enumerable.Range(0,4).Select(_=>admin.PostAsJsonAsync("/api/categorias",new{nombre="Simultánea"})));
   Assert.Single(carrera,r=>r.StatusCode==HttpStatusCode.Created); Assert.Equal(3,carrera.Count(r=>r.StatusCode==HttpStatusCode.Conflict));
  }
 }
}

namespace QuizArena.Cliente.Models;
public record CategoriaAdminApi(int Id, string Nombre, int TotalPreguntas);
public sealed class PreguntaAdminApi
{
 public int Id { get; set; } public int CategoriaId { get; set; } public int Dificultad { get; set; }
 public string Enunciado { get; set; } = "";
 public string OpcionA { get; set; } = ""; public string OpcionB { get; set; } = "";
 public string OpcionC { get; set; } = ""; public string OpcionD { get; set; } = "";
 public string Correcta { get; set; } = ""; public string Fuente { get; set; } = ""; public string Licencia { get; set; } = "";
 public bool Activa { get; set; }
}
public record HistorialApi(int Id, string CodigoSala, string NombreSala, DateTime CreadaUtc, string Modalidad, int Puntos, int Aciertos);
public record JugadorResultadoApi(int JugadorId, string Nombre, int Puntos, int Aciertos, int? EquipoId);
public record EquipoResultadoApi(int Id, string Nombre, int Puntos, int Aciertos, List<JugadorResultadoApi> Integrantes);
public record ResumenApi(int Id, string CodigoSala, DateTime CreadaUtc, string Modalidad, string ResultadoFinal,
 List<JugadorResultadoApi> Participantes, List<EquipoResultadoApi> Equipos);

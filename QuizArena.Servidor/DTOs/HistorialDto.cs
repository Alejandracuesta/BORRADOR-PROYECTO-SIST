namespace QuizArena.Servidor.DTOs;
public record HistorialPartidaDto(int Id, string CodigoSala, string NombreSala, DateTime CreadaUtc,
    string Modalidad, int Puntos, int Aciertos);
public record ResultadoJugadorDto(int JugadorId, string Nombre, int Puntos, int Aciertos, int? EquipoId);
public record ResultadoEquipoHistoricoDto(int Id, string Nombre, int Puntos, int Aciertos, List<ResultadoJugadorDto> Integrantes);
public record ResumenPartidaDto(int Id, string CodigoSala, DateTime CreadaUtc, string Modalidad,
    string ResultadoFinal, List<ResultadoJugadorDto> Participantes, List<ResultadoEquipoHistoricoDto> Equipos);

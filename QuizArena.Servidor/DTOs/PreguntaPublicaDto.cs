namespace QuizArena.Servidor.DTOs;

public record PreguntaPublicaDto(
    int Id,
    string Enunciado,
    int CategoriaId,
    string Categoria,
    string Dificultad,
    string OpcionA,
    string OpcionB,
    string OpcionC,
    string OpcionD);

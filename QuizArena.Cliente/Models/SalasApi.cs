namespace QuizArena.Cliente.Models;

public record CategoriaApi(
    int Id,
    string Nombre,
    int TotalPreguntas)
{
    public override string ToString() => Nombre;
}

public record SalaPublicaApi(
    string Codigo,
    string Nombre,
    string Modalidad,
    int Ocupacion,
    int MaxJugadores,
    int NumeroPreguntas,
    string? Categoria,
    string? Dificultad)
{
    public override string ToString()
        => $"{Nombre} — {Ocupacion}/{MaxJugadores} — {Modalidad}";
}

public record ParticipanteSalaApi(
    int JugadorId,
    string Nombre,
    bool Preparado,
    bool EsAnfitrion);

public record SalaDetalleApi(
    string Codigo,
    string Nombre,
    bool EsPrivada,
    string Modalidad,
    string Estado,
    int MaxJugadores,
    int NumeroPreguntas,
    int? CategoriaId,
    string? Categoria,
    string? Dificultad,
    int CreadorId,
    List<ParticipanteSalaApi> Participantes);

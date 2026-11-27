using QuizArena.Servidor.DTOs;
using QuizArena.Servidor.Models;
namespace QuizArena.Servidor.Services;
public static class ValidadorPreguntas
{
public static string? Validar(
    PreguntaGuardarDto datos,
    out DificultadPregunta dificultad)
{
    dificultad = default;

    string enunciado = (datos.Enunciado ?? "").Trim();

    if (datos.CategoriaId <= 0)
        return "Categoría inválida";

    if (enunciado.Length is < 10 or > 300)
        return "El enunciado debe tener entre 10 y 300 caracteres";

    string[] opciones =
    {
        (datos.OpcionA ?? "").Trim(),
        (datos.OpcionB ?? "").Trim(),
        (datos.OpcionC ?? "").Trim(),
        (datos.OpcionD ?? "").Trim()
    };

    if (opciones.Any(o => o.Length is < 1 or > 150))
        return "Las opciones deben tener entre 1 y 150 caracteres";

    if (opciones.Distinct(
        StringComparer.OrdinalIgnoreCase).Count() != 4)
    {
        return "Las cuatro opciones deben ser distintas";
    }

    if (!Enum.TryParse(
        datos.Dificultad?.Trim(),
        true,
        out dificultad) ||
        !Enum.IsDefined(dificultad))
    {
        return "Dificultad inválida";
    }

    string correcta =
        (datos.Correcta ?? "").Trim().ToUpperInvariant();

    if (correcta is not ("A" or "B" or "C" or "D"))
        return "La respuesta correcta debe ser A, B, C o D";

    if ((datos.Fuente ?? "").Trim().Length is < 2 or > 100)
        return "Fuente inválida";

    if ((datos.Licencia ?? "").Trim().Length is < 2 or > 50)
        return "Licencia inválida";

    return null;
}
}

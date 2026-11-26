using System.Text.Json;
namespace QuizArena.Cliente.Services;
public static class RespuestaApi
{
 public static async Task ComprobarAsync(HttpResponseMessage respuesta, CancellationToken ct = default)
 {
  if (respuesta.IsSuccessStatusCode) return;
  string texto = await respuesta.Content.ReadAsStringAsync(ct);
  try { using var doc = JsonDocument.Parse(texto);
   if (doc.RootElement.TryGetProperty("mensaje", out var mensaje)) texto = mensaje.GetString() ?? texto;
   else if (doc.RootElement.TryGetProperty("errors", out var errores)) texto = string.Join("; ", errores.EnumerateObject().SelectMany(p => p.Value.EnumerateArray()).Select(e => e.GetString()));
  } catch (JsonException) { }
  throw new HttpRequestException($"HTTP {(int)respuesta.StatusCode}: {texto}", null, respuesta.StatusCode);
 }
}

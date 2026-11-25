namespace QuizArena.Servidor.Services;

public sealed class ControlFrecuencia
{
    private readonly object candado = new();

    private readonly Dictionary<(int, string), DateTime>
        ultimasAcciones = new();

    public bool Permitir(
        int jugadorId, string accion, TimeSpan intervalo)
    {
        lock (candado)
        {
            DateTime ahora = DateTime.UtcNow;
            var clave = (jugadorId, accion);

            if (ultimasAcciones.TryGetValue(
                clave, out DateTime anterior) &&
                ahora - anterior < intervalo)
            {
                return false;
            }

            ultimasAcciones[clave] = ahora;
            return true;
        }
    }
}

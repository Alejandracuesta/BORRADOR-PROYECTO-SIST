namespace QuizArena.Servidor.Services;

public sealed class CoordinadorSalas
{
    public SemaphoreSlim Semaforo { get; } = new(1, 1);
}

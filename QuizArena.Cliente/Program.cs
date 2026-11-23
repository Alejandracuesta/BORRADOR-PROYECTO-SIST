namespace QuizArena.Cliente;

static class Program
{
    /// <summary>
    /// Punto de entrada del cliente Windows Forms.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }    
}

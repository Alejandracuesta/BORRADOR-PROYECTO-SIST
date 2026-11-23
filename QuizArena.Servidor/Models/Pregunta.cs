namespace QuizArena.Servidor.Models;

public class Pregunta
{
    public int Id { get; set; }

    public string Enunciado { get; set; } = "";

    public string EnunciadoNormalizado { get; set; } = "";

    public int CategoriaId { get; set; }

    public Categoria Categoria { get; set; } = null!;

    public DificultadPregunta Dificultad { get; set; }

    public string OpcionA { get; set; } = "";

    public string OpcionB { get; set; } = "";

    public string OpcionC { get; set; } = "";

    public string OpcionD { get; set; } = "";

    public string RespuestaCorrecta { get; set; } = "";

    public string Fuente { get; set; } = "";

    public string Licencia { get; set; } = "";

    public bool Activa { get; set; } = true;
}

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuizArena.Servidor.DTOs;
using QuizArena.Servidor.Models;
using QuizArena.Servidor.Services;

namespace QuizArena.Servidor.Data;

public static class ImportadorPreguntas
{
    public static async Task ImportarAsync(
        QuizArenaDbContext db, string ruta,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(ruta))
            throw new FileNotFoundException(
                "No se encuentra el archivo de preguntas.", ruta);

        await using var archivo = File.OpenRead(ruta);

        List<PreguntaImportacionDto?>? datos;

        try
        {
            datos = await JsonSerializer.DeserializeAsync<List<PreguntaImportacionDto?>>(
                archivo,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                },
                cancellationToken);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException(
                "El archivo de preguntas no contiene un JSON válido.", ex);
        }

        if (datos is null || datos.Count == 0)
            throw new InvalidDataException(
                "El archivo no contiene preguntas.");

        var categorias = await db.Categorias.ToListAsync(cancellationToken);

        var enunciados = (await db.Preguntas
            .Select(p => p.EnunciadoNormalizado)
            .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.Ordinal);

        // Preparar todo en memoria: un registro inválido no deja altas pendientes
        // en el contexto ni guarda parcialmente el banco.
        var categoriasNuevas = new List<Categoria>();
        var preguntasNuevas = new List<Pregunta>();
        int repetidas = 0;

        for (int i = 0; i < datos.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dato = datos[i];

            if (dato is null)
                throw new InvalidDataException(
                    $"La pregunta {i + 1} es nula.");

            string categoriaNombre = (dato.Categoria ?? "").Trim();
            string enunciado = (dato.Enunciado ?? "").Trim();
            string dificultadTexto = (dato.Dificultad ?? "").Trim();
            string correcta = (dato.Correcta ?? "").Trim().ToUpperInvariant();
            string fuente = (dato.Fuente ?? "").Trim();
            string licencia = (dato.Licencia ?? "").Trim();

            string[] opciones =
            {
                (dato.OpcionA ?? "").Trim(),
                (dato.OpcionB ?? "").Trim(),
                (dato.OpcionC ?? "").Trim(),
                (dato.OpcionD ?? "").Trim()
            };

            if (categoriaNombre.Length is < 3 or > 60)
                throw new InvalidDataException(
                    $"Categoría inválida en pregunta {i + 1}.");

            var guardar = new PreguntaGuardarDto { CategoriaId = 1,
                Enunciado = enunciado, Dificultad = dificultadTexto,
                OpcionA = opciones[0], OpcionB = opciones[1], OpcionC = opciones[2], OpcionD = opciones[3],
                Correcta = correcta, Fuente = fuente, Licencia = licencia };
            string? error = ValidadorPreguntas.Validar(guardar, out var dificultad);
            if (error != null) throw new InvalidDataException($"Pregunta {i + 1}: {error}");

            string clavePregunta = enunciado.ToUpperInvariant();

            // Detectar también enunciados repetidos dentro del propio archivo.
            if (!enunciados.Add(clavePregunta))
            {
                repetidas++;
                continue;
            }

            string claveCategoria = categoriaNombre.ToUpperInvariant();

            var categoria = categorias.FirstOrDefault(
                c => c.NombreNormalizado == claveCategoria);

            if (categoria is null)
            {
                categoria = new Categoria
                {
                    Nombre = categoriaNombre,
                    NombreNormalizado = claveCategoria
                };

                categorias.Add(categoria);
                categoriasNuevas.Add(categoria);
            }

            preguntasNuevas.Add(new Pregunta
            {
                Categoria = categoria,
                Enunciado = enunciado,
                EnunciadoNormalizado = clavePregunta,
                Dificultad = dificultad,
                OpcionA = opciones[0],
                OpcionB = opciones[1],
                OpcionC = opciones[2],
                OpcionD = opciones[3],
                RespuestaCorrecta = correcta,
                Fuente = fuente,
                Licencia = licencia,
                Activa = true
            });
        }

        db.Categorias.AddRange(categoriasNuevas);
        db.Preguntas.AddRange(preguntasNuevas);

        // Una única operación transaccional de EF Core para el banco completo.
        await db.SaveChangesAsync(cancellationToken);

        Console.WriteLine($"Preguntas nuevas: {preguntasNuevas.Count}");
        Console.WriteLine($"Preguntas ya existentes: {repetidas}");
        Console.WriteLine("Importación terminada.");
    }
}

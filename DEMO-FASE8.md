# Demostración de Quiz Arena — diez minutos

## Preparación

1. Instalar .NET 10 y Visual Studio 2026 con desarrollo de escritorio .NET y ASP.NET Core.
2. Restaurar y compilar `QuizArena.sln`; ejecutar `dotnet test QuizArena.sln`.
3. Crear o importar la base siguiendo README. Para ensayos de borrado, utilizar
   `BaseDatos__Ruta` con una base separada: nunca los datos personales del profesor.
4. Registrar cuentas de demostración con contraseñas propias. Con el servidor
   detenido, habilitar una con `--hacer-admin=NOMBRE`.
5. Iniciar el servidor con el perfil `http` y cuatro procesos WinForms.
6. Abrir `http://localhost:5162/swagger/index.html`. Sus recursos necesitan Internet.

## Secuencia

| Tiempo | Acciones y explicación |
| --- | --- |
| 0–2 minutos | Mostrar servidor, cliente y pruebas; Swagger y OpenAPI. REST valida operaciones, SQLite conserva estado y SignalR avisa de los cambios. |
| 2–4 minutos | Registrar una cuenta y abrir Mi cuenta. Crear categoría y pregunta con administrador; demostrar 403 con usuario normal, 409 por duplicado y baja lógica de pregunta. |
| 4–5 minutos | Explicar cookies independientes, validación de cuenta activa y rol comprobado en servidor. Mostrar que las soluciones no aparecen en GET públicos. |
| 5–7 minutos | Dos clientes en sala individual, un solo inicio, pregunta y reloj, respuesta bloqueada tras enviarse, marcador y podio. |
| 7–9 minutos | Cuatro clientes en equipos; votos distintos, cambio hasta unanimidad, confirmación irreversible, chat privado, edición de mensaje propio y reacciones consecutivas. |
| 9–10 minutos | Revancha e historial independiente; clasificación; enseñar resultados xUnit de concurrencia y recuperación. |

## Datos que conviene tener preparados

- Al menos cinco preguntas activas para la dificultad y categoría elegidas.
- Una pregunta de prueba sin historial para poder editarla y después desactivarla.
- Una categoría vacía para demostrar DELETE 204; otra referenciada para demostrar 409.
- Una cuenta de ensayo fuera de cualquier sala para demostrar su baja sin tocar cuentas reales.
- Dos partidas finalizadas en la misma sala para mostrar que la revancha no sobrescribe resultados.

Las rondas completas pueden superar el tiempo asignado a la exposición. Preparar
un resultado previo y enseñar el flujo de una ronda en vivo; el historial y las
pruebas automáticas permiten demostrar el ciclo completo sin acelerar el reloj
oficial ni modificar puntos manualmente.

## Respuestas clave

- DELETE no siempre borra la fila: cuentas y preguntas se desactivan; mensajes
  pierden su contenido; partidas finalizadas y respuestas definitivas se conservan.
- Un administrador no puede editar una pregunta usada: debe crear una nueva versión.
- La pertenencia a salas, el rol y el autor del mensaje se verifican en el servidor.
- `SemaphoreSlim` coordina operaciones en la única instancia del servidor. SQLite
  e índices únicos mantienen las relaciones y resuelven duplicados.
- SignalR no sustituye a REST ni a SQLite. Al reconectar, el cliente recupera el
  estado oficial; los plazos no se pausan al detener el servidor.

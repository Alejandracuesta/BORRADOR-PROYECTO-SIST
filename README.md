# BORRADOR-PROYECTO-SIST

## Quiz Arena — fase 2.1: SQLite y jugadores

Solución C# con una API REST, una única base de datos SQLite y un cliente WinForms.
Se pueden abrir varias instancias independientes del cliente en el mismo ordenador;
todas utilizan el servidor en `http://localhost:5080`.

```text
QuizArena.sln
├── QuizArena.Servidor   ASP.NET Core Web API (controladores)
│   ├── Program.cs
│   ├── Controllers/EstadoController.cs
│   ├── Controllers/JugadoresController.cs
│   ├── Data/QuizArenaDbContext.cs
│   ├── Models/Jugador.cs
│   ├── DTOs/CrearJugadorDto.cs
│   ├── DTOs/JugadorRespuestaDto.cs
│   ├── Migrations/
│   └── Properties/launchSettings.json
└── QuizArena.Cliente    Windows Forms
    ├── Program.cs
    ├── Form1.cs
    ├── Form1.Designer.cs
    └── Form1.resx
```

Se utiliza **.NET 10**, disponible en el equipo junto con Visual Studio 2026.
El servidor utiliza `Microsoft.EntityFrameworkCore.Sqlite` y
`Microsoft.EntityFrameworkCore.Tools`, ambos en la versión **10.0.14**.
El cliente conserva el código de la fase 1.

### Relación entre los proyectos

Ambos pertenecen a la misma solución. El cliente consume el servidor con
`HttpClient`: `GET http://localhost:5080/api/estado`. No existe una referencia de
proyecto entre ellos, porque son ejecutables independientes y se comunican por
la API REST.

`EstadoController` hereda de `ControllerBase`; `Form1` hereda de `Form` y su clase
parcial del diseñador define `btnComprobar` y `lblEstado`. El botón utiliza una
petición asíncrona, muestra el JSON recibido y gestiona los errores de conexión
y el tiempo de espera.

En el servidor, `JugadoresController` recibe un `QuizArenaDbContext` mediante
inyección de dependencias. El contexto hereda de `DbContext` y expone
`DbSet<Jugador>` para la tabla `Jugadores`. `CrearJugadorDto` valida los datos
entrantes y `JugadorRespuestaDto` define los campos públicos de la respuesta.
El nombre normalizado se guarda en mayúsculas, tras eliminar espacios al inicio
y al final, y tiene un índice único en SQLite para impedir duplicados incluso
entre peticiones simultáneas. Las fechas de registro se devuelven en UTC.

### Preparar la base de datos desde un clon nuevo

1. Abrir `QuizArena.sln` y restaurar los paquetes NuGet.
2. Abrir **Herramientas → Administrador de paquetes NuGet → Consola del
   Administrador de paquetes** en Visual Studio.
3. Aplicar la migración existente indicando explícitamente el proyecto servidor:

```powershell
Update-Database -Project QuizArena.Servidor -StartupProject QuizArena.Servidor
```

La migración **CreacionInicial** ya está generada y guardada en `Migrations`.
No hay que volver a ejecutar `Add-Migration CreacionInicial` en otro clon.
Para cambios futuros del modelo, se generará una nueva migración con otro nombre.

El comando crea `QuizArena.Servidor/quizarena.db`, según el `ContentRootPath`
del servidor. En este equipo la ruta es:

```text
C:\Users\Juan\Desktop\SIST\QuizArena.Servidor\quizarena.db
```

La base de datos y sus archivos auxiliares se excluyen de Git; las migraciones
sí forman parte del repositorio. El servidor usa las migraciones aplicadas y no
modifica automáticamente el esquema al arrancar. Todos los clientes acceden a
los mismos datos mediante la API.

### Ejecutar desde Visual Studio

1. Abrir `QuizArena.sln` con Visual Studio 2026.
2. Seleccionar el perfil **Quiz Arena - Servidor y cliente** en la barra de inicio.
   El perfil compartido se guarda en `QuizArena.slnLaunch`.
3. Pulsar **F5**. Se iniciarán el servidor y el formulario.
4. Pulsar **Comprobar conexión**: aparecerá el JSON y el estado
   **Servidor conectado**.

Si el perfil no aparece, abrir las propiedades de la solución → configurar
proyectos de inicio → varios proyectos de inicio y establecer **Iniciar** para
el servidor y el cliente, en ese orden.

### Ejecutar desde dos terminales

Desde la raíz del repositorio, en la primera terminal:

```powershell
dotnet run --project QuizArena.Servidor --launch-profile http
```

En la segunda:

```powershell
dotnet run --project QuizArena.Cliente
```

Para compilar ambos proyectos:

```powershell
dotnet build QuizArena.sln
```

### API disponible

| Método | Ruta | Resultado |
| --- | --- | --- |
| GET | `/api/estado` | HTTP 200 con el estado del servidor |
| GET | `/api/jugadores` | HTTP 200 con todos los perfiles, ordenados por Id |
| GET | `/api/jugadores/{id}` | HTTP 200 con el perfil o HTTP 404 si no existe |
| POST | `/api/jugadores` | HTTP 201 y cabecera Location al registrar un perfil |
| POST | `/api/jugadores` | HTTP 409 si el nombre ya existe; HTTP 400 si no es válido |

El nombre es obligatorio y debe tener entre 3 y 20 caracteres. La respuesta de
un perfil contiene únicamente `id`, `nombre` y `fechaRegistroUtc`.

Ejemplo de registro desde PowerShell, con el servidor iniciado:

```powershell
Invoke-RestMethod -Uri 'http://localhost:5080/api/jugadores' -Method Post -ContentType 'application/json' -Body '{"nombre":"Carlos"}'
```

Consulta del listado y del primer perfil:

```powershell
Invoke-RestMethod -Uri 'http://localhost:5080/api/jugadores'
Invoke-RestMethod -Uri 'http://localhost:5080/api/jugadores/1'
```

Volver a registrar `CARLOS` o ` Carlos ` devuelve HTTP 409. Al reiniciar el
servidor, los perfiles siguen disponibles.

### Comprobaciones de la fase 1

- La solución y los dos proyectos compilan sin errores.
- `GET http://localhost:5080/api/estado` devuelve HTTP 200 y JSON con
  `aplicacion`, `estado` y `fechaServidor` en UTC.
- El botón muestra la respuesta y **Servidor conectado**.
- Con el servidor detenido, el botón muestra **Sin conexión** y un mensaje
  controlado, y vuelve a estar habilitado para reintentar.
- Si el servidor no responde, el cliente controla el tiempo de espera.

### Validación de la fase 2.1

- Paquetes EF Core 10.0.14 restaurados y solución compilada sin errores.
- Migración `CreacionInicial` generada y aplicada a SQLite.
- Registro con HTTP 201, Id generado y cabecera Location válida.
- Listado ordenado, consulta individual HTTP 200 y jugador inexistente HTTP 404.
- Nombres inválidos rechazados con HTTP 400 y duplicados con HTTP 409.
- Prueba concurrente en una base aislada: una sola alta y siete conflictos
  para ocho peticiones de nombres equivalentes.
- Persistencia de los perfiles tras detener y volver a iniciar el servidor.

La base local de desarrollo contiene los perfiles de ejemplo **Carlos, Lucia
y Pablo**. Estos datos no se incluyen en la migración ni se guardan en Git.
En otro clon la tabla empezará vacía después de aplicar `Update-Database`.

El puerto HTTP 5080 es para esta práctica local. Esta fase registra perfiles;
la pantalla de acceso, autenticación, preguntas, salas, partidas y SignalR se
incorporarán en fases posteriores.

La aplicación de las migraciones sigue la
[documentación de EF Core para Visual Studio](https://learn.microsoft.com/en-us/ef/core/cli/powershell).

El formato del perfil compartido sigue la
[documentación de Microsoft](https://learn.microsoft.com/es-es/visualstudio/ide/how-to-set-multiple-startup-projects?view=visualstudio).

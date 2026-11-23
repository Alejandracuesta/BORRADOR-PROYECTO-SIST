# BORRADOR-PROYECTO-SIST

## Quiz Arena — fase 1

Solución C# con dos aplicaciones independientes que se comunican mediante HTTP:

```text
QuizArena.sln
├── QuizArena.Servidor   ASP.NET Core Web API (controladores)
│   ├── Program.cs
│   ├── Controllers/EstadoController.cs
│   └── Properties/launchSettings.json
└── QuizArena.Cliente    Windows Forms
    ├── Program.cs
    ├── Form1.cs
    ├── Form1.Designer.cs
    └── Form1.resx
```

Se utiliza **.NET 10**, disponible en el equipo junto con Visual Studio 2026.
No se necesitan paquetes NuGet externos.

### Relación entre los proyectos

Ambos pertenecen a la misma solución. El cliente consume el servidor con
`HttpClient`: `GET http://localhost:5080/api/estado`. No existe una referencia de
proyecto entre ellos, porque son ejecutables independientes y se comunican por
la API REST.

`EstadoController` hereda de `ControllerBase`; `Form1` hereda de `Form` y su clase
parcial del diseñador define `btnComprobar` y `lblEstado`. El botón utiliza una
petición asíncrona, muestra el JSON recibido y gestiona los errores de conexión
y el tiempo de espera.

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

### Comprobar la fase 1

- La solución y los dos proyectos compilan sin errores.
- `GET http://localhost:5080/api/estado` devuelve HTTP 200 y JSON con
  `aplicacion`, `estado` y `fechaServidor` en UTC.
- El botón muestra la respuesta y **Servidor conectado**.
- Con el servidor detenido, el botón muestra **Sin conexión** y un mensaje
  controlado, y vuelve a estar habilitado para reintentar.
- Si el servidor no responde, el cliente controla el tiempo de espera.

El puerto HTTP 5080 es para esta prueba local. Las fases de SQLite, usuarios,
salas, partidas y SignalR se incorporarán después de validar esta base.

El formato del perfil compartido sigue la
[documentación de Microsoft](https://learn.microsoft.com/es-es/visualstudio/ide/how-to-set-multiple-startup-projects?view=visualstudio).

# BORRADOR-PROYECTO-SIST

## Quiz Arena — fase 2.2: banco de preguntas de Trivial

Solución C# con una API REST, una única base de datos SQLite y un cliente WinForms.
Se pueden abrir varias instancias independientes del cliente en el mismo ordenador;
todas utilizan el servidor en `http://localhost:5080`.

Esta fase incorpora categorías, preguntas, dificultades e importación local de un
banco JSON. El cliente conserva las funciones de conexión de la fase 1. Todavía no
se implementan la pantalla de acceso, las partidas ni SignalR.

```text
QuizArena.sln
├── QuizArena.Servidor
│   ├── Program.cs
│   ├── Controllers/
│   │   ├── EstadoController.cs
│   │   ├── JugadoresController.cs
│   │   ├── CategoriasController.cs
│   │   └── PreguntasController.cs
│   ├── Data/
│   │   ├── QuizArenaDbContext.cs
│   │   └── ImportadorPreguntas.cs
│   ├── Models/
│   │   ├── Jugador.cs
│   │   ├── Categoria.cs
│   │   ├── Pregunta.cs
│   │   └── DificultadPregunta.cs
│   ├── DTOs/
│   │   ├── CrearJugadorDto.cs
│   │   ├── JugadorRespuestaDto.cs
│   │   ├── PreguntaImportacionDto.cs
│   │   └── PreguntaPublicaDto.cs
│   ├── Datos/preguntas.json
│   ├── Migrations/
│   └── Properties/launchSettings.json
└── QuizArena.Cliente    Windows Forms, sin cambios en esta fase
```

Se utiliza **.NET 10** con Visual Studio 2026. El servidor mantiene únicamente las
referencias directas a `Microsoft.EntityFrameworkCore.Sqlite` y
`Microsoft.EntityFrameworkCore.Tools`, ambas en la versión **10.0.14**. La lectura
del JSON utiliza `System.Text.Json`, incluido en .NET; no requiere otro paquete.

### Arquitectura y relaciones

Los dos proyectos son ejecutables independientes dentro de la misma solución y se
comunican por HTTP. El cliente comprueba el servidor mediante
`GET http://localhost:5080/api/estado` y no tiene una referencia al ensamblado del
servidor.

Los controladores reciben `QuizArenaDbContext` mediante inyección de dependencias.
El contexto hereda de `DbContext` y contiene los conjuntos `Jugadores`,
`Categorias` y `Preguntas`.

- `Jugador` conserva su configuración de la fase 2.1: nombre de 3 a 20 caracteres
  validado por el DTO, nombre normalizado único y fecha de registro UTC.
- Una `Categoria` contiene muchas `Pregunta`. `Pregunta.CategoriaId` es la clave
  externa. El borrado de una categoría con preguntas está restringido.
- Los nombres normalizados de categorías y los enunciados normalizados de
  preguntas tienen índices únicos. El importador elimina los espacios exteriores
  y convierte las claves a mayúsculas para detectar repeticiones.
- `DificultadPregunta` admite `Facil`, `Media` y `Dificil`, y se almacena como texto.
- Cada pregunta tiene cuatro opciones, una respuesta correcta interna, procedencia,
  licencia y un indicador `Activa`. La API consulta únicamente preguntas activas.

`PreguntaImportacionDto` permite cargar el registro completo desde el JSON local.
`PreguntaPublicaDto` define los campos que reciben los clientes: `id`, `enunciado`,
`categoriaId`, `categoria`, `dificultad`, `opcionA`, `opcionB`, `opcionC` y `opcionD`.
No contiene `respuestaCorrecta` ni `correcta`.

Los endpoints de lectura son públicos. La exclusión de la solución del DTO evita
enviarla en esas respuestas; todavía no existe autenticación ni control de acceso
por jugador. El JSON del repositorio contiene las soluciones para la importación
local y es accesible a quienes tengan acceso al código fuente.

### Preparar la base de datos desde un clon nuevo

1. Abrir `QuizArena.sln` y restaurar los paquetes NuGet.
2. Abrir **Herramientas → Administrador de paquetes NuGet → Consola del
   Administrador de paquetes** en Visual Studio.
3. Aplicar las migraciones existentes al proyecto servidor:

```powershell
Update-Database -Project QuizArena.Servidor -StartupProject QuizArena.Servidor
```

`Migrations` incluye **CreacionInicial**, que crea `Jugadores`, y **BancoPreguntas**,
que añade `Categorias` y `Preguntas`. El comando aplica las migraciones que falten.
No hay que volver a ejecutar `Add-Migration CreacionInicial` ni
`Add-Migration BancoPreguntas` al clonar el repositorio.

La actualización de una base de la fase 2.1 conserva la tabla de jugadores y sus
datos. Se mantiene la migración inicial. Antes de aplicar una actualización a una
base existente, detener el servidor y guardar una copia de seguridad de SQLite.

La base de datos se sitúa en `ContentRootPath/quizarena.db`. Con la organización
actual de este equipo, la ruta es:

```text
C:\Users\Juan\Desktop\SIST\QuizArena.Servidor\quizarena.db
```

La base de datos y sus archivos auxiliares están excluidos de Git. Las migraciones
y `Datos/preguntas.json` sí se incluyen para reproducir la estructura y el banco
inicial. El servidor no aplica migraciones automáticamente al arrancar.

Después de aplicar las migraciones en un clon nuevo, las tablas están vacías.
Los perfiles locales Carlos, Lucia y Pablo no son datos sembrados por la migración.

### Importar las preguntas

El banco inicial `QuizArena.Servidor/Datos/preguntas.json` contiene **12 preguntas
originales de ejemplo**, con cuatro opciones cada una, procedencia y licencia.
Incluye cuatro categorías: **Ciencia, Geografía, Historia y Tecnología**. Cada
categoría tiene tres preguntas, una por cada dificultad.

Con el servidor detenido, abrir una terminal en la carpeta del proyecto servidor:

```powershell
cd C:\Users\Juan\Desktop\SIST\QuizArena.Servidor
dotnet run -- --importar-preguntas
```

En un banco vacío, la salida esperada es:

```text
Preguntas nuevas: 12
Preguntas ya existentes: 0
Importación terminada.
```

Al repetir el mismo comando, la salida esperada es:

```text
Preguntas nuevas: 0
Preguntas ya existentes: 12
Importación terminada.
```

El importador valida categorías, enunciados, dificultad, opciones distintas,
respuesta correcta, fuente y licencia antes de guardar conjuntamente los cambios.
Reimportar el mismo banco no duplica los enunciados. Esta operación es de
mantenimiento local y termina sin iniciar el servidor HTTP. No existe un endpoint
de importación ni una ruta estática para descargar el JSON.

Si el archivo falta, contiene JSON malformado o incluye registros inválidos,
el comando muestra un mensaje de error y termina con código de salida 1.
La validación del banco completo evita guardar una importación parcial.

### Ejecutar desde Visual Studio

1. Abrir `QuizArena.sln` con Visual Studio 2026.
2. Aplicar las migraciones e importar el banco si todavía no se ha hecho.
3. Seleccionar **Quiz Arena - Servidor y cliente** en la barra de inicio.
   El perfil compartido se guarda en `QuizArena.slnLaunch`.
4. Pulsar **F5** para iniciar ambos proyectos.

El botón **Comprobar conexión** del formulario continúa mostrando el estado del
servidor y el JSON de la fase 1. Las preguntas se consultan por REST en esta fase.

Si el perfil no aparece, abrir las propiedades de la solución → configurar
proyectos de inicio → varios proyectos de inicio y establecer **Iniciar** para el
servidor y el cliente, en ese orden.

También se pueden ejecutar desde dos terminales situadas en la raíz del repositorio:

```powershell
# Primera terminal
dotnet run --project QuizArena.Servidor --launch-profile http
```

```powershell
# Segunda terminal
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
| GET | `/api/jugadores` | HTTP 200 con los perfiles ordenados por Id |
| GET | `/api/jugadores/{id}` | HTTP 200 con el perfil o HTTP 404 si no existe |
| POST | `/api/jugadores` | HTTP 201 y Location al crear; HTTP 400 si el nombre es inválido; HTTP 409 si ya existe |
| GET | `/api/categorias` | HTTP 200 con categorías ordenadas por nombre y su total de preguntas activas |
| GET | `/api/preguntas` | HTTP 200 con hasta 50 preguntas activas, ordenadas por Id |
| GET | `/api/preguntas/{id}` | HTTP 200 con una pregunta activa o HTTP 404 si no existe o está inactiva |
| GET | `/api/preguntas/aleatorias?cantidad=5` | HTTP 200 con cinco preguntas activas distintas |

El listado y la selección aleatoria admiten los filtros opcionales `categoriaId`
y `dificultad`. Se deben utilizar los identificadores reales de `/api/categorias`.

```text
/api/preguntas?dificultad=Dificil
/api/preguntas?categoriaId=2
/api/preguntas?categoriaId=2&dificultad=Media
/api/preguntas/aleatorias?cantidad=3&dificultad=Media
```

`cantidad` debe estar entre **1 y 15**. El servidor devuelve HTTP 400 para una
cantidad fuera de rango o filtros inválidos. Si la selección filtrada no contiene
suficientes preguntas, devuelve HTTP 409 con el número de preguntas disponibles.
Una categoría positiva inexistente produce un listado vacío; al solicitar una
selección aleatoria insuficiente, produce HTTP 409.

Con el banco inicial, solicitar `cantidad=20` devuelve HTTP 400 y solicitar
`cantidad=15` devuelve HTTP 409, porque hay doce preguntas. No se repiten preguntas
dentro de una selección aleatoria. Las solicitudes independientes pueden compartir
preguntas; el control entre rondas se incorporará con las partidas.

Ejemplos de consulta desde PowerShell, con el servidor iniciado:

```powershell
Invoke-RestMethod -Uri 'http://localhost:5080/api/categorias'
Invoke-RestMethod -Uri 'http://localhost:5080/api/preguntas'
Invoke-RestMethod -Uri 'http://localhost:5080/api/preguntas?dificultad=Dificil'
Invoke-RestMethod -Uri 'http://localhost:5080/api/preguntas/aleatorias?cantidad=5'
Invoke-RestMethod -Uri 'http://localhost:5080/api/jugadores'
```

El registro de jugadores conserva sus reglas: el nombre es obligatorio, tiene
entre 3 y 20 caracteres y no puede repetirse ignorando mayúsculas y espacios
exteriores. Los perfiles devueltos contienen `id`, `nombre` y `fechaRegistroUtc`.

### Comprobaciones superadas de la fase 2.2

Se han superado **38 comprobaciones de integración**, con compilación de la
solución sin errores ni advertencias. Las pruebas que modifican el banco para
simular errores, inactivación y mayor volumen se realizaron en copias aisladas.

- La solución compila sin errores y conserva las funciones de las fases 1 y 2.1.
- `BancoPreguntas` se aplica después de `CreacionInicial` sin borrar los jugadores.
- La primera importación inserta doce preguntas y cuatro categorías.
- La segunda importación detecta las doce preguntas existentes sin duplicarlas.
- `/api/categorias` devuelve las cuatro categorías con tres preguntas activas cada una.
- `/api/preguntas` devuelve doce preguntas; cada dificultad devuelve cuatro.
- Los filtros por categoría y dificultad funcionan por separado y combinados.
- Cada selección aleatoria devuelve la cantidad solicitada con identificadores distintos.
- Las cantidades y filtros inválidos devuelven HTTP 400; una selección insuficiente,
  HTTP 409; una pregunta inexistente o inactiva, HTTP 404.
- Ninguna respuesta de preguntas incluye `respuestaCorrecta` ni `correcta`.
- El JSON no se puede descargar mediante una ruta estática y no hay endpoint de importación.
- Un banco inválido o JSON malformado se rechaza sin altas parciales.
- Las preguntas inactivas se excluyen de consultas, selecciones y recuentos.
- La importación detecta duplicados dentro del mismo archivo y el listado
  conserva el límite de 50 con un banco de mayor tamaño.
- Carlos, Lucia y Pablo se conservan; categorías, preguntas y jugadores persisten
  después de detener y reiniciar el servidor.
- Git incluye las migraciones y el JSON, y excluye la base de datos local.

La aplicación de migraciones sigue la
[documentación de EF Core para Visual Studio](https://learn.microsoft.com/en-us/ef/core/cli/powershell).
El perfil de inicio compartido sigue la
[documentación de Microsoft](https://learn.microsoft.com/es-es/visualstudio/ide/how-to-set-multiple-startup-projects?view=visualstudio).

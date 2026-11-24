# BORRADOR-PROYECTO-SIST

## Quiz Arena — fases 2.3 y 3: banco ampliado y acceso de jugadores

Solución C# para Visual Studio 2026 y **.NET 10**, con una API REST en
`http://localhost:5080`, una base de datos SQLite y varias instancias WinForms.
Cada ventana puede acceder con una cuenta distinta y mantiene sus propias cookies.

La fase 2.3 incorpora 100 preguntas al banco inicial de 12. La fase 3 añade
registro con contraseña, inicio de sesión, consulta del perfil y cierre de sesión.

```text
QuizArena.sln
├── QuizArena.Servidor
│   ├── Program.cs
│   ├── Controllers/
│   │   ├── EstadoController.cs
│   │   ├── JugadoresController.cs
│   │   ├── CategoriasController.cs
│   │   ├── PreguntasController.cs
│   │   └── AccesoController.cs
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
│   │   ├── CredencialesDto.cs
│   │   ├── PreguntaImportacionDto.cs
│   │   └── PreguntaPublicaDto.cs
│   ├── Datos/
│   │   ├── preguntas.json
│   │   └── preguntas_trivial_100_quizarena.json   Solo local, excluido de Git
│   ├── Migrations/
│   └── Properties/launchSettings.json
└── QuizArena.Cliente
    ├── Program.cs
    ├── PerfilJugador.cs
    ├── Form1.cs
    ├── Form1.Designer.cs
    └── Form1.resx
```

El servidor mantiene las referencias directas a
`Microsoft.EntityFrameworkCore.Sqlite` y `Microsoft.EntityFrameworkCore.Tools`,
ambas en la versión **10.0.14**. La autenticación por cookies, `PasswordHasher` y
`System.Text.Json` están disponibles con los componentes de ASP.NET Core y .NET
utilizados por la solución; esta fase no necesita paquetes adicionales.

### Arquitectura y relaciones

Los dos proyectos son aplicaciones independientes de la misma solución y se
comunican por HTTP. El cliente no tiene una referencia al ensamblado del servidor.
Los controladores reciben `QuizArenaDbContext` mediante inyección de dependencias;
el contexto contiene `Jugadores`, `Categorias` y `Preguntas`.

- `Jugador` tiene nombre visible, nombre normalizado único, fecha de registro UTC
  y `PasswordHash` nullable. `PasswordHasher<Jugador>` genera y verifica los hashes.
- Los jugadores anteriores a esta fase conservan sus registros y tienen
  `PasswordHash = null`. No pueden iniciar sesión sin credenciales asignadas
  mediante un procedimiento controlado. Las pruebas de acceso utilizan cuentas
  nuevas, como CarlosQA, LuciaQA y PabloQA.
- Una categoría contiene muchas preguntas mediante `Pregunta.CategoriaId`.
  El borrado de categorías con preguntas está restringido. Los nombres de
  categorías y los enunciados normalizados tienen índices únicos.
- Cada pregunta contiene cuatro opciones, dificultad, procedencia, licencia,
  respuesta correcta interna y estado `Activa`. Las consultas públicas solo
  incluyen preguntas activas.

`CredencialesDto` recibe el nombre y la contraseña. El nombre debe tener entre
**3 y 20 caracteres** y la contraseña entre **6 y 64**. `JugadorRespuestaDto`
devuelve únicamente `id`, `nombre` y `fechaRegistroUtc`; los hashes y las
contraseñas no forman parte de las respuestas públicas.

`PreguntaPublicaDto` devuelve `id`, `enunciado`, `categoriaId`, `categoria`,
`dificultad` y las cuatro opciones. Excluye `respuestaCorrecta` y `correcta`.
El importador local utiliza `PreguntaImportacionDto`, que sí contiene la solución.

### Preparar o actualizar SQLite

1. Abrir `QuizArena.sln` y restaurar los paquetes NuGet.
2. Detener el servidor antes de actualizar una base existente y guardar una copia
   de seguridad de SQLite.
3. Abrir **Herramientas → Administrador de paquetes NuGet → Consola del
   Administrador de paquetes** en Visual Studio.
4. Aplicar las migraciones existentes:

```powershell
Update-Database -Project QuizArena.Servidor -StartupProject QuizArena.Servidor
```

Las migraciones se acumulan en este orden:

| Migración | Cambio |
| --- | --- |
| `CreacionInicial` | Crea la tabla de jugadores |
| `BancoPreguntas` | Añade categorías y preguntas |
| `AccesoJugadores` | Añade `PasswordHash` nullable a los jugadores |

Al actualizar, se conservan los jugadores, categorías y preguntas existentes.
No hay que eliminar la base ni volver a generar estas migraciones en un clon
nuevo. El servidor no modifica automáticamente el esquema al arrancar.

El archivo se sitúa en `ContentRootPath/quizarena.db`. En este equipo corresponde a:

```text
C:\Users\Juan\Desktop\SIST\QuizArena.Servidor\quizarena.db
```

La base de datos y sus auxiliares se excluyen de Git; las migraciones sí forman
parte del repositorio. En un clon nuevo las tablas estarán vacías después de
`Update-Database`: los perfiles y cuentas locales no son datos sembrados.

### Importar los bancos de preguntas

`Datos/preguntas.json` conserva las **12 preguntas originales** y se incluye en
Git. `Datos/preguntas_trivial_100_quizarena.json` contiene las **100 preguntas
aportadas**, adaptadas al formato del importador sin cambiar sus enunciados,
opciones ni respuestas correctas.

Las 100 preguntas nuevas tienen dificultad **Media provisional**. Su procedencia
figura como `Banco trivial 100 aportado` y su licencia como `Pendiente de verificar`.
El banco adaptado se mantiene local y está excluido de Git mientras se verifica
su permiso de distribución. Por eso, un clon nuevo necesita disponer de ese
archivo local para reproducir el banco completo de 112 preguntas.

Con el servidor detenido, abrir una terminal en el proyecto servidor:

```powershell
cd C:\Users\Juan\Desktop\SIST\QuizArena.Servidor
```

Importar primero el banco inicial:

```powershell
dotnet run -- --importar-preguntas
```

El archivo predeterminado sigue siendo `Datos/preguntas.json`. Para importar el
banco aportado, colocarlo en `Datos` y ejecutar:

```powershell
dotnet run -- --importar-preguntas --archivo=preguntas_trivial_100_quizarena.json
```

`--archivo=` admite únicamente el nombre de un archivo `.json` dentro de `Datos`,
sin rutas de otras carpetas. La operación valida todo el banco antes de guardar,
termina sin iniciar HTTP y no dispone de un endpoint público.

Si solo están las 12 preguntas iniciales, la primera importación del nuevo archivo
produce 100 preguntas nuevas y cero existentes. Al repetirla, produce cero nuevas
y 100 existentes. Los enunciados normalizados impiden duplicados entre archivos
y dentro del mismo banco.

Con ambos bancos, el resultado esperado es **112 preguntas y 11 categorías**:

| Categoría | Preguntas |
| --- | ---: |
| Arte y literatura | 10 |
| Ciencia | 13 |
| Cine y series | 10 |
| Cultura general | 10 |
| Deportes | 10 |
| Geografía | 13 |
| Historia | 13 |
| Música | 10 |
| Naturaleza | 10 |
| Tecnología | 3 |
| Tecnología y videojuegos | 10 |
| **Total** | **112** |

`GET /api/preguntas` devuelve como máximo 50 registros. Para contar el banco
completo, sumar `totalPreguntas` de `GET /api/categorias`:

```powershell
$categorias = Invoke-RestMethod -Uri 'http://localhost:5080/api/categorias'
($categorias | Measure-Object -Property totalPreguntas -Sum).Sum
```

### Ejecutar y utilizar WinForms

1. Abrir `QuizArena.sln` con Visual Studio 2026.
2. Aplicar las migraciones e importar los bancos disponibles.
3. Seleccionar **Quiz Arena - Servidor y cliente** en la barra de inicio.
4. Pulsar **F5** para iniciar ambas aplicaciones.

El formulario permite escribir nombre y contraseña, **Registrarse**, **Iniciar
sesión**, **Ver mi perfil**, **Cerrar sesión** y **Comprobar conexión**. La
contraseña se oculta al escribir. Registrar una cuenta no inicia automáticamente
la sesión; después se debe utilizar **Iniciar sesión**.

Cada instancia del formulario utiliza su propio `HttpClient`, `HttpClientHandler`
y `CookieContainer`. La sesión de una ventana no modifica las de otras ventanas.
El perfil se solicita mediante la cookie de sesión y no mediante un identificador
de jugador proporcionado por el cliente.

Para probar tres jugadores, mantener un único servidor abierto y ejecutar tres
veces `QuizArena.Cliente.exe`, situado normalmente en:

```text
QuizArena.Cliente\bin\Debug\net10.0-windows\
```

Registrar e iniciar sesión con tres cuentas distintas, por ejemplo CarlosQA,
LuciaQA y PabloQA, introduciendo una contraseña propia en cada ventana. Consultar
los perfiles y cerrar una sesión; las otras dos deben seguir funcionando.
Las contraseñas no se guardan en los archivos del proyecto ni en este documento.

También se pueden ejecutar ambas aplicaciones desde dos terminales en la raíz:

```powershell
dotnet run --project QuizArena.Servidor --launch-profile http
```

```powershell
dotnet run --project QuizArena.Cliente
```

Para compilar toda la solución:

```powershell
dotnet build QuizArena.sln
```

### API de acceso

| Método | Ruta | Resultado |
| --- | --- | --- |
| POST | `/api/acceso/registrar` | HTTP 201 con el perfil; HTTP 409 si el nombre ya existe; HTTP 400 para credenciales inválidas |
| POST | `/api/acceso/entrar` | HTTP 200 con el perfil y cookie de sesión; HTTP 401 si las credenciales no son correctas |
| GET | `/api/acceso/yo` | HTTP 200 con el perfil de la sesión activa; HTTP 401 sin autenticación |
| POST | `/api/acceso/salir` | HTTP 204 al cerrar la sesión autenticada; HTTP 401 sin sesión |

El servidor usa la cookie `QuizArena.Sesion`, marcada `HttpOnly` y `SameSite=Strict`,
con una vigencia de cuatro horas y renovación deslizante. Las respuestas de
autorización de la API son HTTP 401/403 y no redirecciones a una página de acceso.
HTTP se utiliza en el entorno local de esta práctica.

`SignOutAsync` envía la eliminación de la cookie al cliente. Después, ese cliente
deja de presentar su sesión y `/api/acceso/yo` responde HTTP 401. El cierre no
revoca una copia externa de una cookie ya emitida; esta fase no mantiene un
registro central de sesiones revocadas.

El antiguo `POST /api/jugadores` se mantiene como operación heredada de desarrollo:
crea perfiles sin contraseña y no se utiliza para registrar cuentas de acceso.
Las cuentas nuevas deben crearse mediante `/api/acceso/registrar`.

### API de estado, jugadores y preguntas

| Método | Ruta | Resultado |
| --- | --- | --- |
| GET | `/api/estado` | Estado del servidor |
| GET | `/api/jugadores` | Perfiles ordenados por Id, sin hashes ni contraseñas |
| GET | `/api/jugadores/{id}` | Perfil público o HTTP 404 |
| GET | `/api/categorias` | Categorías ordenadas y total de preguntas activas |
| GET | `/api/preguntas` | Hasta 50 preguntas activas, ordenadas por Id |
| GET | `/api/preguntas/{id}` | Pregunta activa sin solución o HTTP 404 |
| GET | `/api/preguntas/aleatorias?cantidad=10` | Diez preguntas activas distintas dentro de la selección |

El listado y las selecciones aleatorias admiten `categoriaId` y `dificultad`
(`Facil`, `Media` o `Dificil`). Los filtros se pueden combinar. La cantidad
aleatoria debe estar entre 1 y 15: los parámetros inválidos devuelven HTTP 400
y una selección sin suficientes preguntas devuelve HTTP 409 con su disponibilidad.
Las peticiones independientes pueden compartir preguntas.

Las consultas generales siguen siendo públicas. El archivo JSON no tiene una
ruta estática de descarga y las soluciones se reservan para la importación y
la lógica interna. Las salas, rondas y partidas se incorporarán en fases posteriores.

### Comprobaciones superadas

Validación completada el **24 de noviembre de 2026**: compilación con **0 errores
y 0 advertencias**, **32 comprobaciones de API y datos** superadas y pruebas
reales con tres ventanas WinForms. Después de reiniciar Visual Studio y el
servidor, se conservan las 112 preguntas, las 11 categorías y los seis perfiles
locales, y las cuentas con contraseña pueden volver a iniciar sesión.

En WinForms se verificaron registro correcto, nombre duplicado, contraseña
incorrecta, acceso correcto, consulta del perfil y cierre de sesión. Las ventanas
de CarlosQA, LuciaQA y PabloQA mostraron identidades distintas; tras cerrar la
sesión de CarlosQA, las otras dos siguieron consultando sus propios perfiles.

- Solución compilada sin errores y migración `AccesoJugadores` aplicada.
- Incorporación de 100 preguntas sin perder las 12 ni los jugadores anteriores.
- Reimportación sin duplicados; 112 preguntas y 11 categorías al disponer de ambos bancos.
- Registro HTTP 201, nombres duplicados HTTP 409 y validación de credenciales HTTP 400.
- Inicio de sesión correcto HTTP 200; credenciales incorrectas o perfiles antiguos sin contraseña HTTP 401.
- Perfil de sesión HTTP 200 y perfil sin autenticación HTTP 401.
- Cierre de sesión HTTP 204 seguido de perfil HTTP 401 en ese cliente.
- Tres instancias WinForms con identidades independientes y cierre de una sin afectar a las demás.
- Botón de conexión, consultas y filtros anteriores operativos.
- Persistencia de perfiles y banco tras reiniciar el servidor.
- Respuestas públicas sin hashes, contraseñas ni soluciones de las preguntas.
- Migraciones y código disponibles para Git; SQLite y el banco aportado de licencia pendiente excluidos.

La configuración de acceso utiliza la
[autenticación por cookies de ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-10.0).
Las migraciones se aplican con las
[herramientas de EF Core para Visual Studio](https://learn.microsoft.com/en-us/ef/core/cli/powershell).

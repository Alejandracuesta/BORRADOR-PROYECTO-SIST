# Quiz Arena — estado de la fase 8

Fecha: 26 de noviembre de 2026. Repositorio: `C:/Users/Juan/Desktop/SIST`.
Referencia: `C:/Users/Juan/.codex/attachments/0bf71620-7046-48d5-b474-8c6e902f18f4/Texto pegado.txt`.
Evidencias: `C:/Users/Juan/Documents/Codex/2026-11-23/te/work/fase8`.

## Implementación terminada

- CRUD propio de cuentas, contraseña actual, baja lógica y anonimización.
- Cuentas Activo/EsAdministrador/EliminadoUtc; cookies revalidadas y Hub comprueba Activo.
- Registro heredado 410 y listados públicos limitados a jugadores activos.
- Rol administrativo en servidor y comando local `--hacer-admin=NOMBRE`.
- CRUD de categorías bajo coordinador compartido y borrado solo sin referencias.
- CRUD de preguntas, reactivación, consulta administrativa paginada y validación compartida con importador.
- Preguntas con historial no editables; GET públicos sin solución.
- Cancelación por anfitrión con historial preservado y bloqueo durante partida activa.
- Edición y baja de mensajes propios; ChatActualizado privado sin transportar texto.
- Historial personal y resumen autorizado; puntos y aciertos de equipos correctamente representados.
- PerfilForm, AdministracionForm e HistorialForm, diseñadores y accesos desde Form1.
- Botón de cancelar sala; edición de chat con selección y borrador conservados.
- Swagger/OpenAPI en 5080 y 5162, ruta /swagger/index.html y descripción de permisos.
- QuizArena.Tests, referencia al servidor y bases temporales verificadas.
- Podio individual conservado y adaptado para marcador y empates de equipos.
- Reacciones consecutivas siguen permitidas; el límite de dos segundos no se ha reintroducido.

## Verificaciones completadas

19 casos xUnit pasan, sin omisiones. TRX: `.../te/outputs/fase8-pruebas/fase8.trx`.
Incluyen CRUD, errores 400/401/403/404/409/410, cookies antiguas, permisos,
SignalR privado, chat editable/eliminado, aforo concurrente, respuestas simultáneas,
equipos de dos y tres integrantes, decisiones irreversibles, supervisor sin REST,
puntuación por rapidez, revancha individual/equipos, historial y Swagger.
Compilación de la solución con cero errores y advertencias; diez migraciones y
EF sin cambios pendientes. La factoría de diseño evita arrancar el servidor al
crear migraciones. Las dos nuevas se revisaron antes de aplicarlas.

Validación nativa sobre `work/fase8/visual.db`: cuatro procesos independientes,
alta y edición de categoría, alta de pregunta, cambio de nombre, edición de
mensaje privado, entrada automática, individual 1048–821, equipos 650–650 y
revancha 1200–1200. Se utilizaron peticiones REST de apoyo para preparar salas
y completar las respuestas/votos. Historial consultado desde WinForms después
de reiniciar. Podio de equipos corregido y verificado visualmente.
Los tres nuevos diseñadores se abrieron en Visual Studio correctamente.
Las bajas de cuenta, contraseñas y permisos de eliminación se verificaron con
xUnit; no se afirma que se hayan realizado manualmente todos esos botones.

## Datos y respaldo

Base real preservada: diez jugadores, once categorías, 112 preguntas y veinte
partidas. Los datos de cuentas, banco e historial coinciden con la copia previa.
Salas, pertenencias y chat registran cambios de uso posteriores a la primera
revisión; se conserva su estado actual, sin restaurar respaldos sobre él.
SQLite integrity_check=ok y foreign_key_check vacío.

Migraciones añadidas:
- 20261126150616_AdministracionJugadores: solo tres columnas de Jugadores.
- 20261126150755_EdicionMensajes: solo EditadoUtc y Eliminado de MensajesSala.

Respaldos fuera del repo: `.../te/outputs/QuizArena-antes-fase8.zip` y
`.../te/outputs/quizarena-antes-fase8.db`. Instantánea privada de comparación:
`work/fase8/antes-fase8.json`. No publicar bases, cookies, hashes ni instantáneas.
El banco marcado Propias, ya incluido por el usuario, se conserva.

## Operación local pendiente: administrador de la base real

La administración se probó con AdminVisual8 en la base temporal y se verificó
el comando local de promoción. No se ha creado ni promovido una cuenta real.
La revisión automática rechazó el intento de crear/habilitar AdminQuiz en la
base real con el motivo «blocked by policy», sin más detalle. No repetir la
operación mediante otra vía para evitar ese rechazo.

El propietario puede registrar su cuenta con contraseña propia y, con el
servidor detenido, ejecutar desde la solución:

```powershell
dotnet run --project QuizArena.Servidor -- --hacer-admin=TU_USUARIO
```

La siguiente petición o un nuevo inicio de sesión recuperará su rol. No hay
contraseña fija ni endpoint público para asignárselo.

## Entrega

README actualizado y guion de diez minutos en DEMO-FASE8.md.
La fase 7 previa quedó guardada en eb614d9.
Se conserva WebApplication1 y su incorporación local en la solución, que ya eran
cambios del usuario. La entrega de Quiz Arena solo incorpora servidor, cliente
y pruebas; no se debe versionar accidentalmente ese proyecto auxiliar sin pedirlo.

No repetir los scripts `work/fase8/implementar_*.py`: ya están ejecutados.
El cliente y servidor de prueba están detenidos; VS queda en PerfilForm diseñador.
Para reproducir la validación: `dotnet test QuizArena.sln`. Las pruebas crean
sus propias bases temporales y no requieren tocar quizarena.db.

## Comprobación de entrega limpia

Se exportó el código confirmado en fb9236f a una carpeta nueva, sin SQLite,
binarios, proyectos auxiliares ni configuración local. Restore y build de sus
**tres proyectos** terminaron con 0 errores y 0 advertencias, y sus **19 casos
xUnit** pasaron de nuevo, sin omisiones. Resultado:
`.../te/outputs/fase8-pruebas/entrega-limpia.trx`.

La fase 8 se confirmó en **5d43295**. El commit remoto API **919a7e8** se integró
sin descartar sus mejoras en **fb9236f**: HTML Swagger original, acceso desde raíz,
apertura del navegador y ejemplos HTTP. Se corrigió el campo de credenciales del
archivo .http a `clave`. La solución versionada contiene servidor, cliente y
pruebas; los cambios locales previos de WebApplication1 permanecen sin confirmar.

La implementación está terminada. La única acción de configuración pendiente
es habilitar una cuenta real administradora mediante el comando local descrito
arriba, porque la revisión automática rechazó ejecutarlo. El resultado de la
publicación queda recogido en el informe local final y en el estado de origin/main.

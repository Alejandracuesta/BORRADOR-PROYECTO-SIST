# Quiz Arena — continuar fase 7

## Situación al guardar este punto
**Implementación funcional completa**, compilada con 0 errores y 0 advertencias.
No declarar la validación completa hasta terminar los pasos pendientes de abajo.
Fecha local: 25 de noviembre de 2026.
Repositorio: `C:/Users/Juan/Desktop/SIST` (base inicial `9875e7d`, fase 6 ya confirmada).
Instrucciones exactas: `C:/Users/Juan/.codex/attachments/dddbc3cc-0d18-43b6-bf0d-f570bf7f83fc/Texto pegado.txt`.
Copia, scripts y evidencias: `C:/Users/Juan/Documents/Codex/2026-11-23/te/work/fase7`.

## Implementado
- SignalR.Client 10.0.14 y sesión de cookies compartida; SalaHub autenticado, grupos sala/equipo y NotificadorTiempoReal.
- Eventos de salas/partidas/votos; refrescos pendientes y REST de respaldo cada 10s; reloj local cada segundo.
- Chat general y privado por equipo, historial autorizado de últimos 50, mensajes de 1–250 caracteres, frecuencia por cuenta 1s y reacciones 2s.
- ChatForm, ClasificacionForm y botones en diseñadores; chat cambia de ámbito tras terminar equipos y recupera historial al reconectar.
- Revancha: prepara misma sala, desmarca a todos, consultas seleccionan última partida; puntuación y votos nuevos independientes.
- Conservación del historial al salir de una revancha en espera; clasificación distingue modo histórico por asignación de equipo aunque cambie la modalidad de la sala.
- SupervisorPartidas cada segundo bajo mismo semáforo; notificaciones del supervisor fuera del semáforo.
- Reconexión automática, reintento inicial y tras cierre, reincorporación a grupos e indicadores WinForms.
- Clasificación individual acumulada y mejores resultados de equipos por partida, autenticadas.
- README funcional actualizado. Sin commit/push.
- **Conservada la puntuación individual por rapidez que YA EXISTÍA en HEAD.** Las pruebas antiguas de puntos fijos no son válidas; no cambiar la regla del usuario.

## Migraciones APLICADAS
1. `20261124224017_ChatSalas` — solo crea MensajesSala e índices/relaciones.
2. `20261124224444_RevanchaSalas` — solo sustituye índice único SalaId por no único.
Ocho migraciones totales. Base original conservada. No borrar/recrear SQLite.
Respaldos en `.../te/outputs/QuizArena-antes-fase7.zip` y `quizarena-antes-fase7.db`.
Instantánea privada de filas previas: `work/fase7/antes-fase7.json`, no publicar ni imprimir hashes.
Comprobar datos con `python -X utf8 work/fase7/verificar_integridad.py` desde el workspace.

## Verificación automática SUPERADA (276)
- 12 SignalR real: cookies, rechazo anónimo/grupo ajeno, avisos de listado, incorporación e inicio. `resultado-71.txt`.
- 39 chat real con cuatro conexiones: privacidad bidireccional, historial, ámbito, mensajes y reacciones/frecuencia, salida. `resultado-72.txt`.
- 29 supervisor/revancha/clasificación: 50s sin REST, dos partidas de equipos y una individual consecutivas en la misma sala; todos salen. `resultado-73-75.txt`.
- 77 aisladas de equipos con SQLite, repartos 4–12, puntuaciones, rachas, límites e idempotencia. `resultado-regresion-equipos.json` y `regresion-equipos-v2.log`.
- 48 aisladas individuales adaptadas a rapidez real, tres dificultades, rachas y recuperación. `regresion-individual-final.log`.
- 35 salas y 32 acceso/banco. `work/resultado-fase7-salas.json` y `work/resultado-fase7-acceso.json`. Su sala temporal final ya se retiró.
- 4 comparación clasificación/SQL y conservación tras cambiar modalidad. `resultado-clasificacion.json`.
- Integridad original comprobada con ocho migraciones, foreign_key_check vacío e integrity_check ok.

## Verificación NATIVA realizada
- Cuatro WinForms con CarlosQA, LuciaQA, PabloQA y MartaQA; misma sala privada `4F63D090`, nombre Validación visual fase 7.
- Sala/entradas/preparación montadas por API. Un clic REAL del anfitrión abre cuatro partidas automáticamente, todas conectadas en tiempo real.
- Primera ronda: Carlos B, Pablo A; cambio Pablo a B, unanimidad y confirmación manual, bloqueo visible. Rival sin responder obtiene cero al plazo.
- Chat privado enviado manualmente por Pablo: “Equipo 1: mensaje privado de prueba fase 7.”. Carlos recupera mensaje; Lucía abre historial vacío.
- Reacciones Aplauso reales vistas por compañeros.
- ClasificacionForm y distribución SalasForm inspeccionadas con datos reales; indicadores conectados visibles.
- El chat cambia automáticamente a general al finalizar.
- Primera partida visual `PartidaId=17` finalizada, Equipo1 400, Equipo2 0; sigue en la sala con los cuatro participantes.
- Evidencias: sala-visual.json, final-visual-1.json, reinicio-visual.json y decisiones-antes-reinicio.json.
- El archivo reinicio-visual.json se preparó para reinicio en ronda 4, **pero aún NO se ha reiniciado el servidor**; no contarlo como prueba pasada.

## PENDIENTE (seguir desde aquí)
1. Terminar prueba visual de revancha: cerrar chats/volver a sala en los cuatro clientes, solicitar revancha, preparar e iniciar segunda partida; observar cuatro aperturas y cero puntos. Completarla con apoyo API y verificar dos historiales.
2. Interrumpir/reiniciar servidor manteniendo clientes abiertos. Verificar indicadores desconectados, botones protegidos, recuperación de sesión/grupos/votos y plazos sin pausa. Falta esta prueba real.
3. Revisar nuevos diseñadores ChatForm y ClasificacionForm, compilar finalmente y ejecutar EF has-pending-model-changes.
4. Auditoría git diff --check, exclusiones DB/banco, secretos, integridad final. README validación y outputs/QuizArena-Fase7-validacion.txt con resultados reales y límites.
5. Cerrar clientes temporales, dejar VS en diseñador, liberar solo membresías QA de la sala 4F63D090 tras finalizar; conservar historiales y registros del usuario.

## Bloqueo de interfaz y entorno actual
- Visual Studio abierto, servidor escuchando 5080 y cuatro clientes reales aún abiertos.
- Avisos auxiliares de Windows de pruebas fallidas de reporte ya corregidas llevaron a diálogos Just-In-Time y Attach Security Warning para RegresionEquipos.exe. NO aceptar estos avisos.
- Se pidió al usuario pulsar Don't Attach en todos hasta ver el editor. Respondió que los cerró, pero última captura aún mostraba un aviso: reobservar antes de actuar. El último pedido asíncrono indica cerrar TODOS los avisos seguidos.
- Normas de control de Windows prohíben actuar sobre solicitudes de seguridad: usuario debe cerrarlas. Esto solo bloquea la prueba de parada/reinicio; los formularios del juego funcionan.
- Detener procesos lanzados desde VS desde el propio VS. Shell sandbox devuelve Acceso denegado al intentar Stop-Process sobre ellos.
- Para Ctrl+F5: usar Ctrl+Alt+P, filtro QuizArena.Servidor, seleccionar PID real y adjuntar; Debug > Terminate All. F5 puede interrumpir pruebas negativas con HubException; Ctrl+F5 evita esas pausas.
- No automatizar consolas con comandos UI.
- Primeros auxiliares fallaron por ruta de informe/proyecto de referencia. Corregidos; NO reusar ejecutables viejos bloqueados. Pruebas aisladas actuales incluyen fuente servidor y UseAppHost=false.

## Comandos y rutas
PowerShell; SDK 10.0.401.
DOTNET_CLI_HOME=C:/Users/Juan/Documents/Codex/2026-11-23/te/work/dotnet-home
NUGET_PACKAGES=C:/Users/Juan/Documents/Codex/2026-11-23/te/work/nuget-packages
Compilar repo: `dotnet build QuizArena.sln --no-restore` con proyectos detenidos.
EF: `work/invocar_ef.ps1 -Comando @('migrations','has-pending-model-changes')`; fijar Logging__EventLog__LogLevel__Default=None.
SignalR restaurado desde feed local work/fase7/nuget (14 nupkg) por problema TLS de dotnet; descargas HTTPS verificadas con Python. No desactivar certificados.
Scripts implementar_71.py, implementar_72.py, implementar_73.py, implementar_745.py YA EJECUTADOS, NO repetirlos.
Proyecto work/fase7/Pruebas contiene última prueba 7.3–7.5; Program-71.txt y Program-72.txt conservan previas. No repetir pruebas que crean salas sin comprobar primero que cuentas QA estén libres.

El usuario pidió dejar todo preparado si se agota su límite semanal; este punto permite retomar sin repetir fases ni borrar datos.

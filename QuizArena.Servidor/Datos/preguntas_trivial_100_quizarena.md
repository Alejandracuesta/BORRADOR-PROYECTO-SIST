# Banco trivial QuizArena — 100 preguntas

Formato: cada opción lleva la marca `[CORRECTA]` o `[INCORRECTA]`.
Hay exactamente **una** `[CORRECTA]` por pregunta y coincide con el campo
`correcta` de `preguntas_trivial_100_quizarena.json`.

## Instrucciones para Codex

- Fuente de verdad para importar: `preguntas_trivial_100_quizarena.json`
  (formato de `PreguntaImportacionDto`). Campo `correcta` = letra A/B/C/D.
- `respuestaCorrecta` = texto de la opción correcta; `respuestasIncorrectas` =
  las otras tres. Son informativos: el importador los ignora.
- Nunca enviar `correcta`, `respuestaCorrecta` ni `respuestasIncorrectas` al
  cliente mientras la pregunta esté abierta (`PreguntaPublicaDto` no los tiene).
- Importar: `dotnet run -- --importar-preguntas --archivo=preguntas_trivial_100_quizarena.json`.
- Puntuación por velocidad (ya implementada en `MotorPartidas.PuntosPorVelocidad`
  y usada en `PartidasController` al responder): solo los aciertos puntúan;
  `puntos = round(base × (1 − 0,5 × segundosTranscurridos / plazo))`, con
  base 100/200/300 (Fácil/Media/Difícil) y plazo 20/20/30 s, medido desde
  `AperturaUtc` con la hora del servidor. Responder al instante = 100 % de la
  base; al agotarse el plazo = 50 %. La bonificación de racha se suma aparte.

---

## Arte y literatura

### 1. ¿Quién escribió «Don Quijote de la Mancha»?
Dificultad: Facil

- A) Garcilaso de la Vega [INCORRECTA]
- B) Francisco de Quevedo [INCORRECTA]
- C) Miguel de Cervantes [CORRECTA]
- D) Lope de Vega [INCORRECTA]

### 2. ¿Quién pintó «La Gioconda»?
Dificultad: Facil

- A) Sandro Botticelli [INCORRECTA]
- B) Miguel Ángel [INCORRECTA]
- C) Leonardo da Vinci [CORRECTA]
- D) Rafael [INCORRECTA]

### 3. ¿Qué pintor español pintó el «Guernica»?
Dificultad: Facil

- A) Joan Miró [INCORRECTA]
- B) Pablo Picasso [CORRECTA]
- C) Diego Velázquez [INCORRECTA]
- D) Salvador Dalí [INCORRECTA]

### 4. ¿Quién escribió «Romeo y Julieta»?
Dificultad: Facil

- A) Charles Dickens [INCORRECTA]
- B) William Shakespeare [CORRECTA]
- C) Oscar Wilde [INCORRECTA]
- D) Christopher Marlowe [INCORRECTA]

### 5. ¿En qué museo se expone «Las meninas» de Velázquez?
Dificultad: Media

- A) Museo Reina Sofía [INCORRECTA]
- B) Museo del Louvre [INCORRECTA]
- C) Museo del Prado [CORRECTA]
- D) Museo Thyssen-Bornemisza [INCORRECTA]

### 6. ¿Qué autor colombiano escribió «Cien años de soledad»?
Dificultad: Media

- A) Pablo Neruda [INCORRECTA]
- B) Mario Vargas Llosa [INCORRECTA]
- C) Gabriel García Márquez [CORRECTA]
- D) Julio Cortázar [INCORRECTA]

### 7. ¿Cuántos versos tiene un soneto clásico?
Dificultad: Media

- A) 14 [CORRECTA]
- B) 10 [INCORRECTA]
- C) 16 [INCORRECTA]
- D) 12 [INCORRECTA]

### 8. ¿Qué escultor creó «El pensador»?
Dificultad: Media

- A) Miguel Ángel [INCORRECTA]
- B) Auguste Rodin [CORRECTA]
- C) Antonio Canova [INCORRECTA]
- D) Donatello [INCORRECTA]

### 9. ¿Qué movimiento artístico encabezó André Breton con su manifiesto de 1924?
Dificultad: Dificil

- A) Surrealismo [CORRECTA]
- B) Futurismo [INCORRECTA]
- C) Dadaísmo [INCORRECTA]
- D) Cubismo [INCORRECTA]

### 10. ¿Qué escritor argentino es autor de «El Aleph»?
Dificultad: Dificil

- A) Ernesto Sabato [INCORRECTA]
- B) Julio Cortázar [INCORRECTA]
- C) Adolfo Bioy Casares [INCORRECTA]
- D) Jorge Luis Borges [CORRECTA]

## Ciencia

### 11. ¿Cuál es la fórmula química del agua?
Dificultad: Facil

- A) H2O2 [INCORRECTA]
- B) H2O [CORRECTA]
- C) CO2 [INCORRECTA]
- D) O2 [INCORRECTA]

### 12. ¿Cuántos huesos tiene el cuerpo humano adulto?
Dificultad: Facil

- A) 206 [CORRECTA]
- B) 156 [INCORRECTA]
- C) 306 [INCORRECTA]
- D) 256 [INCORRECTA]

### 13. ¿Qué gas absorben las plantas durante la fotosíntesis?
Dificultad: Facil

- A) Helio [INCORRECTA]
- B) Oxígeno [INCORRECTA]
- C) Dióxido de carbono [CORRECTA]
- D) Nitrógeno [INCORRECTA]

### 14. ¿Cuál es el planeta más grande del sistema solar?
Dificultad: Facil

- A) Júpiter [CORRECTA]
- B) Saturno [INCORRECTA]
- C) Tierra [INCORRECTA]
- D) Neptuno [INCORRECTA]

### 15. ¿Cuál es la velocidad aproximada de la luz en el vacío?
Dificultad: Media

- A) 3.000.000 km/s [INCORRECTA]
- B) 30.000 km/s [INCORRECTA]
- C) 150.000 km/s [INCORRECTA]
- D) 300.000 km/s [CORRECTA]

### 16. ¿Qué orgánulo celular produce la mayor parte del ATP?
Dificultad: Media

- A) Ribosoma [INCORRECTA]
- B) Mitocondria [CORRECTA]
- C) Lisosoma [INCORRECTA]
- D) Aparato de Golgi [INCORRECTA]

### 17. ¿Cuál es el elemento químico más abundante en el universo?
Dificultad: Media

- A) Oxígeno [INCORRECTA]
- B) Carbono [INCORRECTA]
- C) Helio [INCORRECTA]
- D) Hidrógeno [CORRECTA]

### 18. ¿Qué científico formuló la teoría de la relatividad?
Dificultad: Media

- A) Isaac Newton [INCORRECTA]
- B) Niels Bohr [INCORRECTA]
- C) Galileo Galilei [INCORRECTA]
- D) Albert Einstein [CORRECTA]

### 19. ¿Cuál es el número atómico del carbono?
Dificultad: Dificil

- A) 14 [INCORRECTA]
- B) 12 [INCORRECTA]
- C) 6 [CORRECTA]
- D) 8 [INCORRECTA]

### 20. ¿Qué constante relaciona la energía de un fotón con su frecuencia?
Dificultad: Dificil

- A) Constante de Avogadro [INCORRECTA]
- B) Constante de Coulomb [INCORRECTA]
- C) Constante de Boltzmann [INCORRECTA]
- D) Constante de Planck [CORRECTA]

## Cine y series

### 21. ¿Qué película de James Cameron de 1997 narra el hundimiento de un famoso transatlántico?
Dificultad: Facil

- A) Mentiras arriesgadas [INCORRECTA]
- B) Abyss [INCORRECTA]
- C) Avatar [INCORRECTA]
- D) Titanic [CORRECTA]

### 22. ¿Cómo se llama el joven mago protagonista de la saga creada por J. K. Rowling?
Dificultad: Facil

- A) Merlín [INCORRECTA]
- B) Gandalf [INCORRECTA]
- C) Harry Potter [CORRECTA]
- D) Percy Jackson [INCORRECTA]

### 23. ¿Quién dirigió «Parque Jurásico» (1993)?
Dificultad: Facil

- A) Ridley Scott [INCORRECTA]
- B) George Lucas [INCORRECTA]
- C) Steven Spielberg [CORRECTA]
- D) James Cameron [INCORRECTA]

### 24. ¿En qué ciudad ficticia vive Batman?
Dificultad: Facil

- A) Gotham City [CORRECTA]
- B) Central City [INCORRECTA]
- C) Star City [INCORRECTA]
- D) Metrópolis [INCORRECTA]

### 25. ¿Qué serie está ambientada en los Siete Reinos de Poniente?
Dificultad: Media

- A) Juego de tronos [CORRECTA]
- B) The Witcher [INCORRECTA]
- C) Vikingos [INCORRECTA]
- D) Los anillos de poder [INCORRECTA]

### 26. ¿Cómo se llama el profesor de química protagonista de «Breaking Bad»?
Dificultad: Media

- A) Jesse Pinkman [INCORRECTA]
- B) Saul Goodman [INCORRECTA]
- C) Hank Schrader [INCORRECTA]
- D) Walter White [CORRECTA]

### 27. ¿Qué película de Alfred Hitchcock contiene la famosa escena de la ducha en el motel Bates?
Dificultad: Media

- A) Vértigo [INCORRECTA]
- B) Los pájaros [INCORRECTA]
- C) La ventana indiscreta [INCORRECTA]
- D) Psicosis [CORRECTA]

### 28. ¿Cuál fue la primera película de habla no inglesa en ganar el Óscar a mejor película?
Dificultad: Media

- A) La vida es bella [INCORRECTA]
- B) Amélie [INCORRECTA]
- C) Roma [INCORRECTA]
- D) Parásitos [CORRECTA]

### 29. ¿Cuántos Óscar ganó «El señor de los anillos: El retorno del rey»?
Dificultad: Dificil

- A) 9 [INCORRECTA]
- B) 13 [INCORRECTA]
- C) 7 [INCORRECTA]
- D) 11 [CORRECTA]

### 30. ¿Por qué película ganó Pedro Almodóvar el Óscar al mejor guion original?
Dificultad: Dificil

- A) Hable con ella [CORRECTA]
- B) Volver [INCORRECTA]
- C) Todo sobre mi madre [INCORRECTA]
- D) Mujeres al borde de un ataque de nervios [INCORRECTA]

## Cultura general

### 31. ¿Cuántos días tiene un año bisiesto?
Dificultad: Facil

- A) 364 [INCORRECTA]
- B) 366 [CORRECTA]
- C) 365 [INCORRECTA]
- D) 367 [INCORRECTA]

### 32. ¿Cuántos colores tiene el arcoíris según la tradición?
Dificultad: Facil

- A) 8 [INCORRECTA]
- B) 5 [INCORRECTA]
- C) 6 [INCORRECTA]
- D) 7 [CORRECTA]

### 33. ¿Cuántos lados tiene un hexágono?
Dificultad: Facil

- A) 6 [CORRECTA]
- B) 5 [INCORRECTA]
- C) 7 [INCORRECTA]
- D) 8 [INCORRECTA]

### 34. ¿Qué número romano representa el 10?
Dificultad: Facil

- A) X [CORRECTA]
- B) C [INCORRECTA]
- C) L [INCORRECTA]
- D) V [INCORRECTA]

### 35. ¿Qué idioma tiene más hablantes nativos en el mundo?
Dificultad: Media

- A) Español [INCORRECTA]
- B) Chino mandarín [CORRECTA]
- C) Inglés [INCORRECTA]
- D) Hindi [INCORRECTA]

### 36. ¿Cuál es la moneda oficial de Japón?
Dificultad: Media

- A) Rupia [INCORRECTA]
- B) Yen [CORRECTA]
- C) Yuan [INCORRECTA]
- D) Won [INCORRECTA]

### 37. ¿Cuántas cartas tiene una baraja francesa sin comodines?
Dificultad: Media

- A) 52 [CORRECTA]
- B) 48 [INCORRECTA]
- C) 40 [INCORRECTA]
- D) 54 [INCORRECTA]

### 38. ¿En qué fecha se celebra el Día de la Constitución en España?
Dificultad: Media

- A) 8 de diciembre [INCORRECTA]
- B) 12 de octubre [INCORRECTA]
- C) 1 de mayo [INCORRECTA]
- D) 6 de diciembre [CORRECTA]

### 39. ¿Cómo se escribe el año 1999 en números romanos?
Dificultad: Dificil

- A) MCMXCVIIII [INCORRECTA]
- B) MCMXCIX [CORRECTA]
- C) MIM [INCORRECTA]
- D) MDCCCXCIX [INCORRECTA]

### 40. ¿Cuántos husos horarios oficiales tiene Rusia?
Dificultad: Dificil

- A) 13 [INCORRECTA]
- B) 9 [INCORRECTA]
- C) 7 [INCORRECTA]
- D) 11 [CORRECTA]

## Deportes

### 41. ¿Cuántos jugadores tiene un equipo de fútbol en el campo?
Dificultad: Facil

- A) 12 [INCORRECTA]
- B) 10 [INCORRECTA]
- C) 9 [INCORRECTA]
- D) 11 [CORRECTA]

### 42. ¿Qué deporte se juega con raqueta y volante?
Dificultad: Facil

- A) Squash [INCORRECTA]
- B) Pádel [INCORRECTA]
- C) Bádminton [CORRECTA]
- D) Tenis [INCORRECTA]

### 43. ¿Cada cuántos años se celebran los Juegos Olímpicos de verano?
Dificultad: Facil

- A) 2 [INCORRECTA]
- B) 4 [CORRECTA]
- C) 5 [INCORRECTA]
- D) 3 [INCORRECTA]

### 44. ¿Qué selección ganó el Mundial de fútbol de 2010?
Dificultad: Facil

- A) Alemania [INCORRECTA]
- B) España [CORRECTA]
- C) Brasil [INCORRECTA]
- D) Países Bajos [INCORRECTA]

### 45. ¿Cuántos puntos vale un tiro libre anotado en baloncesto?
Dificultad: Media

- A) 2 [INCORRECTA]
- B) 1 [CORRECTA]
- C) 3 [INCORRECTA]
- D) 0 [INCORRECTA]

### 46. ¿Qué tenista ha ganado más veces Roland Garros?
Dificultad: Media

- A) Novak Djokovic [INCORRECTA]
- B) Roger Federer [INCORRECTA]
- C) Björn Borg [INCORRECTA]
- D) Rafael Nadal [CORRECTA]

### 47. ¿Cuál es la distancia oficial de un maratón?
Dificultad: Media

- A) 40 km [INCORRECTA]
- B) 45,5 km [INCORRECTA]
- C) 42,195 km [CORRECTA]
- D) 42 km [INCORRECTA]

### 48. ¿En qué ciudad se celebraron los Juegos Olímpicos de 1992?
Dificultad: Media

- A) Madrid [INCORRECTA]
- B) Seúl [INCORRECTA]
- C) Atlanta [INCORRECTA]
- D) Barcelona [CORRECTA]

### 49. ¿Cuántos jugadores por equipo hay en el campo en el rugby a quince?
Dificultad: Dificil

- A) 7 [INCORRECTA]
- B) 13 [INCORRECTA]
- C) 11 [INCORRECTA]
- D) 15 [CORRECTA]

### 50. ¿Cuántos títulos de Grand Slam individuales ganó Rafael Nadal?
Dificultad: Dificil

- A) 24 [INCORRECTA]
- B) 22 [CORRECTA]
- C) 18 [INCORRECTA]
- D) 20 [INCORRECTA]

## Geografía

### 51. ¿Cuál es el océano más grande del mundo?
Dificultad: Facil

- A) Atlántico [INCORRECTA]
- B) Pacífico [CORRECTA]
- C) Ártico [INCORRECTA]
- D) Índico [INCORRECTA]

### 52. ¿Cuál es la capital de Italia?
Dificultad: Facil

- A) Florencia [INCORRECTA]
- B) Milán [INCORRECTA]
- C) Venecia [INCORRECTA]
- D) Roma [CORRECTA]

### 53. ¿En qué continente se encuentra Kenia?
Dificultad: Facil

- A) Oceanía [INCORRECTA]
- B) África [CORRECTA]
- C) Asia [INCORRECTA]
- D) América del Sur [INCORRECTA]

### 54. ¿Cuál es el país más grande del mundo por superficie?
Dificultad: Facil

- A) Canadá [INCORRECTA]
- B) China [INCORRECTA]
- C) Rusia [CORRECTA]
- D) Estados Unidos [INCORRECTA]

### 55. ¿Cuál es la montaña más alta del mundo sobre el nivel del mar?
Dificultad: Media

- A) Everest [CORRECTA]
- B) Kangchenjunga [INCORRECTA]
- C) Aconcagua [INCORRECTA]
- D) K2 [INCORRECTA]

### 56. ¿Cuál es la capital de Canadá?
Dificultad: Media

- A) Montreal [INCORRECTA]
- B) Ottawa [CORRECTA]
- C) Toronto [INCORRECTA]
- D) Vancouver [INCORRECTA]

### 57. ¿Cuál es el desierto cálido más grande del mundo?
Dificultad: Media

- A) Sahara [CORRECTA]
- B) Kalahari [INCORRECTA]
- C) Atacama [INCORRECTA]
- D) Gobi [INCORRECTA]

### 58. ¿Qué río atraviesa la ciudad de Budapest?
Dificultad: Media

- A) Elba [INCORRECTA]
- B) Rin [INCORRECTA]
- C) Danubio [CORRECTA]
- D) Vístula [INCORRECTA]

### 59. ¿Cuál es el lago más profundo del mundo?
Dificultad: Dificil

- A) Titicaca [INCORRECTA]
- B) Baikal [CORRECTA]
- C) Tanganica [INCORRECTA]
- D) Superior [INCORRECTA]

### 60. ¿Cuál es la capital de Nueva Zelanda?
Dificultad: Dificil

- A) Wellington [CORRECTA]
- B) Christchurch [INCORRECTA]
- C) Hamilton [INCORRECTA]
- D) Auckland [INCORRECTA]

## Historia

### 61. ¿En qué año terminó la Segunda Guerra Mundial?
Dificultad: Facil

- A) 1944 [INCORRECTA]
- B) 1939 [INCORRECTA]
- C) 1945 [CORRECTA]
- D) 1950 [INCORRECTA]

### 62. ¿Quién fue el primer ser humano en pisar la Luna?
Dificultad: Facil

- A) Yuri Gagarin [INCORRECTA]
- B) Michael Collins [INCORRECTA]
- C) Neil Armstrong [CORRECTA]
- D) Buzz Aldrin [INCORRECTA]

### 63. ¿En qué año cayó el muro de Berlín?
Dificultad: Facil

- A) 1979 [INCORRECTA]
- B) 1985 [INCORRECTA]
- C) 1991 [INCORRECTA]
- D) 1989 [CORRECTA]

### 64. ¿Qué civilización construyó las pirámides de Guiza?
Dificultad: Facil

- A) Romana [INCORRECTA]
- B) Persa [INCORRECTA]
- C) Griega [INCORRECTA]
- D) Egipcia [CORRECTA]

### 65. ¿En qué año se aprobó la Constitución española vigente?
Dificultad: Media

- A) 1931 [INCORRECTA]
- B) 1981 [INCORRECTA]
- C) 1975 [INCORRECTA]
- D) 1978 [CORRECTA]

### 66. ¿Qué emperador francés fue derrotado en la batalla de Waterloo?
Dificultad: Media

- A) Napoleón III [INCORRECTA]
- B) Napoleón Bonaparte [CORRECTA]
- C) Luis XIV [INCORRECTA]
- D) Carlomagno [INCORRECTA]

### 67. ¿Quiénes fueron conocidos como los Reyes Católicos?
Dificultad: Media

- A) Isabel I de Castilla y Fernando II de Aragón [CORRECTA]
- B) Felipe V y María Luisa de Saboya [INCORRECTA]
- C) Felipe II e Isabel de Valois [INCORRECTA]
- D) Carlos I e Isabel de Portugal [INCORRECTA]

### 68. ¿En qué año comenzó la Revolución francesa?
Dificultad: Media

- A) 1789 [CORRECTA]
- B) 1776 [INCORRECTA]
- C) 1815 [INCORRECTA]
- D) 1799 [INCORRECTA]

### 69. ¿Qué batalla naval de 1571 enfrentó a la Liga Santa contra el Imperio otomano?
Dificultad: Dificil

- A) Batalla de Pavía [INCORRECTA]
- B) Batalla de Lepanto [CORRECTA]
- C) Batalla de Rocroi [INCORRECTA]
- D) Batalla de Trafalgar [INCORRECTA]

### 70. ¿Quién fue el primer emperador de Roma?
Dificultad: Dificil

- A) Augusto [CORRECTA]
- B) Julio César [INCORRECTA]
- C) Trajano [INCORRECTA]
- D) Nerón [INCORRECTA]

## Música

### 71. ¿Cuántas cuerdas tiene una guitarra española estándar?
Dificultad: Facil

- A) 6 [CORRECTA]
- B) 7 [INCORRECTA]
- C) 5 [INCORRECTA]
- D) 4 [INCORRECTA]

### 72. ¿De qué país era el grupo The Beatles?
Dificultad: Facil

- A) Estados Unidos [INCORRECTA]
- B) Irlanda [INCORRECTA]
- C) Reino Unido [CORRECTA]
- D) Australia [INCORRECTA]

### 73. ¿Qué compositor alemán siguió componiendo a pesar de quedarse sordo?
Dificultad: Facil

- A) Johannes Brahms [INCORRECTA]
- B) Johann Sebastian Bach [INCORRECTA]
- C) Ludwig van Beethoven [CORRECTA]
- D) Richard Wagner [INCORRECTA]

### 74. ¿Cuántas notas tiene la escala musical diatónica?
Dificultad: Facil

- A) 5 [INCORRECTA]
- B) 8 [INCORRECTA]
- C) 7 [CORRECTA]
- D) 12 [INCORRECTA]

### 75. ¿Qué cantante es conocido como el «Rey del Pop»?
Dificultad: Media

- A) Prince [INCORRECTA]
- B) Freddie Mercury [INCORRECTA]
- C) Michael Jackson [CORRECTA]
- D) Elvis Presley [INCORRECTA]

### 76. ¿Qué compositor escribió «Las cuatro estaciones»?
Dificultad: Media

- A) Joseph Haydn [INCORRECTA]
- B) Antonio Vivaldi [CORRECTA]
- C) Georg Friedrich Händel [INCORRECTA]
- D) Wolfgang Amadeus Mozart [INCORRECTA]

### 77. ¿Cuántas teclas tiene un piano estándar?
Dificultad: Media

- A) 96 [INCORRECTA]
- B) 88 [CORRECTA]
- C) 72 [INCORRECTA]
- D) 76 [INCORRECTA]

### 78. ¿En qué país nació el tango?
Dificultad: Media

- A) Argentina [CORRECTA]
- B) Brasil [INCORRECTA]
- C) Cuba [INCORRECTA]
- D) España [INCORRECTA]

### 79. ¿Qué compositor español escribió el «Concierto de Aranjuez»?
Dificultad: Dificil

- A) Joaquín Rodrigo [CORRECTA]
- B) Enrique Granados [INCORRECTA]
- C) Manuel de Falla [INCORRECTA]
- D) Isaac Albéniz [INCORRECTA]

### 80. ¿Qué ópera de Georges Bizet está ambientada en Sevilla?
Dificultad: Dificil

- A) La traviata [INCORRECTA]
- B) Tosca [INCORRECTA]
- C) Carmen [CORRECTA]
- D) Aida [INCORRECTA]

## Naturaleza

### 81. ¿Cuál es el animal terrestre más rápido?
Dificultad: Facil

- A) Caballo [INCORRECTA]
- B) Antílope [INCORRECTA]
- C) Guepardo [CORRECTA]
- D) León [INCORRECTA]

### 82. ¿Cuál es el animal más grande del mundo?
Dificultad: Facil

- A) Cachalote [INCORRECTA]
- B) Elefante africano [INCORRECTA]
- C) Ballena azul [CORRECTA]
- D) Tiburón ballena [INCORRECTA]

### 83. ¿Cuántas patas tiene una araña?
Dificultad: Facil

- A) 8 [CORRECTA]
- B) 12 [INCORRECTA]
- C) 10 [INCORRECTA]
- D) 6 [INCORRECTA]

### 84. ¿Qué animal es conocido como el rey de la selva?
Dificultad: Facil

- A) Gorila [INCORRECTA]
- B) Tigre [INCORRECTA]
- C) León [CORRECTA]
- D) Jaguar [INCORRECTA]

### 85. ¿Cómo se llama el proceso por el que una oruga se convierte en mariposa?
Dificultad: Media

- A) Metamorfosis [CORRECTA]
- B) Mitosis [INCORRECTA]
- C) Fotosíntesis [INCORRECTA]
- D) Ósmosis [INCORRECTA]

### 86. ¿Qué árbol produce bellotas?
Dificultad: Media

- A) Pino [INCORRECTA]
- B) Olivo [INCORRECTA]
- C) Castaño [INCORRECTA]
- D) Encina [CORRECTA]

### 87. ¿Cuál es el ave más grande del mundo?
Dificultad: Media

- A) Emú [INCORRECTA]
- B) Cóndor andino [INCORRECTA]
- C) Avestruz [CORRECTA]
- D) Albatros [INCORRECTA]

### 88. ¿Cuál de estos animales es un marsupial?
Dificultad: Media

- A) Murciélago [INCORRECTA]
- B) Nutria [INCORRECTA]
- C) Canguro [CORRECTA]
- D) Mapache [INCORRECTA]

### 89. ¿Cuántos corazones tiene un pulpo?
Dificultad: Dificil

- A) 4 [INCORRECTA]
- B) 3 [CORRECTA]
- C) 2 [INCORRECTA]
- D) 1 [INCORRECTA]

### 90. ¿A qué grupo de animales pertenece el ajolote?
Dificultad: Dificil

- A) Peces [INCORRECTA]
- B) Anfibios [CORRECTA]
- C) Mamíferos [INCORRECTA]
- D) Reptiles [INCORRECTA]

## Tecnología y videojuegos

### 91. ¿Qué significan las siglas CPU?
Dificultad: Facil

- A) Procesador de usuario central [INCORRECTA]
- B) Unidad de control personal [INCORRECTA]
- C) Unidad de cálculo programable [INCORRECTA]
- D) Unidad central de procesamiento [CORRECTA]

### 92. ¿Qué empresa creó el sistema operativo Windows?
Dificultad: Facil

- A) Google [INCORRECTA]
- B) Apple [INCORRECTA]
- C) IBM [INCORRECTA]
- D) Microsoft [CORRECTA]

### 93. ¿Cómo se llama el fontanero protagonista de los juegos de plataformas de Nintendo?
Dificultad: Facil

- A) Mario [CORRECTA]
- B) Link [INCORRECTA]
- C) Luigi [INCORRECTA]
- D) Wario [INCORRECTA]

### 94. ¿Qué empresa fabrica la consola PlayStation?
Dificultad: Facil

- A) Sony [CORRECTA]
- B) Nintendo [INCORRECTA]
- C) Microsoft [INCORRECTA]
- D) Sega [INCORRECTA]

### 95. ¿Qué lenguaje se usa principalmente para dar estilo a las páginas web?
Dificultad: Media

- A) Python [INCORRECTA]
- B) CSS [CORRECTA]
- C) HTML [INCORRECTA]
- D) SQL [INCORRECTA]

### 96. ¿En qué año se presentó el primer iPhone?
Dificultad: Media

- A) 2005 [INCORRECTA]
- B) 2010 [INCORRECTA]
- C) 2007 [CORRECTA]
- D) 2009 [INCORRECTA]

### 97. ¿Qué videojuego de construcción con bloques creó Markus «Notch» Persson?
Dificultad: Media

- A) Fortnite [INCORRECTA]
- B) Minecraft [CORRECTA]
- C) Terraria [INCORRECTA]
- D) Roblox [INCORRECTA]

### 98. ¿Qué protocolo permite navegar por la web de forma cifrada?
Dificultad: Media

- A) HTTPS [CORRECTA]
- B) HTTP [INCORRECTA]
- C) FTP [INCORRECTA]
- D) SMTP [INCORRECTA]

### 99. ¿Quién es considerada la primera programadora de la historia?
Dificultad: Dificil

- A) Ada Lovelace [CORRECTA]
- B) Hedy Lamarr [INCORRECTA]
- C) Margaret Hamilton [INCORRECTA]
- D) Grace Hopper [INCORRECTA]

### 100. ¿Qué número decimal corresponde al binario 1010?
Dificultad: Dificil

- A) 8 [INCORRECTA]
- B) 5 [INCORRECTA]
- C) 12 [INCORRECTA]
- D) 10 [CORRECTA]

-> Day_One

VAR is_angry = false

=== Day_One ===
-> start

= start
"¡Hola, Magetender!"
"Soy yo, el unicornio más exquisito del pueblo, Anita Caberneigh."
* "eh... ¿quién?"
    "¿Cómo? ¿No has oído hablar de mí?"
    "Dios mío, parece que me he topado con un mago que vive bajo una roca."
    "De verdad deberías salir más..."
    ** "Eso es grosero"
        "¿Grosero? Confundes mi confianza con descaro, cariño. Verás, la primera se gana."
        "Pero basta de perder el tiempo, me gustaría pedir..."
        *** "Tienes razón, perdón"
            "Me gustaría pedir una poción que me haga sentir con los pies en la tierra y en calma."
            "Mantener la perfección es... agotador, lo entiendes."
            "Ah, y por favor... no olvides una <b>cereza encima</b>."
        *** "Un por favor estaría bien..."
            "¿Oh...? Qué fascinante."
            ~ is_angry = true
            "¡UN MAGETENDER QUIERE DARME LECCIONES DE ETIQUETA!"
            "DIME, ¿TAMBIÉN ANDAS DANDO LECCIONES A LA REALEZA SOBRE CÓMO RESPIRAR?"
    ** "¿En qué puedo ayudarte?"
        "Me gustaría pedir una poción que me haga sentir con los pies en la tierra y en calma."
        "Mantener la perfección es... agotador, lo entiendes."
        "No olvides una <b>cereza encima</b>."
* "Encantado de servirte"
    "Sí, deberías considerar esto un honor poco común."
    "En fin, me gustaría pedir una poción que me haga sentir con los pies en la tierra y en calma."
    "Mantener la perfección es... agotador, lo entiendes."
    "No olvides una <b>cereza encima</b>."

- -> END

= neutral
"Hmm. No está horrible... aunque parece que te falta un conocimiento excepcional de magetendería."
"Solo puedo suponer que te dieron este puesto por nepotismo... o por una recomendación interna. ¿Me acerco?"
"Hasta la próxima."

- -> END

= satisfied
"Ah, sí... esta bebida me hace sentir tan... tranquila."
"Como si bailara sobre arcoíris mientras las hadas levantan mi fabulosa melena."
"¿P-por qué estaba tan alterada siquiera... acepta mi generosa propina como símbolo de mi gratitud."
"Hasta la próxima, Magetender."

- -> END

= angry
"¡¿QU-QUÉ DEMONIOS ES ESTO?!"
"¡¡¡MI PRESIÓN ARTERIAL SOLO HA SUBIDO MÁS!!!"
"¡MAGETENDER, TU INCOMPETENCIA NO QUEDARÁ IMPUNE!!!"

- -> END

=== Day_Two ===
-> start

= start
"Hola, Magetender. Soy yo, otra vez. El unicornio más exquisito del pueblo, Anita Caberneigh."
"Pero en serio, ya no necesito presentación... calla... sabes que es verdad."
* "¿Me lo recuerdas?"
    "Seguro que bromeas... Nadie simplemente 'olvida' quién es Anita..."
    "Mi popularidad supera a todas las demás. Muchos me adoran... y el resto me envidia."
    "Quiero una bebida que me dé calma y compostura."
    "Asegúrate de que esté terminada <b>con una cereza encima.</b>"
    "Ahora... quizá una reverencia sería apropiada..."
    ** "*Hacer una reverencia*"
        "Hmm. Aceptable."
    ** "*Negarse a reverenciar*"
        "Oh, qué audaz. Qué... lamentable."
        ~ is_angry = true
        "NO LLEVO BIEN LAS FALTAS DE ETIQUETA ADECUADA, MAGETENDER"
* "Por supuesto, ¿cómo iba a olvidarte?"
    "Como era de esperar. El reconocimiento te sienta bien."
    "Quiero una bebida que me dé calma y compostura."
    "Asegúrate de que esté terminada <b>con una cereza encima.</b>"
    "Ahora... quizá una reverencia sería apropiada..."
    ** "*Hacer una reverencia*"
        "Hmm. Aceptable."
    ** "*Negarse a reverenciar*"
        "Oh, qué audaz. Qué... lamentable."
        ~ is_angry = true
        "NO LLEVO BIEN LAS FALTAS DE ETIQUETA ADECUADA, MAGETENDER"

- -> END

= neutral
"Hmm. Esto ha hecho poco por mejorar mi humor."
"Hay cierta... falta de refinamiento aquí."
"Intenta elevar tu oficio antes de nuestro próximo encuentro."
"Adiós."

- -> END

= satisfied
"Ah... sí. Esto es bastante encantador."
"Me siento positivamente serena... como si el mundo mismo se hubiera suavizado."
"Quizá te juzgué mal... solo un poco."
"No te confíes. Pero... bien hecho, Magetender."

- -> END

= angry
"¿¿QUÉ es esto??!"
"¡Esto es una vergüenza total!"
"¡Me has arruinado el humor por completo!"
"¡Responderás por esta incompetencia!"

- -> END

=== Day_Three ===
-> start

= start
-> Day_Two.start

= neutral
-> Day_Two.neutral

= satisfied
-> Day_Two.satisfied

= angry
-> Day_Two.angry

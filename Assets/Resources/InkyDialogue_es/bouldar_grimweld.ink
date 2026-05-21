VAR bouldar_mood = 0
VAR is_angry = false

-> Day_One

=== Day_One ===
-> start

= start
"*HHHRRRHHH* Puerta pequeña estúpida..."
"¡Magtender! Bebida, ya. Engranajes queman, quiero bebida que los enfríe."

* "Un por favor puede ayudar mucho."
	~ bouldar_mood--
	"*HRRRRRR* Dije bebida. <i>Ahora</i>. Repetir hace Bouldar enfadado. ¡Tú no quieres Bouldar Grimweld enfadado!"

	** "No deberías amenazar a trabajadores. Ya no nos pagan bastante."
		~ bouldar_mood--
		"Bouldar no importa. ¡Bouldar <i>necesita</i> bebida ya! O Bouldar aplasta tu cara."

		*** "Eso es demasiado agresivo."
			~ bouldar_mood--
			~ is_angry = true
			"¿Agresivo? ¡TÚ AGRESIVO! ¡Tú te burlas de Bouldar!"
			"Tú eres malo, no quiero bebida de magetender malo. *KRRR GRRAAA*"

		*** "Solo bromeaba, perdón."
			~ bouldar_mood++
			"No... Bouldar también perdón. Engranajes de Bouldar duelen. Como fuego, queman piedra."
			"Solo quiero algo que calme. Quiero estar tranquilo, ¿entiendes?"

			**** "Entiendo. Tengo justo lo que necesitas."
				"Bouldar lo aprecia, y espera la bebida."

	** "Vale, perdón. Solo bromeaba."
		~ bouldar_mood++
		"*HHRRK* Bouldar perdón, Bouldar dice por favor. Solo molesto, articulaciones y engranajes duelen. Fusibles fundidos."
		"No puede controlarse... quiere estar tranquilo, no con los pies en la tierra."

		*** "Bueno, tú <i>estás</i> hecho de piedra. No sé si puedes ser otra cosa <i>que</i> con los pies en la tierra."
			~ bouldar_mood--
			"*KRRRR* haces demasiadas bromas. Enfócate en la bebida. Tus bromas ni siquiera son graciosas."

			**** "Ay... al menos lo intenté."
				"Solo haz bebida, no más bromas. Bromas malas."

		*** "Suena bien. Te preparo algo bueno."
			~ bouldar_mood++
			"Gracias magetender. No muchos sitios me sirven. Demasiado grande, demasiado enfadado. Espero que sea buena, y me ayude a arreglarme."

* "¿Tus engranajes?"
	~ bouldar_mood++
	"Sí. Engranajes duelen y crujen. Se rozan y queman, como roca fundida por dentro."
	"Duele moverse. Hace Bouldar enfadado."

	** "Siento oír eso."
		~ bouldar_mood++
		"Es primera vez que alguien dice eso a mí. Otras tabernas ni toman pedido, solo me echan."
		"Solo quiero estar tranquilo, y no tan enfadado."

		*** "Puedo con eso."
			"Gracias magetender, eres amable."

	** "¿Y una bebida es la cura?"
		~ bouldar_mood--
		"¿Tú dudas de mí magetender?! ¡Estoy aquí por bebida! ¡No por juicio!"

		*** "Tienes razón, perdón. Solo intentaba aligerar el ambiente."
			~ bouldar_mood++
			"Ya veo.... Bouldar perdón por gritar. A Bouldar no le gusta que lo juzguen, porque Bouldar no puede controlarlo."
			"Solo intento vivir cómodo."

			**** "Claro. Te preparo esa bebida."
				"¡Bouldar la espera con ganas!"

		*** "Solo bromeaba."
			"¡Broma no graciosa! ¡Dolor de Bouldar es real! Solo quiero bebida que ayude, tú te burlas de mí..."

			**** "No quise herirte."
				~ bouldar_mood++
				"*KHHH* Está bien. Solo consigue bebida a Bouldar, me siento mejor después."

			**** "De verdad no, pero vale."
				~ bouldar_mood--
				~ is_angry = true
				"*KRRR GRAAA* ¿POR QUÉ MALO? ¿POR QUÉ MAGETENDER MALO? ¡TÚ COMO TODOS LOS QUE JUZGAN A BOULDAR!"
				"¡NO QUIERO BEBIDA DE TI!"

- -> END

= satisfied
"*HHSSSS* ¿Oyes eso magetender?! Ese sonido es vapor de mis engranajes. ¡Alivio! ¡Dulce alivio!"
"Tienes mi gratitud. Dolió tanto tiempo, ¡quizá vuelva cada día!"
"Ahora mi hijo puede sentarse cómodo en mis hombros."
"¡Adelante Chip, agradece al magetender!"
"..."
"¡Bien dicho! Entonces, quizá nos veamos otra vez. ¡Adiós por ahora!~"

- -> END

= neutral
"*CLANK* *CLICK* ¿Oyes eso magetender? Esos son mis engranajes. Siguen crujiendo y trabándose."
"El calor persiste, aunque un poco desvanecido."
"Desafortunado... esperaba alivio. Pensé que me arreglabas. En cambio, sigo roto."
"Ven Chip, nos vamos a casa. Papá encontrará otra forma."
"Gracias por intentarlo, magetender."

- -> END

= angry
"*GRRAAA* ¿QUÉ ES ESTA MONSTRUOSIDAD?! PEDÍ SENSACIÓN CALMANTE. TÚ TRAES DOLOR."
"¡MIS ENGRANAJES, MI NÚCLEO, ARDEN! ¿QUÉ HAS HECHO?!"
"¡PAGARÁS POR ESTO, MIS FUSIBLES SE HAN FUNDIDO! ¡TE APLASTARÉ!!!"

- -> END

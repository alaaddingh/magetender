-> Day_One

VAR is_angry = false
VAR show_license = false

=== Day_One ===
-> start

= start
"¡Hola, colega adulto! ¡Solo una bebida, por favor! jejeje..."
"¡Solo la... eh... DE SIEMPRE!"
"<b>¡¡¡BZZZZOHH!!!</b> ¿No pareces emocionado?"
"..."
* "¿Cuántos años tienes?"
	"¿Qué? Lo bastante mayor para pilotar una nave espacial, obviamente..."
	"¡Tengo veinte mil millones y pico!"
	"¿Qué, quieres una identificación? Bien. Regálate los ojos terrestres con mi Licencia Galáctica de Adulto."
	~ show_license = true
	"Como ves, está laminada... lo que la hace MUY oficial..."

	** "Parece legítima. Adelante."
		"Excelente juicio, terrícola."
		"De todos modos, necesito una buena bebida, con los pies en la tierra pero elevada."
		"Quiero sentirme un poco 'con los pies en la Tierra' entre los dos. Jeje... ¿ves el juego de palabras?"
		"Y no olvides coronarla con <b> un mini paraguas!</b>"
	** "Esto no parece legítimo."
		~ is_angry = true
		"<b>¡¡¡BZZZZOHH!!!</b> ¿¡QUÉ?!"
		"¡ESTO ES EDADISMO INTERGALÁCTICO, TERRÍCOLA! ¡PREPÁRATE PARA SER ZOINKED!"
* "¿Qué te preparo?"
	"Me gustaría una buena bebida, con los pies en la tierra pero elevadora..."
	"Quiero sentirme un poco más 'con los pies en la Tierra' entre los dos. Jeje... ¿ves el juego de palabras?"
	"Y no olvides coronarla con <b> un mini paraguas!</b>"

- -> END

= neutral
"Eh... esta bebida no captura del todo mis sentidos... está bien, supongo."
"No te sientas mal, capturar terrícolas es más fácil que capturar el equilibrio correcto del sabor."

- -> END

= satisfied
"¡ZOINKS! Esto es increíble... ¡esto es lo bueno!"
"Definitivamente me siento un poco más 'con los pies en la tierra'. ¡Bien hecho!"

- -> END

= angry
"¿¡QUÉ EN LA VÍA LÁCTEA ME PREPARASTE?!"
"¡¡¡ESTO SABE A ESOS CONTAMINANTES DE CARBONO QUE USTEDES LOS TERRÍCOLAS ADORAN EXPULSAR!!"
"¡¡¡DUELEMOS AHORA MISMO!!! ¡¡¡APRENDERÁS POR QUÉ ME LLAMAN UN ZORP DE VERDAD DESPUÉS DE ZOINKARTE!!!"

- -> END

=== Day_Two ===
-> start

= start
"¡WAZZZZ qué tal, terrícola! ¡Ojalá tu día sea tan zorp-tástico como el mío!"
"..."
* "Sigues pareciendo un poco joven"
	"¡JA! Error clásico de terrícola."
	"Mi especie envejece distinto. Muy distinto. De hecho, soy, como... antiguo."
	"¡veinte mil millones y pico, para ser exactos!"
	"Mira mi Licencia Galáctica de Madurez. Quiero decir, ¡mira esa laminación! Muy oficial..."
	** "Vale. Estás bien."
		"Excelente..."
		"En fin, ¿puedes hacerme una bebida con los pies en la tierra pero elevadora?"
		"¿Con un <b>paraguas encima</b> de mi poción?"
		"¡Algo que dispare mi mente zorp a plena levitación!"
	** "Buena falsificación. Nada de bebida."
		~ is_angry = true
		"¡¡¡BZZZZOHH!!! ¿¡RECHAZAS A UN ANCIANO INTERGALÁCTICO?!"
		"¡DEBERÍA DENUNCIAR ESTO AL CONSEJO CÓSMICO DE INMEDIATO!"
		"¡PREPÁRATE PARA UNA QUEJA FORMAL Y UN ZOINKING APROPIADO!"
* "¿Qué te apetece?"
	"¿Puedes hacerme una bebida con los pies en la tierra pero elevadora?"
	"¿Con un <b>paraguas encima</b> de mi poción?"

- -> END

= neutral
"Eh... esta bebida no captura del todo lo que busco."
"Mordisquear cuernos de unicornio me da más zoomies que esta... bebida..."
"Espero una mejora cuando vuelva, pequeño terrícola."

- -> END

= satisfied
"¡ZOINKS! ¡Esto me da los zoomies que necesitaba!"
"¡Siento que estoy levitando en un planeta de ensueño!"
"Me has servido bien, Magetender. Hasta la próxima."

- -> END

= angry
"¿¡QUÉ EN LA VÍA LÁCTEA ES ESTO?! ¡¡¡ESTO SABE A BASURA ESPACIAL!!!"
"¡PREPÁRATE PARA SER ZOINKED POR ZIP ZORP!!!"

- -> END



=== Day_Three ===
-> start

= start
"¡BZZOOHHHH! Hola, terrícola..."
"Buenas noticias."
"Oficialmente HE TERMINADO de estar 'con los pies en la Tierra'."
"Extraño la sensación de estar en órbita... ahh, la nostalgia."

* "Pareces menos enérgico hoy."
	"No pensé que lo notarías..."
	"La gravedad de tu planeta me está pasando factura. ¡Quiero volver a la órbita!"
	"Así que hoy..."
	"Quiero una bebida MUCHO más elevadora."
	"Como si volviera a pilotar una nave espacial..."
	"Por favor no olvides ese <b>mini paraguas.</b>"

* "¿Tú otra vez?"
	"..."
	"¿Qué se supone que significa eso?"
	"Estaría tejiendo entre cinturones de asteroides ahora mismo si tus bebidas calmantes no fueran <i>súper</i> efectivas."
	"¡ANSÍO la órbita, Magetender. ¡¡ANSÍO LA ÓRBITA!! <b>¡BZZOOHHHH!</b>"

	** "Pareces loco."
	    ~ is_angry = true
		"¡¡¡BZZZZOHH!!!"
		"¿¿¿LOCO???"
		"¡ESA OBSERVACIÓN ASTUTA SÍ QUE ME ELEVA..."
		"¡ELEVA MI PRESIÓN SANGUÍNEA ZORPIANA!"

	** "¿Qué te preparo?"
		"Así que hoy..."
		"Quiero una bebida MUCHO más elevadora."
		"Como si volviera a pilotar una nave espacial..."
		"Por favor no olvides ese <b>mini paraguas.</b>"

- -> END

= neutral
"bueno... esta bebida no me eleva el ánimo <i> del todo </i>..."
"Creo que pasará un tiempo hasta que tenga la energía adecuada para volver a la órbita..."

- -> END

= satisfied
"¡ZOINKS! ¡Esto sí que me eleva!"
"Con tu ayuda, creo que pronto saldré zumbando de este planeta sin problema..."

- -> END

= angry
"¿¡QUÉ EN LA VÍA LÁCTEA ES ESTO?! ¡¡¡ESTO SABE A BASURA ESPACIAL!!!"
"¡PREPÁRATE PARA SER ZOINKED POR ZIP ZORP!!!"
- -> END

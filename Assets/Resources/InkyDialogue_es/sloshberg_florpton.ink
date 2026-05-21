-> Day_One

VAR is_angry = false

=== Day_One ===
-> start

= start
"<b>*GLAURGHH* *GLAURGHHHHHHHH*</b>"
	"Ese es mi saludo... ¿te gusta? ¿O fue <b>*GLAURGH*</b>-bage?"
* "Me gusta el saludo."
	"Jejeje... bueno..."
	"Vi una reseña de NobleChadwick33 en Kelp. Dijo que le hiciste una bebida tan buena que dejó su trabajo de oficina."
	"...para dedicarse al malabarismo. Respeto. Yo podría usar ese tipo de ánimo."
	"Llevo como una semana entera haciendo doomscrolling sin parar..."
	"Me siento bastante lento... bastante baboso..."
	"¿Podrías hacerme una bebida que me ayude con la letargia... <b>*GLAURGH*</b>"
	"Y quizá echarme una <b>rodaja de lima...</b>"
* "Estoy confundido..."
	"Vamos, hombre. el nombre es Sloshberg Florpton, ¿esperabas un saludo normal?"
	** "¿Qué te preparo?"
		"Vi una reseña de NobleChadwick33 en Kelp. Dijo que le hiciste una bebida tan buena que dejó su trabajo de oficina..."
		"...para dedicarse al malabarismo. Respeto. Yo podría usar ese tipo de ánimo."
		"Llevo como una semana entera haciendo doomscrolling sin parar..."
		"Me siento bastante lento... bastante baboso..."
		"¿Podrías hacerme una bebida que me ayude con la letargia... <b>*GLAURGH*</b>?"
		"Y quizá echarme una <b>rodaja de lima...</b>"
	** "Fue raro."
		"<b>*GLOARGH enfadado*</b> Ohohh... duro..."
		~ is_angry = true
		"¡<b>*GLAURGH*</b> me libre de intentar algo original!"
		"¡Parece que esas reseñas de Kelp eran pura babosada!"

- -> END

= neutral
"<b>*GLOURGH*</b> Esta bebida está bien... no ha levantado mucho mi letargia..."
"Yo... algo cuestiono tus habilidades de hacer pociones <b>*GLOURGH*</b>, amigo..."

- -> END

= satisfied
"<b>*GLORGH feliz*</b> ¡UAU! Me siento INCREÍBLE... no creo haberme sentido así desde que era un glóbulo pequeño..."
"Mi cuerpo de gelatina se siente revitalizado... ¡tengo justo energía para darte una propina enorme!"
"¡Voy a invitar a todos mis blobs a venir a devorar aquí! ¡Gracias, Magetender!"

- -> END

= angry
"<b>*GLOARGH enfadado*</b> ¿¡QUÉ DEMONIOS DE LODO ES ESTO?!"
"¿¡CREES QUE SOY ALGÚN TIPO DE CONTENEDOR DE BABOSAS, EH?!"
"¡TE VOY A DAR UN <b>*GLOARGH*</b> BRUTAL QUE RECORDARÁS!!!"

- -> END

=== Day_Two ===
-> start

= start
"<b>*GLAURGHH* *GLAURGHHHHHHHH*</b>"
"..."
"Necesito tu ayuda..."
"Mi <b>*GLAURGHH*</b> está demasiado alto después de una Noche de Blobs..."
"Todo se siente raro... demasiado baboso..."
"¿Podrías hacerme algo para bajarme?"
"¿Todavía haces buenas bebidas?"
* "Sí. Te cubro."
	"<b>*glorgh...*</b> Gracias... necesito el reinicio..."
	"Por favor no olvides esa <b>rodaja de lima...</b>"
* "Depende. ¿Qué tal estuvo la Noche de Blobs?"
	"Digamos que yo y todos mis blobs compañeros despertamos en una zanja..."
	** "suena duro."
		"Sí... exacto... gracias por no juzgar..."
	** "Suena como culpa tuya."
		~ is_angry = true
		"<b>*GLOARGH enfadado*</b> UAU... vale..."
		"Vengo aquí necesitando sentirme mejor, y ahora solo me siento <b>*GLOURGH*</b> juzgado..."

- -> END

= neutral
"<b>*GLOURGH*</b> Esta bebida no me hace sentir menos lento..."
"Creo que vi basura sabrosa detrás de la taberna..."
"Sí... eso me subirá el ánimo."

- -> END

= satisfied
"<b>*GLORGH feliz*</b> ¡UAU! ¡Mi naturaleza ondulante ha vuelto!"
"¡Gracias otra vez, Magetender! ¡Pronto traeré a mis blobs aquí!"

- -> END

= angry
"<b>*GLOARGH enfadado*</b> ¡ESTA POCIÓN ES PURO SLOGFLORP DIRECTO!"
"¡NO SOY UN CONTENEDOR DE COMPOST PARA QUE LO USES!!!"

- -> END

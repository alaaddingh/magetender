-> Day_Three


VAR knight_mood = 0
VAR allies = false
VAR enemies = false
VAR left_without_drink = false
VAR is_angry = false




=== Day_Three ===
-> start

= start
"¡Saludos, magetender!"
"Debo admitir que no estoy aquí por una bebida, sino más bien, para evitar <i>sus</i> miradas indiscretas."
"Así que no me prestes atención, simplemente usaré tu techo como refugio."


* "¿Quiénes son 'ellos'?"
    "¿Por qué debes saberlo?"
    "¿Estás con <i>ellos</i>? ¿Intentando extraerme información?"
    
    ** "¡Por supuesto que no! Solo intento ganarme la vida."
    ~ knight_mood++
    "Ya veo... fue un error mío. Qué poco decoroso de mi parte. Verás..."
    "<i>Ellos</i> me han seguido durante bastante tiempo. No puedo probarlo, pero sé que están aquí en alguna parte..."
    "...observando."
    
        *** "Te creo."
        ~ knight_mood++
        ~ allies = true
        "¿De verdad? Hum... nadie me lo había dicho antes. ¿Significa eso que somos... aliados?"
        "En ese caso... consígueme una bebida, amable señor. ¡Celebraremos nuestra nueva amistad con una ronda de licores!"
        "Pero que sea suave, ¡algo que mantenga mis sentidos agudos!"
        // Goes to drink making
        
        *** "¿Qué tal una bebida? Quizá alivie un poco tus preocupaciones."
        "¿Una bebida? No sé... no querría perder de vista mi objetivo."
        
            **** "Puedo ayudarte con eso. Prepararé algo para que mantengas el foco."
            "¿Ah, puedes hacer eso? Sería muy apreciado, amable señor. Esperaré con ansias tu creación."
            //Goes to drink making
    
    
    ** "Otra vez, ¿quiénes son <i>ellos</i>?!"
    ~ knight_mood--
    "No me mientas, magetender. ¡Conozco los ojos de un mentiroso cuando los veo!"
    "¿Es esta taberna tu base secreta? ¿Es eso? ¡Parece que he caído en la guarida del león!"
    
        *** "¡Para nada! Solo soy un humilde magetender."
        ~ knight_mood++
        "¿Es así? Bueno, no creas que no te tendré vigilado..."
        "...pero quizá, si pudiera conseguir una bebida... lo agradecería mucho. Algo que me mantenga concentrado."
        // goes to drink making
        
    
        *** "Por favor, pide algo. Si no, tendré que pedirte que te vayas. No puedes quedarte si no eres cliente."
        "Oh...<i>ejem</i>. Supongo que podría tomar algo si es así. Ehh..."
        "...qué vergonzoso. Algo suave, por favor, debo mantenerme alerta."
        //Goes to drink making


* "Tendré que pedirte que te vayas, si no eres un cliente que paga."
    "¿Es así? Hmm, muy lamentable. Verás, no confío en que no echarías algo en mi bebida."
    "Si trabajas para <i>ellos</i>, sería fácil envenenarme..."
    
    ** "No me des ideas."
    ~ knight_mood--
    "¿Y qué se supone que significa eso?! ¿Estás admitiendo tu lealtad a mis enemigos?!"
    
        *** "Estaba bromeando."
        "¡No es momento de bromear! ¡Un caballero de la ciudad está siendo amenazado y te atreves a reírte?! ¡Nunca vuelvas a hacer tal cosa en mi presencia!"
        
            **** "Entonces... ¿vas a pedir?"
            ~ knight_mood--
            ~ left_without_drink = true
            "¡Ja-ja! ¿Después de tal espectáculo? ¿Por qué debería? ¡Qué servicio tan horrible! ¡Este lugar es una vergüenza!"
            //leaves, no fight (unless players trigger it), and no drink. No money.
            
            **** "Vale, no lo haré."
            ~ knight_mood++
            "¡Bien! Ahora... si hace falta una bebida para quedarme, supongo que tomaré una. Pero que sea suave, debo mantenerme concentrado y alerta. El enemigo podría estar en cualquier parte..."
            // goes to drink making
    
    
    
        *** "¡Sí, lo soy!"
        ~ knight_mood--
        ~ enemies = true
        "...no. Solo intento ganarme la vida."
        "¡Aliándote con mi enemigo! No puedes engañarme, <i>mero</i> tabernero. Usas este lugar para recoger información sobre posibles objetivos."
        "¡Haré a la ciudad un favor eliminando a alguien tan deshonroso como tú!"
        ~ is_angry = true
        // fight starts
    
    
    ** "¿Por qué haría eso?"
    ~ knight_mood++
    "Bueno yo... p-porque estás con <i>ellos</i>. ¡Y desean verme perecer!"
    
        *** "No deseo ver eso."
        ~ knight_mood++
        "Oh... bueno..."
        "...ejem. Mis disculpas. Parece que... puede que te haya juzgado mal."
        "De hecho, ¡déjame pedir una de tus mejores bebidas! Te he tenido atado hablando conmigo demasiado tiempo."
        "Solo que sea sencilla, algo que no embotará mis sentidos."
        // goes to drink making
        
        
        *** "No tienes pruebas."
        ~ knight_mood--
        "¡No necesito pruebas! Pero si es prueba lo que deseas, ¡entonces hazme una bebida!"
        "¡Si está envenenada, ahí tienes tu prueba!"
        
            **** "Pero entonces estarías muerto. Entonces tu plan se va al traste."
            ~ left_without_drink = true
            "Hum... supongo que tienes razón."
            "Bueno... ¡está bien! ¡Olvídalo, si no quieres mi patrocinio, me iré!"
            //leaves, no fight (unless players trigger it), and no drink. No money.

            **** "Claro, vayamos con eso."
            "¡Con eso iremos en efecto! ¡Muéstrame de qué lado estás de verdad, magetender!"
            // goes to drink making

- -> END



= satisfied
"¡Ah! ¡Esto es perfecto! El tipo de bebida que me mantendrá en guardia, listo para cualquier enemigo que enfrente."
"Debo aplaudirte, magetender, tenía mis dudas. Pero esta es una bebida excelente."
"Te has ganado mi confianza, por ahora..."
"Volveré, la próxima vez espero que podamos profundizar nuestra relación..."
"¡...en alianza, por supuesto! ¡Adiós por ahora!"

-> END



= neutral
"Hmm, qué...<i>interesante</i>."
"No es exactamente la bebida que buscaba."
"Es bastante lamentable, pero no te culpo."
"Debe ser difícil trabajar bajo la mirada vigilante de <i>ellos</i>."
"Volveré, hablemos de esto de nuevo algún día."
"No te dejaré controlado por tal mal."

-> END




= angry
"¿QUÉ SIGNIFICA ESTO?!"
"¿De verdad intentaste envenenarme?!"
"¡Qué vergüenza! ¡No tienes honor si así tratas a un caballero de la ciudad!"
"¡Volverás a oír hablar de esto, cerraré este lugar si hace falta!"
"¡Por el bien de esta ciudad y su gente, prepárate para defender tu honor!"

-> END

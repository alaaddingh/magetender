VAR bouldar_mood = 0


-> Day_One


=== Day_One ===
-> Intro

= Intro
"*HHHRRRHHH* Stupid tiny door..."
"Magtender! Drink, now. My gears burn, want drink to cool them."


* "A please can go a long way."
~bouldar_mood--
"*HRRRRRR* "I said drink. <i>Now</i>. Repeat make me angry. You not want Bouldar Grimweld angry!"

    ** "You shouldn't threaten workers.["] We don't get paid enough as it is."
    ~bouldar_mood--
    "Bouldar no care. Bouldar <i>need</i> drink now! Or Bouldar crush your face in."
    
        *** "That's overly aggressive."
        ~bouldar_mood--
        "Aggressive? YOU AGGRESSIVE! You make fun of Bouldar!"
        "You are mean, I don't want any drink from mean magetender! *KRRR GRRAAA*"
        // fight starts
        
        *** "I was just messing around, sorry."
        ~bouldar_mood++
        "No...Bouldar sorry too. Bouldar's gears hurt. Like fire, burn stone."
        "Just want something to soothe. Want to be calm, understand?"
        
            **** "I understand.["] I've got just the thing."
            "Bouldar appreciates it, and looks forward to drink."
            // goes to drink making
    
    ** "Alright, I'm sorry.["] I was just messing around."
    ~bouldar_mood++
    "*HHRRK* Bouldar sorry, Bouldar says please. Just upset, joints and gears ache. Fuses blown."
    "Can't control self...want to be calm, not grounded."
    
        *** "Well you <i>are</i> made of stone.["] I'm not sure if you can be anything <i>but</i> grounded."
        ~bouldar_mood--
        "*KRRRR* you make too many joke. Focus on drink instead. Your jokes not even funny."
        
            **** "Ouch...["]I tried atleast."
            "Just make drink, no more joke. Joke bad."
            //goes to drink making

        *** "Sounds good.["] I'll hook you up good."
        ~bouldar_mood++
        "Thank you magetender. Not many place will serve me. Too big, too angry. I hope it good, and helps fix me."
        //goes to drink making


* "Your gears?"
~bouldar_mood++
"Yes. Gears ache and creak. They rub together and burn, like molten rock inside."
"Hurt's to move. Makes me angry."

    ** "Sorry to hear that."
    ~bouldar_mood++
    "That is first time someone say that to me. Other bar won't even take order, just kick me out."
    "I just want to be calm, and not so angry."
    
        *** "I can work with that."
        "Thank you magetender, you are kind."
        // goes to drink making
        
  
    ** "And a drink is the cure?"
    ~bouldar_mood--
    "You are doubting me magetender?! I am here for drink! Not judgement!"
    
        *** "You're right, sorry.["] Just trying to keep the mood light."
        ~bouldar_mood++
        ""I see....Bouldar sorry for yelling. Bouldar not like being judged, because Bouldar can't control it."
        "I just trying to live comfortably."
            
            **** "Of course.["] Let me get you that drink."
            "Bouldar looks forward to it!"
            //goes to drink making
            
        *** "I was just joking."
        "Joke not funny! Bouldar pain is real! Just want drink to help, you make fun of me..."
        
            **** "I didn't mean to hurt you."
            ~bouldar_mood++
            "*KHHH* It fine. Just get Bouldar drink, I feel better after."
            // goes to drink making
            
            **** "I really wasn't, but okay."
            ~bouldar_mood--
            "*KRRR GRAAA* WHY MEAN? WHY MEAN MAGETENDER? YOU LIKE ALL OTHER WHO JUDEGE BOULDAR!"
            "I DON'T WANT DRINK FROM YOU!"
            // goes to drink making

- -> END



= satisfied
"*HHSSSS* Do you hear that magetender?! That sound is steam from my cogs! Relief! Sweet relief!"
"You have my thanks. It hurt so long, I may come back everyday!"
"Now my son can sit comfortable on my shoulders.
"Go on Chip, thank the magetender!"
"..."
"Well said! Then, maybe I see you again. Bye for now!~"

- -> END


= neutral
"*CLANK* *CLICK* You hear that magetender? That is my gears. Still creaking and locking up. "
"The heat lingers, though slightly faded."
"Unfortunate...I hoped for relief. I thought you fix me. Instead, still broken."
"Come Chip, we go home now. Papa will find another way."
"Thanks for trying, magetender."

- -> END


= angry
"*GRRAAA* WHAT IS THIS MONSTROCITY?! I ASKED FOR SOOTHING FEELING. YOU BRING PAIN."
"MY GEARS, MY CORE, IT BURNS! WHAT HAVE YOU DONE?!"
"YOU WILL PAY FOR THIS, MY FUSES HAVE BLOWN! I WILL CRUSH YOU!!!"

- -> END

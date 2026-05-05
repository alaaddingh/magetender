-> Day_One


VAR knight_mood = 0
VAR allies = false
VAR enemies = false
VAR left_without_drink = false
VAR is_angry = false




=== Day_One ===
-> start

= start
"Greetings magetender!"
"I must admit that I am not here for a drink, rather, to avoid <i>their</i> prying eyes."
"So pay me no mind, I shall simply be using your roof as refuge."


* "Who's 'they'?"
    "Why must you know?"
    "Are you with <i>them</i>? Trying to extract information from me?!"
    
    ** "Of course not!["] I'm just someone trying to make a living."
    ~ knight_mood++
    "I see...that is my mistake. How unsightly of me. You have my apologies, you see..."
    "<i>They</i> have been following me for quite some time now. I can't prove it, but I know they're here somewhere..."
    "...watching."
    
        *** "I believe you."
        ~ knight_mood++
        ~ allies = true
        "You do? Huh...no one has ever told me that before. Does that mean we are...allies?"
        "In that case...get me a drink kind Sir. We shall celebrate our newfound friendship with a round of spirits!"
        "But make it light, something that will keep my senses sharp!"
        // Goes to drink making
        
        *** "How about a drink?["] It might ease your worries a bit."
        "A drink? I don't know...I wouldn't want to lose sight of my objective."
        
            **** "I can help with that.["] I'll whip up something to help you stay focused."
            "Ah, you can do that? That would be most appreciated, kind Sir. I shall eagerly await your creation."
            //Goes to drink making
    
    
    ** "Again, who's <i>they</i>?!"
    ~ knight_mood--
    "Do not lie to me, magetender. I know the eyes of a liar when I see them!"
    "Is this bar your secret base? Is that it? I have stumbled into the lion's den it seems!"
    
        *** "Not at all!["] I am but a mere magetender."
        ~ knight_mood++
        "Is that so? Well, do not think I won't have my eyes on you..."
        "...but maybe, if I could get a drink...I would much appreciate it. Something that can keep me focused."
        // goes to drink making
        
    
        *** "Please just order.["] Otherwise I'll have to ask you to leave. You can't stay if you're not a customer."
        "Oh...<i>ahem</i>. I guess I could get something if that's the case. Uhh..."
        "...how embarrassing. Just something light please, I must stay alert."
        //Goes to drink making


* "I'll have to ask you to leave[."], if you're not a paying customer."
    "Is that so? Hmm, most unfortunate. You see, I do not trust you wouldn't slip something into my drink."
    "If you're working for <i>them</i>, it would be easy for you to poison me..."
    
    ** "Don't give me any ideas."
    ~ knight_mood--
    "And what is that supposed to mean?! Are you admitting your allegiance to my enemies?!"
    
        *** "I was joking."
        "This is no time to fool around! A knight of the city is being threatened and you dare to laugh about it?! Never do such a thing again in my presence!"
        
            **** "So...are you going to order?"
            ~ knight_mood--
            ~ left_without_drink = true
            "Wha-ha! After such a show? Why should I? What horrible service! This place is a disgrace!"
            //leaves, no fight (unless players trigger it), and no drink. No money.
            
            **** "Alright I won't."
            ~ knight_mood++
            "Good! Now...if it takes a drink for me to stay then I guess I shall take one. But keep it light, I must stay focused and alert. The enemy could be anywhere..."
            // goes to drink making
    
    
    
        *** "Yes, I am!["]
        ~ knight_mood--
        ~ enemies = true
        "...not. I'm just trying to make a living."
        "By siding with thy enemy! You cannot fool me, <i>mere</i> barkeep. You use this place to gather intel on potential targets."
        "I shall do this city a favor by ridding this place of your dishonorable self!"
        ~ is_angry = true
        // fight starts
    
    
    ** "Why would I do that?"
    ~ knight_mood++
    "Well I...b-because you are with <i>them</i>. And they wish to see me perish!"
    
        *** "I don't wish to see that."
        ~ knight_mood++
        "Oh...well..."
        "...ahem. My apologies. It seems I...may have misjudged you."
        "In fact, let me order one of your finest brews! I have tied you up speaking to me long enough."
        "Just keep it simple, something that won't dull my senses."
        // goes to drink making
        
        
        *** "You have no proof."
        ~ knight_mood--
        "I needn't need proof! But if proof is what you so desire, then make me a drink!"
        "If it is poisoned, then there is your proof!"
        
            **** "But wouldn't you be dead?["] Then your plan sort of goes out the window."
            ~ left_without_drink = true
            "Huh...I supposed you're right."
            "Well...fine then! Forget it, if you do not want my patronage then I shall begone!"
            //leaves, no fight (unless players trigger it), and no drink. No money.

            **** "Sure, let's go with that."
            "With that we shall go indeed! Show me who's side you're truly on, magetender!"
            // goes to drink making

- -> END



= satisfied
"Ah! This is perfect! The kind of brew that will keep me on my toes, ready for whatever foe I may face."
"I must applaud you magetender, I had my doubts. But this is a most fine drink."
"You have earned my trust, for now..."
"I shall be back, next time I hope we may deepen our relations..."
"...in allyship of course! Farewell for now!"

-> END



= neutral
"Hmm, how...<i>interesting</i>."
"Tis not quite the brew I was looking for."
"This is quite unfortunate, but I do not blame you."
"It must be difficult to work under <i>their</i> watchful eye."
"I will be back, let us discuss this again sometime."
"I shan't leave you to be controlled by such evil."

-> END




= angry
"WHAT IS THE MEANING OF THIS?!"
"Did you truly attempt to poison me?!"
"Shame on you! You have no honor if this is how you treat a knight of the city!"
"You shall be hearing about this again, I will get this place shut down if I must!"
"For the good of this city and its people, prepare to defend your honor!"

-> END

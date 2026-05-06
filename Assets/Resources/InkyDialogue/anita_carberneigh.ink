-> Day_One

VAR is_angry = false

=== Day_One ===
-> start

= start
"Hello, Magetender!"
"It is I, the most exquisite unicorn in town, Anita Caberneigh."
* "uh.. who?"
    "What's that? You haven't heard of me?"
    "Dear me, it appears I've encountered a mage who lives under a rock."
    "You really ought to get out more.."
    ** "That's rude"
        "Rude? You mistake my confidence for being brash, darling. You see, the former is earned."
        "But enough dilly-dallying, I'd like to order.."
        *** "You're right, sorry"
            "I'd like to request a potion that will make me feel grounded and calm."
            "Maintaining perfection is... exhausting, you understand."
            "Oh, and please.. do not forget a <b>cherry on top</b>."
        *** "A please would be nice.."
            "Oh...? How fascinating."
            ~ is_angry = true
            "A MAGETENDER WISHES TO LECTURE ME ON ETIQUETTE!"
            "TELL ME, DO YOU GO AROUND LECTURING ROYALTY ON HOW TO BREATHE AS WELL?"
    ** "How can I help?"
        "I'd like to request a potion that will make me feel grounded and calm."
        "Maintaining perfection is... exhausting, you understand."
        "Do not forget a <b>cherry on top</b>."
* "Pleased to serve you"
    "Yes, you should consider this a rare honor."
    "Anyhow, I'd like to request a potion that will make me feel grounded and calm."
    "Maintaining perfection is... exhausting, you understand."
    "Do not forget a <b>cherry on top</b>."

- -> END

= neutral
"Hmm. Not horrible... though you do seem to lack any exceptional knowledge of magetending."
"I can only assume you were given this position through nepotism... or an internal reference. Am I getting warmer?"
"Farewell until next time."

- -> END

= satisfied
"Ah, yes... this drink makes me feel so... tranquil."
"As if I'm dancing on rainbows while fairies lift my fabulous mane."
"Wh-why was I even worked up to begin with... please accept my generous tip as a symbol of my gratitude."
"Farewell until next time, Magetender."

- -> END

= angry
"WH-WHAT IN THE WORLD IS THIS!?!"
"MY BLOOD PRESSURE HAS ONLY RISEN!!!"
"MAGETENDER, YOUR INCOMPETENCE SHALL NOT GO UNPUNISHED!!!"

- -> END

=== Day_Two ===
-> start

= start
"Hello, Magetender. It is I, once again. The most exquisite unicorn in town, Anita Caberneigh."
"But really, I need no introduction at this point.. hush now... you know it's true."
* "Remind me again?"
    "Surely, you jest.. Nobody simply 'forgets' who Anita is.."
    "My popularity exceeds all others. I am adored by many... and envied by the rest."
    "I want a brew that brings me calm and composure."
    "Do ensure it is finished <b>with a cherry on top.</b>"
    "Now... perhaps a bow would be appropriate?"
    ** "*Add a bow*"
        "Hmm. Acceptable."
    ** "*Refuse to bow*"
        "Oh, how bold. How... unfortunate."
        ~ is_angry = true
        "I DO NOT TAKE KINDLY TO BREACHES OF PROPER ETIQUETTE, MAGETENDER"
* "Of course, how could I forget?"
    "As expected. Recognition suits you."
    "I want a brew that brings me calm and composure."
    "Do ensure it is finished <b>with a cherry on top.</b>"
    "Now... perhaps a bow would be appropriate?"
    ** "*Add a bow*"
        "Hmm. Acceptable."
    ** "*Refuse to bow*"
        "Oh, how bold. How... unfortunate."
        ~ is_angry = true
        "I DO NOT TAKE KINDLY TO BREACHES OF PROPER ETIQUETTE, MAGETENDER"

- -> END

= neutral
"Hmm. This has done little to improve my mood."
"There is a certain... lack of refinement here."
"Do try to elevate your craft before our next encounter."
"Farewell."

- -> END

= satisfied
"Ah... yes. This is quite lovely."
"I feel positively serene... as though the world itself has softened."
"Perhaps I misjudged you... just slightly."
"Do not grow complacent. But... well done, Magetender."

- -> END

= angry
"WHAT is this??!"
"This is an utter disgrace!"
"You have ruined my entire mood!"
"You will answer for this incompetence!"

- -> END

=== Day_Three ===
-> Day_Two

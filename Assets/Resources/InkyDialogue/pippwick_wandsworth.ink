VAR hamster_mood = 0
VAR promise = false
VAR buy_book = false
VAR hamster_day1_result = "none"
VAR is_angry = false


-> Day_Three


=== Day_Three ===
-> start

= start
"SQUEAK. SQUEAK. SQUE-"
"A-ahem! Ahem...sorry about that. Had something stuck in my throat."
"Haaah...honestly this is the last place I'd rather be. I'm supposed to be in my lab, developing my new spells so that I can finally publish my book!"
"It's called Wandsworth's Wonderful World of Wishes! It's supposed to be a book on all the <i>hip</i> new age spells."


* "Hip?"
    "Yeah, you know, like all the spells the kids are into these days!"
    
    ** "I wouldn't know."
    "Oh...right. Not everyone is gifted in the ways of magic..."
    "But that's okay! You're gifted in the way of drink making, right?"
    
        *** "You could say that."
        "You don't sound too confident..."
        "I know the feeling my friend, I was the best in my class for magic. One of the youngest professors at the academy ever!"
        "But...I haven't been able to develop any new spells lately. You wouldn't happen to have a drink that could give me some ideas?"
        
            **** "I sure do!"
            ~ promise = true
            "Perfect! I'm expecting a surge of motivation and inspiration when I taste this drink!"
            // goes to drink making
            

            **** "Not exactly...["]but it should do the trick!"
            "I'll take your word for it, just give me something that might help and my money is yours."
            "Money that I have...a lot of, by the way..."
            // goes to drink making
        
        
        *** "Actually it's only my fourth day..."
        "Oh...really? Um...well, maybe you're a prodigy!"
        "I hate to ask, but what you make won't..."
        "...kill me, right?"
        
            **** "Only time will tell."
            "Jeez, you've really got a way of putting your customer's minds at ease huh?"
            "Forget it, jokes aside, I need a drink. Something that'll keep me grounded but get my mind racing with new ideas!"
            "Got it?"
            // goes to drink making
            
            
            **** "No-one's died yet."
            "Ha...haha. Well, that's certainly good news. Um, I'll just take something simple. I want to stay grounded but feel that rush of the spirits flowing through me!"
            "Think you can do that?"
            // goes to drink making
    
    ** "What are they into?"
    "...You don't want to know."
    "Let's just keep it at that."
    
        *** "I see..."
        "Yeah...anyway, case and point, I need a drink. Something that will get my creative brilliance up and running again!"
        "If I can't finish the development of these spells...am I even a wizard anymore?"
        
            **** "Of course you are."
            "Well, I appreciate that magetender. But it's developing those spells that brings me true joy and purpose."
            "Think you can whip me up a drink that could help me?"
            // goes to drink making
            
            
            **** "Fear not, I have the cure."
            ~ promise = true
            "I like your confidence! I'll be holding you to it young magetender, I expect great things!"
            "As others expect such from me..."
            // goes to drink making
        
        
        *** "No, I want to know."
        "No, no you don't. Look, just forget about it okay? I'm not here to joke around."
        "This is serious! I haven't developed a new formula in months!"
        "ARGH! Just make me a drink already!"
        // goes to drink making

* "That sounds interesting!"
"Right?! Well, be sure to check out the local bookstore once it's finally published."
"I'll be sure to come back and remind you!"

    ** "Sounds like a plan."
    "Perfect! The only thing left is to actually finish it..."
    "Those last few spells are really stumping me, so I need something that'll help."
    
        *** "I've got just the drink."
        ~ promise = true
        "Wonderful! I'll be whipping up new formulas in no-time then!"
        "I'll be holding you to your word."
        //goes to drink making
        
        
        *** "I've got magic of my own."
        "Well isn't that great to hear? Well, let me see your magic magetender!"
        "I'm expecting great things from you, my book release is riding on this!"
        //goes to drink making
    
    
    ** "I don't need a magic book."
    "Well...I suppose not. But, it's my life's work! Is it so bad to want people to read it?"
    "A book full of spells I created!"
    
    
        *** "I have no use for magic."
        "You must truly kill the vibe mustn't you?"
        "Whatever, I doubt any drink you make could <i>actually</i> help me."
        
            **** "Wait, give it a chance."
            "Oh <i>now</i> you change your tune?"
            "Whatever, look, just get me something that'll get my creative juices flowing, alright?"
            //goes to drink making
            
            **** "And I doubt you <i>created</i> any of those spells."
            "SQUEAK! SQUEAK! SQUEAK!"
            "WHY DON'T I SHOW YOU SOME OF MY SPELLS?!"
            ~ is_angry = true

        
        
        *** "Alright I'll read it."
        ~ buy_book = true
        "Haha! A buyer already! In return, I'll give you a nice tip if your drink can truly help me get my mojo back."
        // goes to drink making


- -> END




= satisfied
"Haha! How wonderful! Truly magic indeed!"
{promise: "You said your drink would cure me, and I'll admit I had my doubts, but I can feel the magic flowing in my veins!"| "I absolutely must come back here the next time I am struggling with my work." }
"I could write 10, no, 100 new spells with this!"
"Thank you! Thank you magetender!"
~ hamster_day1_result = "satisfied"

-> END



= neutral
"Hmm...how disappointing."
{promise: "You promised me that it would work and yet, I do not feel new inspiration coming on." | "I was hoping this drink might inspire inspiration in my work, so that I might conjure up some new spells..."}
"Alas...I am back to square one."
"I appreciate your efforts, magetender, but it seems my wizard's block is yet to be cured."
~ hamster_day1_result = "neutral"

-> END




= angry
"SQUEAK! SQUEAK!"
{promise: "What on earth is this?! You promised to make me a drink that could help me finish my book!" | "What on earth is this disgusting creation?! I thought your drink might help me!"}
"Instead I receive this garbage?!"
"It might have done more harm than good!"
"I'll be thinking about this horrid drink so much I won't be able to create any new spells!"
"This is all your fault!"
~ hamster_day1_result = "angry"

-> END


=== Day_Two ===
-> 2nd_Visit

= 2nd_Visit
{ hamster_day1_result == "satisfied":
    "Ah, magetender! You again! Or rather, me again. I'm back!"
    "Your drink last time was...well, it worked! I got three new spells written up that very evening!"
    "Three! Can you believe it? I was on a roll!"
    "...and then I hit a wall again. But that's beside the point."
- else:
    { hamster_day1_result == "neutral":
        "Magetender! I have returned, aren't you excited to have my patronage once more?"
        "Look, I know last time wasn't exactly a roaring success but I have no other options."
        "My publisher is breathing down my neck and I still have four spells left to finalise."
        "Four! It's not even that many! And yet here I am, completely and utterly stuck."
    - else:
        "..."
        "Don't say anything. I really don't want to be reminded of my failures."
        "As you can see...I am desperate. Truly, rock bottom, desperate."
        "Just...make me something. Anything. Please."
        "I know last time did not go so well, but I am hoping we can try again."
    }
}

* "Good to see you!["] How's the book coming along?"
{ hamster_day1_result == "satisfied":
    ~ hamster_mood++
    "Well! Well...mostly well."
    "I have four spells left and I was feeling great until my publisher got involved."
- else:
    "How do you think it's going magetender?"
    "I have four spells left and a publisher who keeps sending me letters with the word URGENT written on them."
    "In red ink. Who uses red ink?"
}
"Apparently some of my spells are too...niche. His words."
"He wants me to cut the spell for temporarily turning bread into a pillow."
    
    ** "The bread pillow spell sounds useful actually."
    ~ hamster_mood++
    "RIGHT?! That is EXACTLY what I said!"
    "Imagine you're on a long journey and you're utterly exhausted. You know what you never bring on a long journey?"
    "A pillow! And what do you always bring? Bread!"
    "Problem. Solved. But he says that nobody would ever need it. I say he has no creative vision."

        *** "I'm on your side.["] Keep it in."
        ~ hamster_mood++
        "Thank you! Finally, a person of culture."
        "Now, if your drink could give me the same energy I have right now I could finish this book tonight!"
        // goes to drink making

        *** "I mean...he might have a point."
        ~ hamster_mood--
        "Et tu, magetender?"
        "You know what, forget the bread pillow. I have bigger problems."
        "Just make me something that gets my brain working again. Something with a kick to it."
        // goes to drink making

    ** "What are the four spells you have left?"
    ~ hamster_mood++
    "Ah! Good question. Glad you asked."
    "I have the spell for making petrichor smell like freshly baked cookies, a spell to temporarily give furniture feelings, a spell that makes you sneeze in musical notes, and one that makes shadows slightly warmer."
    "...the publisher wants to cut all four."
    // I lowk don't like the smell of petrichor
    
        *** "I can see why."
        ~ hamster_mood--
        "Excuse me?!"
        "These are nuanced spells! They are quality of life improvements!"
        "Do you know how miserable a cold shadow is?! Have you ever stood in one?!"

            **** "Fair point.["] Make him keep them in then."
            ~ hamster_mood++
            "Exactly! Thank you!"
            "It's as if my publisher doesn't understand what the whole point of my book is!"
            "It's a book of QOL spells! Not ones that can topple nations!"
            "Look, just get me something that will give me enough energy to write a strongly worded letter to my publisher <b>and</b> finish my remaining spells."
            // goes to drink making

            **** "A cold shadow is just...a shadow."
            ~ hamster_mood--
            "SQUEAK!"
            "You are just as bad as my publisher! Worse even!"
            "At least he reads the spells before dismissing them! You haven't even seen my work!"

                ***** "I'm sorry, I was just being honest."
                ~ hamster_mood++
                "Yeah and honesty hurts sometimes!"
                "But...you're right. I get that it might seem pointless, but these are the spells that kids find interesting these days!"
                "Anyway, just make me something strong.I need to prove to <i>both</i> of you are dead wrong!"
                "And yes, a drink is the way to do that."
                // goes to drink making

                ***** "Warmer shadows though? Really?"
                ~ hamster_mood--
                "THAT IS THE LAST STRAW!"
                "I did not come here to be mocked by a magetender who has never created a single original spell in their life!"
                "You want to see some <i>real</i> magic?"
                "I'LL SHOW YOU REAL MAGIC!"
                // fight starts

        *** "Your publisher sounds awful."
        ~ hamster_mood++
        "He is! He has absolutely no appreciation for the more subtle arts!"
        "He keeps saying the spells need to be more practical. More marketable."
        "I told him, not everything needs to be marketable, some things just need to bring joy!"
        "Anyway, a drink please. I need my drunk brain back if I want to make any more spells. "
        // goes to drink making

* "Publisher trouble?"
~ hamster_mood++
"UGH don't even get me started."
"He wants me to cut half my spells and replace them with what he calls 'crowd pleasers'."
"Crowd pleasers! As if my spells aren't already crowd pleasing!"
"Name one person who wouldn't want their sneezes to sound musical. Go on. Name one."

    ** "...me."
    ~ hamster_mood--
    "Oh wonderful. A critic AND my magetender. Lucky me."
    "Fine, you don't want musical sneezes, what would YOU want then?"
    
        *** "Something actually useful."
        ~ hamster_mood--
        "Something actually useful. Right."
        "Because making petrichor smell like cookies has no use. Making shadows, ever so slightly, warmer has no use?"
        "Tell me magetender, have you ever been caught in the rain?"

            **** "Yes, and it smelled like rain."
            ~ hamster_mood--
            "And wouldn't it have been <i>better</i> if it smelled like <i>cookies</i>?!"
            "I rest my case! Now make me a drink before I start rethinking our entire patron and patronee relationship!"
            // goes to drink making

            **** "Sure, but cookie scented rain wouldn't have helped."
            ~ hamster_mood--
            "SQUEAK! SQUEAK! SQUEAK!"
            "WHY MUST YOU HAVE ZERO IMAGINATION? YOU ARE SUCH A BORING PERSON TO BE A MAGETENDER!"
            "FORGET THE DRINK, FORGET THE BOOK. I'LL SHOW YOU RIGHT HERE RIGHT NOW HOW AMAZING MY MAGIC CAN BE!"
            // fight starts

        *** "I take it back.["] Musical sneezes sound great."
        ~ hamster_mood++
        "Ha! I thought so."
        "Look, just make me something that gets my creative energy going. I have four spells to finalise and a publisher to prove wrong."
        // goes to drink making

    ** "Honestly I'd want that."
    ~ hamster_mood++
    "See?! A person of taste and culture!"
    "You know what, I already feel better just talking to you."
    "That's what my publisher doesn't understand. These spells bring people joy. Simple, wonderful, joy."
    "Now, if your drink can do the same thing for my brain that this conversation has done for my mood, we're in business."

        *** "No promises.["] But I'll try."
        "Trying is all I ask! I uh...would prefer it if you succeed though."
        "I'm in a time crunch..."
        // goes to drink making

        *** "Consider it done."
        ~ promise = true
        ~ hamster_mood++
        "Such wonderful confidence! Don't let me down now, the fate of Wandsworth's Wonderful World of Wishes rests in your hands!"
        // goes to drink making

- -> END


= satisfied
{ hamster_day1_result == "satisfied":
    "TWICE! Twice I have seen and tasted your version of magic!"
    "I was beginning to think last time was a fluke but no! You are the real deal magetender!"
    "Three spells last time and now I can feel even MORE coming on!"
- else:
    { hamster_day1_result == "neutral":
        "This is it! THIS is what I was looking for last time!"
        "I knew you had it in you! I just knew it!"
        "Last time was like a sneeze that never came, but <i>this</i>? This is the sneeze!"
    - else:
        "I...I don't believe it."
        "After last time I had written this place off entirely."
        "And yet here we are! The ideas, they're finally flowing again!"
    }
}
{ buy_book == true:
    "And you believed in the book before you even tasted success! A signed copy. First edition. That is a promise."
- else:
    { promise == true:
        "You promised and you delivered! I shall not forget this magetender!"
    }
}
"I must go immediately before I lose this feeling!"
"Wandsworth's Wonderful World of Wishes will be COMPLETE by morning!"
"*SQUEAK!SQUEAK!*"

- -> END


= neutral
{ hamster_day1_result == "satisfied":
    "Hmm. Not quite what you made last time."
    "Last time I was buzzing! This time I feel like fire that can't quite get started..."
    "What changed? What did you do differently?"
- else:
    { hamster_day1_result == "neutral":
        "Again. AGAIN with the almost!"
        "It is like being one ingredient short of a perfect spell. So close and yet so far."
        "I cannot keep doing this magetender, my publisher is not a patient man."
    - else:
        "Well...it is better than last time I suppose."
        "Last time was a disaster. This is merely a disappointment."
        "I suppose that counts as progress."
    }
}
{ promise == true:
    "You promised me again and again I am left wanting. You should not make promises you cannot keep."
}
"I'll come back. I have no choice."
"Those spells aren't going to write themselves and my publisher's letters are getting more aggressive by the day."
"Don't let me down next time, magetender."

- -> END


= angry
{ hamster_day1_result == "satisfied":
    "SQUEAK! SQUEAK!"
    "After last time?! After how WELL last time went?!"
    "I thought you had it figured out! I thought we had an understanding!"
- else:
    { hamster_day1_result == "neutral":
        "I gave you another chance! I sat here and I gave you ANOTHER CHANCE!"
        "Last time was bad enough but THIS?! This is somehow WORSE!"
    - else:
        "Fool me once magetender, shame on you!"
        "Fool me TWICE?!"
        "SHAME ON ME! AND ALSO STILL ON YOU!"
    }
}
{ promise == true:
    "And you PROMISED! You sat there and promised me!"
}
"My publisher is going to drop me, my book will never be finished, and it is ALL. YOUR. FAULT!"
"PREPARE YOURSELF MAGETENDER! IF I CANNOT FIND THE MOTIVATION FOR MAGIC IN LIQUOR, THEN MAY I FIND IT IN BATTLE!"

- -> END


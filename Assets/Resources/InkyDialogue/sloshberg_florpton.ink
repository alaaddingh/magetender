-> Day_One

VAR is_angry = false

=== Day_One ===
-> start

= start
"<b>*GLAURGHH* *GLAURGHHHHHHHH*</b>"
	"That's my greeting... ya like it? Or was it <b>*GLAURGH*</b>-bage?"
* "I like the greeting."
	"Heheh... anyways..."
	"I saw a review by NobleChadwick33 on Kelp. Said you made him a drink so good he quit his desk job!"
	"...to pursue juggling. Respect. I could use that kinda uplift."
	"I've been doomscrolling for like a whole week nonstop..."
	"Feelin' pretty sluggish... pretty sloshy..."
	"Could ya make me a brew that helps with my lethargy... <b>*GLAURGH*</b>"
	"And maybe toss in a <b>slice of lime...</b>"
* "I'm confused.."
	"C'mon, man. the name is Sloshberg Florpton, were you expecting a normal greeting?"
	** "What can I get you?"
		"I saw a review by NobleChadwick33 on Kelp. Said you made him a drink so good he quit his desk job..."
		"...to pursue juggling. Respect. I could use that kinda uplift."
		"I've been doomscrolling for like a whole week nonstop..."
		"Feelin' pretty sluggish... pretty sloshy..."
		"Could ya make me a brew that helps with my lethargy... <b>*GLAURGH*</b>?"
		"And maybe toss in a <b>slice of lime...</b>"
	** "It was weird."
		"<b>*Angry GLOARGH*</b> Ohohh... harsh..."
		~ is_angry = true
		"<b>*GLAURGH*</b> forbid I try something out original!"
		"Looks like those Kelp reviews were just a load of slosh!"

- -> END

= neutral
"<b>*GLOURGH*</b> This drink is alright... it hasn't really lifted my lethargy..."
"I... sorta question your <b>*GLOURGH*</b> potion-making skills, my guy..."

- -> END

= satisfied
"<b>*Happy GLORGH*</b> WOW! I feel INCREDIBLE... don't think I've felt this since I was a little globule..."
"My jelly body feels revitalized... I have just enough energy to hand ya a big ol' tip!"
"I'm inviting all my blobs to come gobble here! Thank you, Magetender!"

- -> END

= angry
"<b>*Angry GLOARGH*</b> WHAT THE SLIME IS THIS!?!"
"YOU THINK I'M SOME SORT OF SLOSH-BIN, HUH!?!"
"I'LL GIVE YOU A ROUGH <b>*GLOARGH*</b> YOU'LL REMEMBER!!!"

- -> END

=== Day_Two ===
-> start

= start
"<b>*GLAURGHH* *GLAURGHHHHHHHH*</b>"
"..."
"I need your help..."
"My <b>*GLAURGHH*</b> is way too high after a Blob's Night Out..."
"Everything feels off... too sloshy..."
"Could you make me something to bring me back down?"
"Hopefully you still make good brews?"
* "Yeah. I've got you."
	"<b>*glorgh...*</b> Thanks... I need the reset..."
	"Please don't forget that <b>slice of lime..</b>"
* "Depends. How bad was Blob's Night Out?"
	"Let's just say that me and all my fellow blobs woke up in a ditch.."
	** "sound rough."
		"Yeah... exactly... thanks for not judging..."
	** "Sounds like your own fault."
		~ is_angry = true
		"<b>*Angry GLOARGH*</b> WOW... okay..."
		"I come in here need to feel better, now I just feel <b>*GLOURGH*</b>  judged.."

- -> END

= neutral
"<b>*GLOURGH*</b> This drink doesn't make me feel any less sluggish.."
"I think I saw some tasty trash behind the tavern..."
"Yeah... that'll bring my mood up."

- -> END

= satisfied
"<b>*Happy GLORGH*</b> WOW! My wiggly nature is back!"
"Thank you again, Magetender! I'll get my blobs out here soon!"

- -> END

= angry
"<b>*Angry GLOARGH*</b> THIS POTION IS SOME STRAIGHT SLOGFLORP!!!"
"I AM NOT A COMPOST BIN FOR YOU TO MAKE USE OF!!!"

- -> END

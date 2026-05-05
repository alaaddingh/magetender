-> Day_One

VAR is_angry = false
VAR show_license = false

=== Day_One ===
-> start

= start
"Hey there, fellow adult! I'll just get one brew please! heheh..."
"Just the.. uh.. USUAL!"
"<b>BZZZZOHH!!!</b> You don't seem thrilled?"
"..."
* "How old are you?"
	"What? Old enough to command a spaceship, obviously.."
	"I'm in my early 20 billions!"
	"What, you want a form of ID? Fine. Feast your Earth-eyes on my Galactic Adult License."
	"As you can see It's laminated.. which makes is VERY official.."
	~ show_license = true
	** "Looks legit. Go ahead."
		"Excellent judgment, Earthling."
		"Regardless, I need a nice, grounded yet elevated brew."
		"I'm wanting to feel a little 'down to Earth' between the two of us. Hehe..see what I did there?"
		"And don't forget to top if off with a <b> a mini umbrella!</b>"
	** "This doesn't look legit."
		~ is_angry = true
		"<b>BZZZZOHH!!!</b> WHAT?!"
		"THIS IS INTERGALACTIC AGE-ISM, EARTHLING! PREPARE TO BE ZOINKED!"
* "What can I get you?"
	"I'd like a nice, grounding yet elevating brew.."
	"I'm wanting to feel a little more 'down to Earth' between the two of us. Hehe... see what I did there?"
	"And don't forget to top if off with a <b> a mini umbrella!</b>"

- -> END

= neutral
"Huh... this drink doesn't fully capture my senses.. it's okay,  I guess."
"Don't feel bad, even capturing Earthlings is easier than capturing the right balance of taste."

- -> END

= satisfied
"ZOINKS! This is amazing... this is the good stuff!"
"I definitely feel a bit more 'down to earth'. Way to go!"

- -> END

= angry
"WHAT IN THE MILKY WAY DID YOU BREW ME?!"
"THIS TASTES LIKE THOSE CARBON POLLUTANTS YOU EARTHLINGS LOVE TO DISPEL!!"
"DUEL ME RIGHT NOW!!! YOU'LL LEARN WHY THEY CALL ME A REAL ZORP AFTER I ZOINK YOU!!!"

- -> END

=== Day_Two ===
-> start

= start
"WAZZZZ good, my Earthling! Hopefully your day is as zorp-tastic as mine!"
"..."
* "You still look a little young"
	"HA! Classic Earthling mistake."
	"My species ages differently. Very differently. In fact, I'm, like... ancient."
	"in my early 20 billions to be exact!"
	"Behold: my Galactic Maturity License. I mean just LOOK at that lamenation! Very official..."
	** "Fine. You're good."
		"Excellent.."
		"Anyway, can you make me a grounded yet elevated brew?"
		"With an <b>umbrella on top</b> of my potion?"
		"Something to blast my zorp-mind into full levitation!"
	** "Nice fake. No drink."
		~ is_angry = true
		"BZZZZOHH!!! YOU REJECT AN INTERGALACTIC ELDER?!"
		"I SHOULD REPORT THIS TO THE COSMIC COUNCIL AT ONCE!"
		"PREPARE FOR A FORMAL COMPLAINT AND A PROPER ZOINKING!"
* "What are you in the mood for?"
	"Can you make me a grounding yet elevating brew?"
	"With an <b>umbrella on top</b> of my potion?"

- -> END

= neutral
"Huh... this drink doesn't quite capture what I'm looking for."
"Gnawing on unicorn horns gives me stronger zoomies than this... brew..."
"I expect an improvement when I return, little Earthling."

- -> END

= satisfied
"ZOINKS! This gives me the zoomies I needed!"
"I feel like I'm levitating on a dreamlike planet!"
"You have served me well, Magetender. Until next time."

- -> END

= angry
"WHAT IN THE MILKY WAY IS THIS?! THIS TASTES LIKE SPACE JUNK!!!"
"PREPARE TO BE ZOINKED BY ZIP ZORP!!!"

- -> END

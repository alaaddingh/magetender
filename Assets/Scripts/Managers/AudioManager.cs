using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance { get; private set; }

	private const string VolumePrefKey = "MasterVolume";
	private const string CombatSceneName = "QTECombatScene";

	[Header("Sources")]
	[SerializeField] private AudioSource uiSource;
	[SerializeField] private AudioSource sfxSource;
	[SerializeField] private AudioSource loopSource;

	[Header("Clips")]
	[SerializeField] private AudioClip startOfDayBellClip;
	[SerializeField] private AudioClip buttonClickClip;
	[SerializeField] private AudioClip glassSelectClip;
	[SerializeField] private AudioClip ingredientClickClip;
	[SerializeField] private AudioClip trashClip;
	[SerializeField] private AudioClip pourLoopClip;
	[SerializeField] private AudioClip monsterWalkInClip;
	[SerializeField] private AudioClip ambienceClip;

	[Header("Assess screen ambience (by mood)")]
	[SerializeField] private AudioClip assessAmbienceHappyClip;
	[SerializeField] private AudioClip assessAmbienceNeutralClip;
	[SerializeField] private AudioClip assessAmbienceAngryClip;

	[Header("Register")]
	[SerializeField] private AudioClip registerChaChingClip;

	[Header("Shop")]
	[SerializeField] private AudioClip cantAffordClip;

	[Header("Combat")]
	[SerializeField] private AudioClip combatHealClip;
	[SerializeField] private AudioClip combatAttackClip;
	[SerializeField] private AudioClip combatCorrectKeyClip;
	[SerializeField] private AudioClip combatIncorrectKeyClip;

	[Header("Background Music")]
	[SerializeField] private AudioClip backgroundMusicClip;
	[SerializeField] private AudioSource musicSource;

	[Header("Character VO's")]
	// Toad customer VO's
	[SerializeField] private List<AudioClip> toadVoiceClipGeneric;
	[SerializeField] private List<AudioClip> toadVoiceClipAngry;
	[SerializeField] private List<AudioClip> toadVoiceClipBattle;

	// Alien customer VO's
	[SerializeField] private List<AudioClip> alienVoiceClipGeneric;
	[SerializeField] private List<AudioClip> alienVoiceClipAngry;
	[SerializeField] private List<AudioClip> alienVoiceClipSatisfied;
	[SerializeField] private List<AudioClip> alienVoiceClipBattle;

	// Blob customer VO's
	[SerializeField] private List<AudioClip> blobVoiceClipGeneric;
	[SerializeField] private List<AudioClip> blobVoiceClipAngry;
	[SerializeField] private List<AudioClip> blobVoiceClipSatisfied;
	[SerializeField] private List<AudioClip> blobVoiceClipBattle;

	// Dragon customer VO's
	[SerializeField] private List<AudioClip> dragonVoiceClipGeneric;
	[SerializeField] private List<AudioClip> dragonVoiceClipAngry;
	[SerializeField] private List<AudioClip> dragonVoiceClipSatisfied;
	[SerializeField] private List<AudioClip> dragonVoiceClipBattle;

	// Hamster customer VO's
	[SerializeField] private List<AudioClip> hamsterVoiceClipGeneric;
	[SerializeField] private List<AudioClip> hamsterVoiceClipAngry;
	[SerializeField] private List<AudioClip> hamsterVoiceClipSatisfied;
	[SerializeField] private List<AudioClip> hamsterVoiceClipBattle;

	// Knight customer VO's
	[SerializeField] private List<AudioClip> knightVoiceClipGeneric;
	[SerializeField] private List<AudioClip> knightVoiceClipAngry;
	[SerializeField] private List<AudioClip> knightVoiceClipSatisfied;
	[SerializeField] private List<AudioClip> knightVoiceClipBattle;

	// Rocky customer VO's
	[SerializeField] private List<AudioClip> rockyVoiceClipGeneric;
	[SerializeField] private List<AudioClip> rockyVoiceClipAngry;
	[SerializeField] private List<AudioClip> rockyVoiceClipSatisfied;
	[SerializeField] private List<AudioClip> rockyVoiceClipBattle;

	// Unicorn customer VO's
	[SerializeField] private List<AudioClip> unicornVoiceClipGeneric;
	[SerializeField] private List<AudioClip> unicornVoiceClipAngry;
	[SerializeField] private List<AudioClip> unicornVoiceClipSatisfied;
	[SerializeField] private List<AudioClip> unicornVoiceClipBattle;


	private AudioSource cantAffordVoice;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);

		float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
		SetMasterVolume(savedVolume);

		if (uiSource != null)
			uiSource.ignoreListenerPause = true;

		CreateCantAffordVoice();
		SceneManager.sceneLoaded += HandleSceneLoaded;
		UpdateMusicForScene(SceneManager.GetActiveScene().name);
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			SceneManager.sceneLoaded -= HandleSceneLoaded;
		}
	}

	private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		UpdateMusicForScene(scene.name);
	}

	private void UpdateMusicForScene(string sceneName)
	{
		if (backgroundMusicClip == null || musicSource == null)
			return;

		musicSource.loop = true;

		// Never play music in the combat scene.
		if (sceneName == CombatSceneName)
		{
			PauseBackgroundMusic();
			return;
		}

		PlayBackgroundMusic();
	}

	public void PlayBackgroundMusic()
	{
		if (backgroundMusicClip == null || musicSource == null)
			return;

		if (musicSource.clip != backgroundMusicClip)
			musicSource.clip = backgroundMusicClip;

		musicSource.loop = true;
		if (!musicSource.isPlaying)
			musicSource.Play();
	}

	public void PauseBackgroundMusic()
	{
		if (musicSource == null)
			return;
		if (musicSource.isPlaying)
			musicSource.Pause();
	}

	public void StopBackgroundMusic()
	{
		if (musicSource == null)
			return;
		musicSource.Stop();
	}

	public void ResumeBackgroundMusic()
	{
		if (musicSource == null || backgroundMusicClip == null)
			return;

		if (musicSource.clip != backgroundMusicClip)
			musicSource.clip = backgroundMusicClip;

		if (!musicSource.isPlaying)
			musicSource.UnPause();
	}

	private void CreateCantAffordVoice()
	{
		var go = new GameObject("CantAffordVoice");
		go.transform.SetParent(transform, false);
		cantAffordVoice = go.AddComponent<AudioSource>();
		cantAffordVoice.playOnAwake = false;
		cantAffordVoice.loop = false;

		AudioSource template = sfxSource != null ? sfxSource : uiSource;
		if (template != null)
		{
			cantAffordVoice.outputAudioMixerGroup = template.outputAudioMixerGroup;
			cantAffordVoice.volume = template.volume;
			cantAffordVoice.spatialBlend = template.spatialBlend;
			cantAffordVoice.priority = template.priority;
		}
	}

	public float GetMasterVolume()
	{
		return AudioListener.volume;
	}

	public void SetMasterVolume(float volume)
	{
		volume = Mathf.Clamp01(volume);
		AudioListener.volume = volume;
		PlayerPrefs.SetFloat(VolumePrefKey, volume);
		PlayerPrefs.Save();
	}

	public void PlayButtonClick()
	{
		if (uiSource == null || buttonClickClip == null)
		{
			return;
		}

		uiSource.PlayOneShot(buttonClickClip);
	}

	public void PlayGlassSelect()
	{
		if (sfxSource == null || glassSelectClip == null)
			return;
		sfxSource.PlayOneShot(glassSelectClip);
	}

	public void PlayIngredientClick()
	{
		if (sfxSource == null || ingredientClickClip == null)
			return;
		sfxSource.PlayOneShot(ingredientClickClip);
	}

	public void PlayCantAfford()
	{
		if (cantAffordClip == null || cantAffordVoice == null)
			return;

		cantAffordVoice.Stop();
		cantAffordVoice.clip = cantAffordClip;
		cantAffordVoice.Play();
	}

	public void PlayTrashLoop()
	{
		if (loopSource == null || trashClip == null)
			return;
		if (loopSource.isPlaying && loopSource.clip == trashClip)
			return;
		loopSource.clip = trashClip;
		loopSource.loop = true;
		loopSource.Play();
	}

	public void StopTrashLoop()
	{
		if (loopSource == null)
			return;
		if (loopSource.isPlaying && loopSource.clip == trashClip)
		{
			loopSource.Stop();
			loopSource.clip = null;
		}
	}

	public void PlayStartOfDayBell()
	{
		if (uiSource == null || startOfDayBellClip == null)
		{
			return;
		}

		uiSource.PlayOneShot(startOfDayBellClip);
	}

	public void PlayMonsterWalkIn()
	{
		if (sfxSource == null || monsterWalkInClip == null)
		{
			return;
		}

		sfxSource.PlayOneShot(monsterWalkInClip);
	}

	public void PlayPourLoop()
	{
		if (loopSource == null || pourLoopClip == null)
		{
			return;
		}

		if (loopSource.isPlaying && loopSource.clip == pourLoopClip)
		{
			return;
		}

		loopSource.clip = pourLoopClip;
		loopSource.loop = true;
		loopSource.Play();
	}

	public void StopPourLoop()
	{
		if (loopSource == null)
		{
			return;
		}

		if (loopSource.isPlaying && loopSource.clip == pourLoopClip)
		{
			loopSource.Stop();
			loopSource.clip = null;
		}
	}

	public void PlayAmbience()
	{
		if (loopSource == null || ambienceClip == null)
		{
			return;
		}

		if (loopSource.isPlaying && loopSource.clip == ambienceClip)
		{
			return;
		}

		loopSource.clip = ambienceClip;
		loopSource.loop = true;
		loopSource.Play();
	}

	public void StopAmbience()
	{
		if (loopSource == null)
		{
			return;
		}

		if (loopSource.isPlaying && loopSource.clip == ambienceClip)
		{
			loopSource.Stop();
			loopSource.clip = null;
		}
	}

	public void PlayAssessAmbience(string state)
	{
		if (sfxSource == null)
			return;
		AudioClip clip = null;
		if (state == "satisfied" && assessAmbienceHappyClip != null)
			clip = assessAmbienceHappyClip;
		else if (state == "angry" && assessAmbienceAngryClip != null)
			clip = assessAmbienceAngryClip;
		else if (assessAmbienceNeutralClip != null)
			clip = assessAmbienceNeutralClip;
		if (clip == null)
			return;
		sfxSource.PlayOneShot(clip);
	}

	public void PlayRegisterChaChing()
	{
		if (uiSource == null || registerChaChingClip == null)
			return;
		uiSource.PlayOneShot(registerChaChingClip);
	}

	public void PlayCombatHeal()
	{
		if (sfxSource == null || combatHealClip == null)
			return;
		sfxSource.PlayOneShot(combatHealClip);
	}

	public void PlayCombatAttack()
	{
		if (sfxSource == null || combatAttackClip == null)
			return;
		sfxSource.PlayOneShot(combatAttackClip);
	}

	public void PlayCombatCorrectKey()
	{
		if (sfxSource == null || combatCorrectKeyClip == null)
			return;
		sfxSource.PlayOneShot(combatCorrectKeyClip, 0.5f);
	}

	public void PlayCombatIncorrectKey()
	{
		if (sfxSource == null || combatIncorrectKeyClip == null)
			return;
		sfxSource.PlayOneShot(combatIncorrectKeyClip);
	}

	public void StopAllSounds()
	{
		if (musicSource != null)
			musicSource.Stop();

		if (loopSource != null)
		{
			loopSource.Stop();
			loopSource.clip = null;
		}

		if (sfxSource != null)
			sfxSource.Stop();

		if (uiSource != null)
			uiSource.Stop();

		if (cantAffordVoice != null)
			cantAffordVoice.Stop();
	}

	//Tyvin: Feel free to remove or replace it, since this was to visually test it out
	// Uses customerSprite.sprite.name from QTECombatManager to determine the character type and play the right clips.
	public void PlayCharacterHit(string characterType)
	{
		Debug.Log("Detected character type: " + characterType);
		if (sfxSource == null)
		{
			Debug.Log("No sfx source assigned for character type: " + characterType);
		}
		if (characterType == "toad_angry")
		{
			if (toadVoiceClipBattle == null || toadVoiceClipBattle.Count == 0)
			{
				Debug.Log("No battle toad voice clips assigned.");
				return;
			}
			int randomIndex = Random.Range(0, toadVoiceClipBattle.Count);	
			sfxSource.clip = toadVoiceClipBattle[randomIndex];
			Debug.Log($"Playing battle toad voice clip: {sfxSource.clip.name}");
			sfxSource.pitch = UnityEngine.Random.Range(1.4f, 1.8f); 
			sfxSource.Play();
		}
		else if (characterType == "alien_fight_0")
		{
			if (alienVoiceClipBattle == null || alienVoiceClipBattle.Count == 0)
			{
				Debug.Log("No battle alien voice clips assigned.");
				return;
			}
			int randomIndex = Random.Range(0, alienVoiceClipBattle.Count);	
			sfxSource.clip = alienVoiceClipBattle[randomIndex];
			Debug.Log($"Playing battle alien voice clip: {sfxSource.clip.name}");
			sfxSource.pitch = UnityEngine.Random.Range(0.8f, 1.4f); 
			sfxSource.Play();
		}
		else if (characterType == "knight_fight")
		{
			if (knightVoiceClipBattle == null || knightVoiceClipBattle.Count == 0)
			{
				Debug.Log("No battle knight voice clips assigned.");
				return;
			}
			int randomIndex = Random.Range(0, knightVoiceClipBattle.Count);	
			sfxSource.clip = knightVoiceClipBattle[randomIndex];
			Debug.Log($"Playing battle knight voice clip: {sfxSource.clip.name}");
			sfxSource.pitch = UnityEngine.Random.Range(0.8f, 1.4f); 
			sfxSource.Play();
		}
		else if (characterType == "unicorn_fight2_0")
		{
			if (unicornVoiceClipBattle == null || unicornVoiceClipBattle.Count == 0)
			{
				Debug.Log("No battle unicorn voice clips assigned.");
				return;
			}
			int randomIndex = Random.Range(0, unicornVoiceClipBattle.Count);	
			sfxSource.clip = unicornVoiceClipBattle[randomIndex];
			Debug.Log($"Playing battle unicorn voice clip: {sfxSource.clip.name}");
			sfxSource.pitch = UnityEngine.Random.Range(0.8f, 1.4f); 
			sfxSource.Play();
		}
		else if (characterType == "slime_angry")
		{
			if (blobVoiceClipBattle == null || blobVoiceClipBattle.Count == 0)
			{
				Debug.Log("No battle blob voice clips assigned.");
				return;
			}
			int randomIndex = Random.Range(0, blobVoiceClipBattle.Count);	
			sfxSource.clip = blobVoiceClipBattle[randomIndex];
			Debug.Log($"Playing battle blob voice clip: {sfxSource.clip.name}");
			sfxSource.pitch = UnityEngine.Random.Range(0.8f, 1.4f); 
			sfxSource.Play();
		}
		else if (characterType == "rocky_fight")
		{
			if (rockyVoiceClipBattle == null || rockyVoiceClipBattle.Count == 0)
			{
				Debug.Log("No battle rocky voice clips assigned.");
				return;
			}
			int randomIndex = Random.Range(0, rockyVoiceClipBattle.Count);	
			sfxSource.clip = rockyVoiceClipBattle[randomIndex];
			Debug.Log($"Playing battle rocky voice clip: {sfxSource.clip.name}");
			sfxSource.pitch = UnityEngine.Random.Range(0.8f, 1.0f); 
			sfxSource.Play();
		}
		else
		{
			Debug.Log("Unknown character type: " + characterType);
			return;
		}
	}




	public void PlayToadVoiceGeneric()
	{
		if (toadVoiceClipGeneric == null || toadVoiceClipGeneric.Count == 0 || sfxSource == null)
		{
			Debug.Log("No generic toad voice clip or sfx source assigned.");
			return;
		}
		int randomIndex = Random.Range(0, toadVoiceClipGeneric.Count);	
		sfxSource.clip = toadVoiceClipGeneric[randomIndex];
		Debug.Log($"Playing generic toad voice clip: {sfxSource.clip.name}");
		sfxSource.Play();
	}
	public void PlayToadVoiceAngry()
	{
		if (toadVoiceClipAngry == null || toadVoiceClipAngry.Count == 0 || sfxSource == null)
		{
			Debug.Log("No angry toad voice clip or sfx source assigned.");
			return;
		}
		int randomIndex = Random.Range(0, toadVoiceClipAngry.Count);	
		sfxSource.clip = toadVoiceClipAngry[randomIndex];
		Debug.Log($"Playing angry toad voice clip: {sfxSource.clip.name}");
		sfxSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f); 
		sfxSource.Play();
	}

	public void PlayAlienVoiceGeneric()
	{
		if (alienVoiceClipGeneric == null || alienVoiceClipGeneric.Count == 0 || sfxSource == null)
		{
			Debug.Log("No generic alien voice clip or sfx source assigned.");
			return;
		}
		int randomIndex = Random.Range(0, alienVoiceClipGeneric.Count);	
		sfxSource.clip = alienVoiceClipGeneric[randomIndex];
		Debug.Log($"Playing generic alien voice clip: {sfxSource.clip.name}");
		sfxSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f); 
		sfxSource.Play();
	}
	public void PlayAlienVoiceAngry()
	{
		if (alienVoiceClipAngry == null || alienVoiceClipAngry.Count == 0 || sfxSource == null)
		{
			Debug.Log("No angry alien voice clip or sfx source assigned.");
			return;
		}
		int randomIndex = Random.Range(0, alienVoiceClipAngry.Count);	
		sfxSource.clip = alienVoiceClipAngry[randomIndex];
		Debug.Log($"Playing angry alien voice clip: {sfxSource.clip.name}");
		sfxSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f); 
		sfxSource.Play();
	}
	public void PlayAlienVoiceSatisfied()
	{
		
		if (alienVoiceClipSatisfied == null || alienVoiceClipSatisfied.Count == 0 || sfxSource == null)
		{
			Debug.Log("No satisfied alien voice clip or sfx source assigned.");
			return;
		}
		int randomIndex = Random.Range(0, alienVoiceClipSatisfied.Count);	
		sfxSource.clip = alienVoiceClipSatisfied[randomIndex];
		Debug.Log($"Playing satisfied alien voice clip: {sfxSource.clip.name}");
		sfxSource.pitch = UnityEngine.Random.Range(1.2f, 1.7f); 
		sfxSource.Play();
	}

		public void PlayBlobVoiceGeneric()
	{
		if (blobVoiceClipGeneric == null || blobVoiceClipGeneric.Count == 0 || sfxSource == null)
		{
			Debug.Log("No generic blob voice clip or sfx source assigned.");
			return;
		}
		int randomIndex = Random.Range(0, blobVoiceClipGeneric.Count);	
		sfxSource.clip = blobVoiceClipGeneric[randomIndex];
		Debug.Log($"Playing generic blob voice clip: {sfxSource.clip.name}");
		sfxSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f); 
		sfxSource.Play();
	}
	public void PlayBlobVoiceAngry()
	{
		if (blobVoiceClipAngry == null || blobVoiceClipAngry.Count == 0 || sfxSource == null)
		{
			Debug.Log("No angry blob voice clip or sfx source assigned.");
			return;
		}
		int randomIndex = Random.Range(0, blobVoiceClipAngry.Count);	
		sfxSource.clip = blobVoiceClipAngry[randomIndex];
		Debug.Log($"Playing angry blob voice clip: {sfxSource.clip.name}");
		sfxSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f); 
		sfxSource.Play();
	}
	public void PlayBlobVoiceSatisfied()
	{
		
		if (blobVoiceClipSatisfied == null || blobVoiceClipSatisfied.Count == 0 || sfxSource == null)
		{
			Debug.Log("No satisfied blob voice clip or sfx source assigned.");
			return;
		}
		int randomIndex = Random.Range(0, blobVoiceClipSatisfied.Count);	
		sfxSource.clip = blobVoiceClipSatisfied[randomIndex];
		Debug.Log($"Playing satisfied blob voice clip: {sfxSource.clip.name}");
		sfxSource.pitch = UnityEngine.Random.Range(1.2f, 1.7f); 
		sfxSource.Play();
	}

	public void PlayUnicornVoiceGeneric()
	{
		if (unicornVoiceClipGeneric == null || unicornVoiceClipGeneric.Count == 0 || sfxSource == null)
		{
			Debug.Log("No generic unicorn voice clip or sfx source assigned.");
			return;
		}
		int randomIndex = Random.Range(0, unicornVoiceClipGeneric.Count);	
		sfxSource.clip = unicornVoiceClipGeneric[randomIndex];
		Debug.Log($"Playing generic unicorn voice clip: {sfxSource.clip.name}");
		sfxSource.pitch = UnityEngine.Random.Range(0.8f, 1.5f); 
		sfxSource.Play();
	}
	public void PlayUnicornVoiceAngry()
	{
		if (unicornVoiceClipAngry == null || unicornVoiceClipAngry.Count == 0 || sfxSource == null)
		{
			Debug.Log("No angry unicorn voice clip or sfx source assigned.");
			return;
		}
		int randomIndex = Random.Range(0, unicornVoiceClipAngry.Count);	
		sfxSource.clip = unicornVoiceClipAngry[randomIndex];
		Debug.Log($"Playing angry unicorn voice clip: {sfxSource.clip.name}");
		sfxSource.pitch = UnityEngine.Random.Range(0.7f, 1.4f); 
		sfxSource.Play();
	}
	public void PlayUnicornVoiceSatisfied()
	{
		
		if (unicornVoiceClipSatisfied == null || unicornVoiceClipSatisfied.Count == 0 || sfxSource == null)
		{
			Debug.Log("No satisfied unicorn voice clip or sfx source assigned.");
			return;
		}
		int randomIndex = Random.Range(0, unicornVoiceClipSatisfied.Count);	
		sfxSource.clip = unicornVoiceClipSatisfied[randomIndex];
		Debug.Log($"Playing satisfied unicorn voice clip: {sfxSource.clip.name}");
		sfxSource.pitch = UnityEngine.Random.Range(1.2f, 1.7f); 
		sfxSource.Play();
	}

	public void PlayKnightVoiceGeneric()
	{
		if (knightVoiceClipGeneric == null || knightVoiceClipGeneric.Count == 0 || sfxSource == null)
		{
			Debug.Log("No generic knight voice clip or sfx source assigned.");
			return;
		}
		int randomIndex = Random.Range(0, knightVoiceClipGeneric.Count);	
		sfxSource.clip = knightVoiceClipGeneric[randomIndex];
		Debug.Log($"Playing generic knight voice clip: {sfxSource.clip.name}");
		sfxSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f); 
		sfxSource.Play();
	}
	public void PlayKnightVoiceAngry()
	{
		if (knightVoiceClipAngry == null || knightVoiceClipAngry.Count == 0 || sfxSource == null)
		{
			Debug.Log("No angry knight voice clip or sfx source assigned.");
			return;
		}
		int randomIndex = Random.Range(0, knightVoiceClipAngry.Count);	
		sfxSource.clip = knightVoiceClipAngry[randomIndex];
		Debug.Log($"Playing angry knight voice clip: {sfxSource.clip.name}");
		sfxSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f); 
		sfxSource.Play();
	}
	public void PlayKnightVoiceSatisfied()
	{
		
		if (knightVoiceClipSatisfied == null || knightVoiceClipSatisfied.Count == 0 || sfxSource == null)
		{
			Debug.Log("No satisfied knight voice clip or sfx source assigned.");
			return;
		}
		int randomIndex = Random.Range(0, knightVoiceClipSatisfied.Count);	
		sfxSource.clip = knightVoiceClipSatisfied[randomIndex];
		Debug.Log($"Playing satisfied knight voice clip: {sfxSource.clip.name}");
		sfxSource.pitch = UnityEngine.Random.Range(1.2f, 1.7f); 
		sfxSource.Play();
	}

	public void PlayRockyVoiceGeneric()
	{
		if (rockyVoiceClipGeneric == null || rockyVoiceClipGeneric.Count == 0 || sfxSource == null)
		{
			Debug.Log("No generic rocky voice clip or sfx source assigned.");
			return;
		}
		int randomIndex = Random.Range(0, rockyVoiceClipGeneric.Count);	
		sfxSource.clip = rockyVoiceClipGeneric[randomIndex];
		Debug.Log($"Playing generic rocky voice clip: {sfxSource.clip.name}");
		sfxSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f); 
		sfxSource.Play();
	}
	public void PlayRockyVoiceAngry()
	{
		if (rockyVoiceClipAngry == null || rockyVoiceClipAngry.Count == 0 || sfxSource == null)
		{
			Debug.Log("No angry rocky voice clip or sfx source assigned.");
			return;
		}
		int randomIndex = Random.Range(0, rockyVoiceClipAngry.Count);	
		sfxSource.clip = rockyVoiceClipAngry[randomIndex];
		Debug.Log($"Playing angry rocky voice clip: {sfxSource.clip.name}");
		sfxSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f); 
		sfxSource.Play();
	}
	public void PlayRockyVoiceSatisfied()
	{
		
		if (rockyVoiceClipSatisfied == null || rockyVoiceClipSatisfied.Count == 0 || sfxSource == null)
		{
			Debug.Log("No satisfied rocky voice clip or sfx source assigned.");
			return;
		}
		int randomIndex = Random.Range(0, rockyVoiceClipSatisfied.Count);	
		sfxSource.clip = rockyVoiceClipSatisfied[randomIndex];
		Debug.Log($"Playing satisfied rocky voice clip: {sfxSource.clip.name}");
		sfxSource.pitch = UnityEngine.Random.Range(1.2f, 1.5f); 
		sfxSource.Play();
	}

	public void PlayHamsterVoiceGeneric()
	{
		if (hamsterVoiceClipGeneric == null || hamsterVoiceClipGeneric.Count == 0 || sfxSource == null)
		{
			Debug.Log("No generic hamster voice clip or sfx source assigned.");
			return;
		}
		int randomIndex = Random.Range(0, hamsterVoiceClipGeneric.Count);	
		sfxSource.clip = hamsterVoiceClipGeneric[randomIndex];
		Debug.Log($"Playing generic hamster voice clip: {sfxSource.clip.name}");
		sfxSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f); 
		sfxSource.Play();
	}
	public void PlayHamsterVoiceAngry()
	{
		if (hamsterVoiceClipAngry == null || hamsterVoiceClipAngry.Count == 0 || sfxSource == null)
		{
			Debug.Log("No angry hamster voice clip or sfx source assigned.");
			return;
		}
		int randomIndex = Random.Range(0, hamsterVoiceClipAngry.Count);	
		sfxSource.clip = hamsterVoiceClipAngry[randomIndex];
		Debug.Log($"Playing angry hamster voice clip: {sfxSource.clip.name}");
		sfxSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f); 
		sfxSource.Play();
	}
	public void PlayHamsterVoiceSatisfied()
	{
		
		if (hamsterVoiceClipSatisfied == null || hamsterVoiceClipSatisfied.Count == 0 || sfxSource == null)
		{
			Debug.Log("No satisfied hamster voice clip or sfx source assigned.");
			return;
		}
		int randomIndex = Random.Range(0, hamsterVoiceClipSatisfied.Count);	
		sfxSource.clip = hamsterVoiceClipSatisfied[randomIndex];
		Debug.Log($"Playing satisfied hamster voice clip: {sfxSource.clip.name}");
		sfxSource.pitch = UnityEngine.Random.Range(1.2f, 2.0f); 
		sfxSource.Play();
	}

	public void PlayDragonVoiceGeneric()
	{
		if (dragonVoiceClipGeneric == null || dragonVoiceClipGeneric.Count == 0 || sfxSource == null)
		{
			Debug.Log("No generic dragon voice clip or sfx source assigned.");
			return;
		}
		int randomIndex = Random.Range(0, dragonVoiceClipGeneric.Count);	
		sfxSource.clip = dragonVoiceClipGeneric[randomIndex];
		Debug.Log($"Playing generic dragon voice clip: {sfxSource.clip.name}");
		sfxSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f); 
		sfxSource.Play();
	}
	public void PlayDragonVoiceAngry()
	{
		if (dragonVoiceClipAngry == null || dragonVoiceClipAngry.Count == 0 || sfxSource == null)
		{
			Debug.Log("No angry dragon voice clip or sfx source assigned.");
			return;
		}
		int randomIndex = Random.Range(0, dragonVoiceClipAngry.Count);	
		sfxSource.clip = dragonVoiceClipAngry[randomIndex];
		Debug.Log($"Playing angry dragon voice clip: {sfxSource.clip.name}");
		sfxSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f); 
		sfxSource.Play();
	}
	public void PlayDragonVoiceSatisfied()
	{
		
		if (dragonVoiceClipSatisfied == null || dragonVoiceClipSatisfied.Count == 0 || sfxSource == null)
		{
			Debug.Log("No satisfied dragon voice clip or sfx source assigned.");
			return;
		}
		int randomIndex = Random.Range(0, dragonVoiceClipSatisfied.Count);	
		sfxSource.clip = dragonVoiceClipSatisfied[randomIndex];
		Debug.Log($"Playing satisfied dragon voice clip: {sfxSource.clip.name}");
		sfxSource.pitch = UnityEngine.Random.Range(1.2f, 2.0f);
		sfxSource.Play();
	}
	public void StopVoice()
	{
		if (sfxSource != null && sfxSource.isPlaying)
		{
			sfxSource.Stop();
		}
	}
}


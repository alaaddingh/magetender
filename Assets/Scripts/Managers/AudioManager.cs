using UnityEngine;
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
}


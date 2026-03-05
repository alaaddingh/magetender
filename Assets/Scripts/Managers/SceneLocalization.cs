using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/* Localizes UI by matching TMP text to a single list of (scene, matchText, key, style). One FindObjectsOfType per scene load.
   Optional: add LocalizedTMPText to labels in the editor with Key + Style set; then this scene scanner can be removed. */
public class SceneLocalization : MonoBehaviour
{
	[Serializable]
	public struct LocalizedEntry
	{
		public string sceneName;
		public string matchText;
		public string key;
		public LocalizedTMPText.Style style;
	}

	private static SceneLocalization s_instance;

	private static readonly LocalizedEntry[] Entries =
	{
		new LocalizedEntry { sceneName = "TitleScreen", matchText = "Start", key = "start_button", style = LocalizedTMPText.Style.Button },
		new LocalizedEntry { sceneName = "TitleScreen", matchText = "Back", key = "back_button", style = LocalizedTMPText.Style.Button },
		new LocalizedEntry { sceneName = "TitleScreen", matchText = "Options", key = "options_button", style = LocalizedTMPText.Style.Button },
		new LocalizedEntry { sceneName = "TitleScreen", matchText = "Credits", key = "credits_button", style = LocalizedTMPText.Style.Button },
		new LocalizedEntry { sceneName = "MixScene", matchText = "Begin Brew", key = "begin_brew_button", style = LocalizedTMPText.Style.Button },
		new LocalizedEntry { sceneName = "MixScene", matchText = "Next Day", key = "next_day_button", style = LocalizedTMPText.Style.Button },
		new LocalizedEntry { sceneName = "MixScene", matchText = "Next", key = "next_button", style = LocalizedTMPText.Style.Button },
		new LocalizedEntry { sceneName = "MixScene", matchText = "Back", key = "back_button", style = LocalizedTMPText.Style.Button },
		new LocalizedEntry { sceneName = "MixScene", matchText = "Mood Profile", key = "mood_profile_title", style = LocalizedTMPText.Style.MoodGraphTitleOrDescription },
		new LocalizedEntry { sceneName = "MixScene", matchText = "Hover icons for mood details", key = "mood_graph_hint", style = LocalizedTMPText.Style.MoodGraphHint },
		new LocalizedEntry { sceneName = "MixScene", matchText = "Adjust your customer's mood", key = "mood_graph_description", style = LocalizedTMPText.Style.MoodGraphTitleOrDescription },
		new LocalizedEntry { sceneName = "MixScene", matchText = "Continue", key = "continue_button", style = LocalizedTMPText.Style.Button },
		new LocalizedEntry { sceneName = "QTECombatScene", matchText = "ATTACK", key = "attack_label", style = LocalizedTMPText.Style.None },
		new LocalizedEntry { sceneName = "QTECombatScene", matchText = "DEFEND", key = "defend_label", style = LocalizedTMPText.Style.None },
		new LocalizedEntry { sceneName = "QTECombatScene", matchText = "PLAYER", key = "player_label", style = LocalizedTMPText.Style.None },
		new LocalizedEntry { sceneName = "QTECombatScene", matchText = "CUSTOMER", key = "customer_label", style = LocalizedTMPText.Style.None },
		new LocalizedEntry { sceneName = "QTECombatScene", matchText = "To Bar", key = "to_bar_button", style = LocalizedTMPText.Style.Button }
	};

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void EnsureInstance()
	{
		if (s_instance != null)
		{
			return;
		}

		GameObject go = new GameObject("SceneLocalization");
		s_instance = go.AddComponent<SceneLocalization>();
		DontDestroyOnLoad(go);

		s_instance.LocalizeActiveScene();
	}

	private void Awake()
	{
		if (s_instance != null && s_instance != this)
		{
			Destroy(gameObject);
			return;
		}

		s_instance = this;
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDestroy()
	{
		if (s_instance == this)
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
			s_instance = null;
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		LocalizeActiveScene();
	}

	private void LocalizeActiveScene()
	{
		string sceneName = SceneManager.GetActiveScene().name;
		TMP_Text[] allTexts = UnityEngine.Object.FindObjectsOfType<TMP_Text>(true);

		for (int e = 0; e < Entries.Length; e++)
		{
			if (Entries[e].sceneName != sceneName)
			{
				continue;
			}

			for (int t = 0; t < allTexts.Length; t++)
			{
				TMP_Text tmp = allTexts[t];
				if (tmp.text.Trim() != Entries[e].matchText)
				{
					continue;
				}

				LocalizedTMPText loc = tmp.GetComponent<LocalizedTMPText>();
				if (loc == null)
				{
					loc = tmp.gameObject.AddComponent<LocalizedTMPText>();
				}

				loc.SetKeyAndStyle(Entries[e].key, Entries[e].style);
			}
		}

		if (sceneName == "MixScene")
		{
			CapMoodGraphLabelFontSizes();
			Invoke(nameof(CapMoodGraphLabelFontSizesDelayed), 0.5f);
		}
	}

	private void CapMoodGraphLabelFontSizesDelayed()
	{
		CapMoodGraphLabelFontSizes();
	}

	private static void CapMoodGraphLabelFontSizes()
	{
		string[] moodLabels = { "calm", "Calm", "calmo", "elevated", "Elevated", "Elevado", "grounded", "Grounded", "Aterrizado", "aterrizado", "dissociative", "Disociativo", "disociativo" };
		TMP_Text[] allTexts = UnityEngine.Object.FindObjectsOfType<TMP_Text>(true);
		const float maxSize = 22f;
		for (int i = 0; i < allTexts.Length; i++)
		{
			TMP_Text tmp = allTexts[i];
			string t = tmp.text.Trim();
			for (int j = 0; j < moodLabels.Length; j++)
			{
				if (t == moodLabels[j] && tmp.fontSize > maxSize)
				{
					tmp.fontSize = maxSize;
					break;
				}
			}
		}
	}
}

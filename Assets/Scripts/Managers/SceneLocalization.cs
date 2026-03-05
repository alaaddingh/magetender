using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/* Runs on scene load to cap mood graph quadrant label font sizes in MixScene. All other UI is localized via LocalizedTMPText in the editor. */
public class SceneLocalization : MonoBehaviour
{
	private static SceneLocalization s_instance;

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

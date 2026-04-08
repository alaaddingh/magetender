using UnityEngine;
using UnityEngine.SceneManagement;
using Magetender.Data;

public class TitleMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject creditsPanel;

	[Header("Main menu buttons (optional; auto-resolve by name if empty)")]
	[SerializeField] private GameObject startButtonRoot;
	[SerializeField] private GameObject newGameButtonRoot;
	[SerializeField] private LocalizedTMPText startButtonLabel;

	private void Start()
	{
		AutoResolveRefs();
		RefreshMainMenuButtons();
	}

    public void StartGame()
    {
        SceneManager.LoadScene("MixScene");
    }

    private const string CombatTutorialCompletedKey = "CombatTutorialCompleted";

    public void StartNewGame()
    {
        SaveSystem.ClearSave();
        PlayerPrefs.DeleteKey(CombatTutorialCompletedKey);
        PlayerPrefs.Save();
        if (CurrentMonster.Instance != null)
            CurrentMonster.Instance.ResetToFirstMonster();
        if (GameManager.Instance != null)
            GameManager.Instance.ResetForNewGame();
        SceneManager.LoadScene("MixScene");
    }

	private void AutoResolveRefs()
	{
		if (startButtonRoot == null)
		{
			var go = GameObject.Find("Start");
			if (go != null) startButtonRoot = go;
		}
		if (newGameButtonRoot == null)
		{
			var go = GameObject.Find("NewGame");
			if (go != null) newGameButtonRoot = go;
		}
		if (startButtonLabel == null && startButtonRoot != null)
		{
			startButtonLabel = startButtonRoot.GetComponentInChildren<LocalizedTMPText>(includeInactive: true);
		}
	}

	private void RefreshMainMenuButtons()
	{
		bool hasSave = SaveSystem.LoadGame() != null;

		// UI constraint: buttons stay present; only the label changes.
		// No save => "Start", Has save => "Continue"

		if (startButtonLabel != null)
			startButtonLabel.SetKey(hasSave ? "continue_button" : "start_button");
	}

    public void ShowCredits()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        if (creditsPanel != null)
            creditsPanel.SetActive(true);
    }

    public void HideCredits()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }
}

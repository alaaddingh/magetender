using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Magetender.Data;

public class TitleMenu : MonoBehaviour
{
	private const string QteSceneName = "QTECombatScene";
	private const string LoseSceneName = "LoseScene";

    [Header("Panels")]
    [SerializeField] private GameObject creditsPanel;

	[Header("Main menu buttons (optional; auto-resolve by name if empty)")]
	[SerializeField] private GameObject startButtonRoot;
	[SerializeField] private GameObject newGameButtonRoot;
	[SerializeField] private LocalizedTMPText startButtonLabel;

	[Header("Button container")]
	[SerializeField] private RectTransform buttonContainer;
	[Tooltip("Added to the container's scene anchored Y when there is no save.")]
	[SerializeField] private float buttonContainerOffsetYFirstPlay;
	[Tooltip("Added to the container's scene anchored Y when a save exists.")]
	[SerializeField] private float buttonContainerOffsetYContinue;

	[Header("First play vs continue (optional)")]
	[SerializeField] private GameObject firstPlayOnlyVisualRoot;
	[SerializeField] private GameObject continueOnlyVisualRoot;
	[SerializeField] private Sprite firstPlayBackdropSprite;
	[SerializeField] private Sprite continueBackdropSprite;

	private const string TitleBackdropChildName = "Background";
	private const string ButtonContainerChildName = "ButtonContainer";
	private Image cachedTitleBackdropImage;
	private Vector2 buttonContainerSceneAnchoredPosition;
	private bool buttonContainerBaseCaptured;

	private void Start()
	{
		GameAnalytics.InitializeIfNeeded();
		AutoResolveRefs();
		RefreshMainMenuButtons();
	}

    public void StartGame()
    {
		// DDOL managers keep in-memory state; reload disk so unpaid rewards (e.g. assessment) are dropped.
		SaveData data = SaveSystem.LoadGame();
		if (data != null)
		{
			bool resumeLose = data.resumeLoseScreenOnContinue;
			if (resumeLose)
			{
				data.resumeLoseScreenOnContinue = false;
				SaveSystem.SaveGame(data);
			}

			if (GameManager.Instance != null)
				GameManager.Instance.LoadProgressFromSave(data);
			if (CurrentMonster.Instance != null)
			{
				CurrentMonster.Instance.ClearPendingVisitPlan();
				CurrentMonster.Instance.ApplySaveProgress(data.currentEncounterIndex);
			}

			if (resumeLose)
			{
				SceneManager.LoadScene(LoseSceneName);
				return;
			}
		}

		bool goCombat = GameManager.Instance != null && GameManager.Instance.PendingBarFight;
		if (goCombat && GameManager.Instance != null)
			GameManager.Instance.RequestOpenPauseMenuOnNextFightSceneLoad();
        SceneManager.LoadScene(goCombat ? QteSceneName : "MixScene");
    }

    private const string CombatTutorialCompletedKey = "CombatTutorialCompleted";

    public void StartNewGame()
    {
        SaveSystem.ClearSave();
        PlayerPrefs.DeleteKey(CombatTutorialCompletedKey);
        PlayerPrefs.Save();
        if (GameManager.Instance != null)
            GameManager.Instance.ResetForNewGame(preserveTutorialCompleted: false);
        if (CurrentMonster.Instance != null)
            CurrentMonster.Instance.ResetToFirstMonster();
		GameAnalytics.InitializeIfNeeded();
		GameAnalytics.RecordPlaythroughStarted();
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
		if (buttonContainer == null)
		{
			var t = transform.Find(ButtonContainerChildName);
			if (t != null)
				buttonContainer = t as RectTransform;
		}
	}

	private void RefreshMainMenuButtons()
	{
		bool hasSave = SaveSystem.LoadGame() != null;

		if (newGameButtonRoot != null)
			newGameButtonRoot.SetActive(hasSave);

		if (firstPlayOnlyVisualRoot != null)
			firstPlayOnlyVisualRoot.SetActive(!hasSave);
		if (continueOnlyVisualRoot != null)
			continueOnlyVisualRoot.SetActive(hasSave);

		ApplyBackdropSprites(hasSave);
		ApplyButtonContainerLayout(hasSave);

		if (startButtonLabel != null)
			startButtonLabel.SetKey(hasSave ? "continue_button" : "start_button");
	}

	private void ApplyButtonContainerLayout(bool hasSave)
	{
		if (buttonContainer == null)
			return;
		if (!buttonContainerBaseCaptured)
		{
			buttonContainerSceneAnchoredPosition = buttonContainer.anchoredPosition;
			buttonContainerBaseCaptured = true;
		}
		float extraY = hasSave ? buttonContainerOffsetYContinue : buttonContainerOffsetYFirstPlay;
		Vector2 b = buttonContainerSceneAnchoredPosition;
		buttonContainer.anchoredPosition = new Vector2(b.x, b.y + extraY);
	}

	private Image GetTitleBackdropImage()
	{
		if (cachedTitleBackdropImage != null)
			return cachedTitleBackdropImage;
		var t = transform.Find(TitleBackdropChildName);
		if (t != null)
			cachedTitleBackdropImage = t.GetComponent<Image>();
		return cachedTitleBackdropImage;
	}

	private void ApplyBackdropSprites(bool hasSave)
	{
		if (firstPlayBackdropSprite == null || continueBackdropSprite == null)
			return;
		var img = GetTitleBackdropImage();
		if (img != null)
			img.sprite = hasSave ? continueBackdropSprite : firstPlayBackdropSprite;
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

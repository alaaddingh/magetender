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
	[SerializeField] private VerticalLayoutGroup buttonContainerLayout;
	[Tooltip("Vertical Layout Group spacing when showing 3-button art (no save).")]
	[SerializeField] private float buttonContainerSpacingFirstPlay = 40f;
	[Tooltip("Vertical Layout Group spacing when showing 4-button art (save exists).")]
	[SerializeField] private float buttonContainerSpacingContinue = 40f;

	[Header("Menu button art (3 vs 4 buttons, optional)")]
	[Tooltip("Image on your button-art object (e.g. TitlePanel). Sprite swaps; background stays fixed.")]
	[SerializeField] private Image menuButtonArtImage;
	[SerializeField] private Sprite firstPlayButtonArtSprite;
	[SerializeField] private Sprite continueButtonArtSprite;
	[Tooltip("Alternative to sprite swap: enable one root for first play, the other when a save exists.")]
	[SerializeField] private GameObject firstPlayOnlyVisualRoot;
	[SerializeField] private GameObject continueOnlyVisualRoot;

	private static readonly string[] MenuButtonArtChildNames = { "TitleButtons", "TitlePanel" };
	private const string ButtonContainerChildName = "ButtonContainer";
	private Vector2 buttonContainerSceneAnchoredPosition;
	private bool buttonContainerBaseCaptured;

	private void Start()
	{
		GameAnalytics.InitializeIfNeeded();
		AutoResolveRefs();
		EnsureDecorativeLayersDoNotBlockClicks();
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
			buttonContainer = FindRectTransform(ButtonContainerChildName);
		if (buttonContainerLayout == null && buttonContainer != null)
			buttonContainerLayout = buttonContainer.GetComponent<VerticalLayoutGroup>();
		if (menuButtonArtImage == null)
		{
			for (int i = 0; i < MenuButtonArtChildNames.Length; i++)
			{
				var t = transform.Find(MenuButtonArtChildNames[i]);
				if (t == null)
					continue;
				menuButtonArtImage = t.GetComponent<Image>();
				if (menuButtonArtImage != null)
					break;
			}
		}
	}

	private RectTransform FindRectTransform(string objectName)
	{
		var transforms = GetComponentsInChildren<RectTransform>(includeInactive: true);
		for (int i = 0; i < transforms.Length; i++)
		{
			if (transforms[i].name == objectName)
				return transforms[i];
		}
		return null;
	}

	private void EnsureDecorativeLayersDoNotBlockClicks()
	{
		if (menuButtonArtImage != null)
		{
			menuButtonArtImage.raycastTarget = false;
			if (buttonContainer != null)
			{
				var artTransform = menuButtonArtImage.rectTransform;
				if (artTransform.parent == buttonContainer.parent
					&& artTransform.GetSiblingIndex() >= buttonContainer.GetSiblingIndex())
				{
					artTransform.SetSiblingIndex(buttonContainer.GetSiblingIndex());
				}
			}
		}

		var background = transform.Find("Background");
		if (background != null)
		{
			var backgroundImage = background.GetComponent<Image>();
			if (backgroundImage != null)
				backgroundImage.raycastTarget = false;
		}

		var smoke = transform.Find("Smoke");
		if (smoke != null)
		{
			var smokeImage = smoke.GetComponent<Image>();
			if (smokeImage != null)
				smokeImage.raycastTarget = false;
		}
	}

	private void RefreshMainMenuButtons()
	{
		bool hasSave = SaveSystem.LoadGame() != null;

		if (newGameButtonRoot != null)
			newGameButtonRoot.SetActive(hasSave);

		ApplyMenuButtonArt(hasSave);
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

		if (buttonContainerLayout != null)
		{
			buttonContainerLayout.spacing = hasSave
				? buttonContainerSpacingContinue
				: buttonContainerSpacingFirstPlay;
			LayoutRebuilder.ForceRebuildLayoutImmediate(buttonContainer);
		}
	}

	private void ApplyMenuButtonArt(bool hasSave)
	{
		if (firstPlayOnlyVisualRoot != null)
			firstPlayOnlyVisualRoot.SetActive(!hasSave);
		if (continueOnlyVisualRoot != null)
			continueOnlyVisualRoot.SetActive(hasSave);

		if (menuButtonArtImage == null || firstPlayButtonArtSprite == null || continueButtonArtSprite == null)
			return;
		menuButtonArtImage.sprite = hasSave ? continueButtonArtSprite : firstPlayButtonArtSprite;
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

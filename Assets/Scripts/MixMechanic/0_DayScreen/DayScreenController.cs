using System.Collections;
using TMPro;
using UnityEngine;
using Magetender.Data;

/* Shows day and coins; Next button hides day panel and shows order screen. */
public class DayScreenController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject dayPanel;
    [SerializeField] private GameObject orderScreen;

    [Header("Day screen UI")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text coinsText;

    [Header("Global coin canvas - show only on Day / Order / Assess")]
    [SerializeField] private GameObject coinCanvas;

    [Header("Maintenance cost loss popup (when surviving to next day)")]
    [SerializeField] private TMP_Text coinLossPopup;
    [SerializeField] private float coinLossFadeDuration = 1.5f;
    [SerializeField] private float coinLossMoveUp = 30f;

    [Header("Score Display Canvas - hide on Day, show on Order")]
    [SerializeField] private GameObject scoreDisplayCanvas;
    [SerializeField] private ScoreManager scoreManager;

	private bool playedFirstWalkIn;
	private bool showedDayPanelThisLoad;

	public static bool OrderScreenRevealedThisSession { get; private set; }

    private void Start()
    {
		playedFirstWalkIn = false;
		OrderScreenRevealedThisSession = false;

        if (orderScreen == null)
        {
            var dc = FindObjectOfType<DialogueController>();
            if (dc != null) orderScreen = dc.orderScreen;
        }

        bool skipDayCounter = CurrentMonster.Instance != null && CurrentMonster.Instance.IsPlannedVisitSameDay();
        showedDayPanelThisLoad = !skipDayCounter;
        if (CurrentMonster.Instance != null)
            CurrentMonster.Instance.ApplyPlannedVisit();
        DisableTutorialManagerIfNotNeeded();

        if (dayPanel != null)
            dayPanel.SetActive(true);
        if (orderScreen != null)
            orderScreen.SetActive(false);

        if (coinCanvas != null)
            coinCanvas.SetActive(true);

        if (scoreDisplayCanvas != null)
            scoreDisplayCanvas.SetActive(false);

        RefreshDisplay();

        if (!skipDayCounter && GameManager.Instance != null && GameManager.Instance.Day > 1)
        {
            int maintenance = CurrentMonster.Instance != null ? CurrentMonster.Instance.GetCurrentMaintenanceCost() : 0;
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayRegisterChaChing();
            if (coinLossPopup != null && maintenance > 0)
                ShowCoinLossPopup(maintenance);
        }

        if (skipDayCounter)
            OnNextPressedInternal(false);
    }

    public void RefreshDisplay()
    {
        int day = 0;
        int coins = 0;
        if (GameManager.Instance != null)
        {
            day = GameManager.Instance.Day;
            coins = GameManager.Instance.Coins;
        }

        string dayLabel = L.Get("day_count", day);
        if (dayText != null)
        {
            // Keep day formatting consistent with the rest of the UI.
            bool isArabic = RtlText.IsArabic();
            dayText.isRightToLeftText = isArabic;
            dayText.text = isArabic
                ? RtlText.FixIfArabic(dayLabel, preserveNumbers: true, fixTags: true, reverseOutput: true)
                : dayLabel;

            if (coinsText == null)
                dayText.text += "\n" + coins;
        }
        if (coinsText != null)
            coinsText.text = coins.ToString();
    }

    private void DisableTutorialManagerIfNotNeeded()
    {
        var tutorialManager = FindFirstObjectByType<TutorialManager>();
        if (tutorialManager == null)
            return;

        SaveData saveData = SaveSystem.LoadGame();
        bool tutorialCompleted = (GameManager.Instance != null && GameManager.Instance.TutorialCompleted) ||
                                 (saveData != null && saveData.tutorialCompleted);
        bool isTutorialLevel = CurrentMonster.Instance != null && CurrentMonster.Instance.GetCurrentLevelId() == "tutorial";

        if (!tutorialCompleted && isTutorialLevel)
            return;

        tutorialManager.gameObject.SetActive(false);
    }

    /* Call from Next button OnClick */
    public void OnNextPressed()
    {
        OnNextPressedInternal(true);
    }

    // Shared implementation; Start() can call this with playButtonClick = false
    private void OnNextPressedInternal(bool playButtonClick)
    {
		if (playButtonClick && AudioManager.Instance != null)
			AudioManager.Instance.PlayButtonClick();
		if (showedDayPanelThisLoad && AudioManager.Instance != null)
			AudioManager.Instance.PlayStartOfDayBell();
        if (dayPanel != null)
            dayPanel.SetActive(false);
        if (orderScreen != null)
            orderScreen.SetActive(true);
        if (coinCanvas != null)
            coinCanvas.SetActive(true);
        OrderScreenRevealedThisSession = true;

		if (!playedFirstWalkIn && AudioManager.Instance != null)
		{
			playedFirstWalkIn = true;
			if (GameOverButton.CameFromRetry)
				GameOverButton.ClearCameFromRetry();
			else
				AudioManager.Instance.PlayMonsterWalkIn();
		}
		if (AudioManager.Instance != null)
			AudioManager.Instance.PlayAmbience();

        if (scoreManager != null)
            scoreManager.RefreshScoreDisplay();
        if (scoreDisplayCanvas != null)
            scoreDisplayCanvas.SetActive(true);
        var graph = FindObjectOfType<MoodGraphUI>();
        if (graph != null)
            graph.ForceRefresh();
    }

    private void ShowCoinLossPopup(int amount)
    {
        if (coinLossPopup == null) return;
        coinLossPopup.text = "-" + amount;
        coinLossPopup.gameObject.SetActive(true);
        CanvasGroup cg = coinLossPopup.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = coinLossPopup.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        var rect = coinLossPopup.rectTransform;
        Vector2 startPos = rect.anchoredPosition;
        StartCoroutine(FadeOutCoinLossPopup(rect, startPos));
    }

    private IEnumerator FadeOutCoinLossPopup(RectTransform rect, Vector2 startPos)
    {
        float elapsed = 0f;
        CanvasGroup cg = coinLossPopup.GetComponent<CanvasGroup>();
        while (elapsed < coinLossFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / coinLossFadeDuration;
            if (cg != null)
                cg.alpha = 1f - t;
            rect.anchoredPosition = startPos + new Vector2(0f, coinLossMoveUp * t);
            yield return null;
        }
        if (cg != null)
            cg.alpha = 0f;
        coinLossPopup.gameObject.SetActive(false);
        rect.anchoredPosition = startPos;
    }

	void OnDisable()
	{
		if (coinLossPopup != null)
			coinLossPopup.gameObject.SetActive(false);
	}
}

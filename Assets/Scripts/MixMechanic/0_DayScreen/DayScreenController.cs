using TMPro;
using UnityEngine;

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

    [Header("Score Display Canvas - hide on Day, show on Order")]
    [SerializeField] private GameObject scoreDisplayCanvas;
    [SerializeField] private ScoreManager scoreManager;

	private bool playedFirstWalkIn;
	private bool showedDayPanelThisLoad;

    private void Start()
    {
		playedFirstWalkIn = false;

        if (orderScreen == null)
        {
            var dc = FindObjectOfType<DialogueController>();
            if (dc != null) orderScreen = dc.orderScreen;
        }

        bool skipDayCounter = CurrentMonster.Instance != null && CurrentMonster.Instance.IsPlannedVisitSameDay();
        showedDayPanelThisLoad = !skipDayCounter;
        if (CurrentMonster.Instance != null)
            CurrentMonster.Instance.ApplyPlannedVisit();

        if (dayPanel != null)
            dayPanel.SetActive(true);
        if (orderScreen != null)
            orderScreen.SetActive(false);

        if (coinCanvas != null)
            coinCanvas.SetActive(true);

        if (scoreDisplayCanvas != null)
            scoreDisplayCanvas.SetActive(false);

        RefreshDisplay();

        if (skipDayCounter)
            OnNextPressed();
    }

    public void RefreshDisplay()
    {
        int day = 1;
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

    /* Call from Next button OnClick */
    public void OnNextPressed()
    {
		if (AudioManager.Instance != null)
			AudioManager.Instance.PlayButtonClick();
		if (showedDayPanelThisLoad && AudioManager.Instance != null)
			AudioManager.Instance.PlayStartOfDayBell();
        if (dayPanel != null)
            dayPanel.SetActive(false);
        if (orderScreen != null)
            orderScreen.SetActive(true);

		if (!playedFirstWalkIn && AudioManager.Instance != null)
		{
			playedFirstWalkIn = true;
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
}

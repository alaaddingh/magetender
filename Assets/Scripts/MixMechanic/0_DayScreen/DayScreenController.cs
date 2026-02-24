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

    private void Start()
    {
        if (orderScreen == null)
        {
            var dc = FindObjectOfType<DialogueController>();
            if (dc != null) orderScreen = dc.orderScreen;
        }

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

        if (dayText != null)
            dayText.text = coinsText == null
                ? "Day " + day + "\n" + coins + " coins"
                : "Day " + day;
        if (coinsText != null)
            coinsText.text = coins + " coins";
    }

    /* Call from Next button OnClick */
    public void OnNextPressed()
    {
        if (dayPanel != null)
            dayPanel.SetActive(false);
        if (orderScreen != null)
            orderScreen.SetActive(true);
        if (scoreManager != null)
            scoreManager.RefreshScoreDisplay();
        if (scoreDisplayCanvas != null)
            scoreDisplayCanvas.SetActive(true);
        var graph = FindObjectOfType<MoodGraphUI>();
        if (graph != null)
            graph.ForceRefresh();
    }
}
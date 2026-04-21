using System.Collections;
using UnityEngine;
using TMPro;

public class LoseScreenController : MonoBehaviour
{
    [Header("Coin UI - show maintenance cost that was unaffordable")]
    [SerializeField] private GameObject coinCanvas;
    [SerializeField] private TMP_Text coinsDisplay;
    [SerializeField] private TMP_Text coinLossPopup;
    [SerializeField] private float coinLossFadeDuration = 1.5f;
    [SerializeField] private float coinLossMoveUp = 30f;

    private void Start()
    {
        if (coinCanvas != null)
            coinCanvas.SetActive(true);

        if (GameManager.Instance == null) return;
        int maintenance = CurrentMonster.Instance != null ? CurrentMonster.Instance.GetCurrentMaintenanceCost() : 0;
        RefreshCoinsDisplay(maintenance);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayRegisterChaChing();
        if (coinLossPopup != null)
            ShowCoinLossPopup(maintenance);
    }

    private void RefreshCoinsDisplay(int maintenanceCost)
    {
        if (coinsDisplay == null) return;
        int coins = GameManager.Instance != null ? GameManager.Instance.Coins : 0;
        int afterMaintenance = coins - maintenanceCost;
        coinsDisplay.text = afterMaintenance.ToString();
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
}

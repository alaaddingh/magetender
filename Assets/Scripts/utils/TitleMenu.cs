using UnityEngine;
using UnityEngine.SceneManagement;
using Magetender.Data;

public class TitleMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject creditsPanel;

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

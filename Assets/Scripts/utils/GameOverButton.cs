using UnityEngine;
using UnityEngine.SceneManagement;
using Magetender.Data;

public class GameOverButton : MonoBehaviour
{
    private const string CombatTutorialCompletedKey = "CombatTutorialCompleted";

    public string MixScene = "MixScene";

    public static bool CameFromRetry { get; private set; }

    public static void ClearCameFromRetry()
    {
        CameFromRetry = false;
    }

    public void OnPress()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        CameFromRetry = true;
        int tutorialCompleted = PlayerPrefs.GetInt(CombatTutorialCompletedKey, 0);
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt(CombatTutorialCompletedKey, tutorialCompleted);
        PlayerPrefs.Save();

        if (GameManager.Instance != null)
            GameManager.Instance.AddCoins(-GameManager.Instance.Coins);
        if (CurrentMonster.Instance != null)
            CurrentMonster.Instance.ResetToFirstMonster();
        SaveSystem.WriteData();

        SceneManager.LoadScene(MixScene);
    }
}

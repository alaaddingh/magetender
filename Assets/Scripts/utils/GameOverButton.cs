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
        var saveData = SaveSystem.LoadGame();
        bool tutorialCompleted = (GameManager.Instance != null && GameManager.Instance.TutorialCompleted) ||
                                 (saveData != null && saveData.tutorialCompleted);

		// Match TitleMenu.StartNewGame reset behavior; only difference is tutorialCompleted is preserved here.
		SaveSystem.ClearSave();
		PlayerPrefs.Save();

        if (GameManager.Instance != null)
            GameManager.Instance.ResetForNewGame(tutorialCompleted);
        if (CurrentMonster.Instance != null)
            CurrentMonster.Instance.ResetToFirstMonster();

        SceneManager.LoadScene(MixScene);
    }
}

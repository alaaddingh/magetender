using UnityEngine;
using UnityEngine.SceneManagement;
using Magetender.Data;

public class TitleMenu : MonoBehaviour
{
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
}

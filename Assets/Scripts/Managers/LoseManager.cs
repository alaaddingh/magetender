using UnityEngine;
using UnityEngine.SceneManagement;
using Magetender.Data;

public class LoseManager : MonoBehaviour
{
    public string loseSceneName = "LoseScene";

    public bool CheckLoseAndLoad()
    {
        int maintenanceCost = CurrentMonster.Instance != null ? CurrentMonster.Instance.GetCurrentMaintenanceCost() : 0;
        int earnedCoins = GameManager.Instance != null ? GameManager.Instance.Coins : 0;

        if (earnedCoins < maintenanceCost)
        {
            LoadLoseScene();
            return true;
        }

        return false;
    }

    public void LoadLoseScene()
    {
        // save lose-state to avoid loophole */
        SaveSystem.WriteLoseState();

        SceneManager.LoadScene(loseSceneName);
    }
}

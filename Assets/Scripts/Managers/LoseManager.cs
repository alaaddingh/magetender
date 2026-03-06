using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseManager : MonoBehaviour
{
    public string loseSceneName = "LoseScene";

    public bool CheckLoseAndLoad()
    {
        int maintenanceCost = GameManager.Instance.MaintenanceCost;
        int earnedCoins = GameManager.Instance.Coins;

        if (earnedCoins < maintenanceCost)
        {
            LoadLoseScene();
            return true;
        }

        return false;
    }

    public void LoadLoseScene()
    {
        SceneManager.LoadScene(loseSceneName);
    }
}

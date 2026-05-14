using UnityEngine;
using UnityEngine.SceneManagement;
using Magetender.Data;

public class LoseManager : MonoBehaviour
{
    public string loseSceneName = "LoseScene";

	public static bool WouldLoseIfEndOfDayAdvanceNow()
	{
		var cm = CurrentMonster.Instance;
		var gm = GameManager.Instance;
		if (cm == null || gm == null)
			return false;

		if (cm.GetCurrentLevelId() == "tutorial")
			return false;

		if (cm.HasNextMonsterInCurrentLevel())
			return false;

		int maintenanceCost = cm.GetMaintenanceCostForDay(gm.Day + 1);
		return gm.Coins < maintenanceCost;
	}

    public bool CheckLoseAndLoad()
    {
		if (!WouldLoseIfEndOfDayAdvanceNow())
			return false;

		LoadLoseScene();
		return true;
    }

    public void LoadLoseScene()
    {
        // save lose-state to avoid loophole */
        SaveSystem.WriteLoseState();

        SceneManager.LoadScene(loseSceneName);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using Magetender.Data;

public class GameOverButton : MonoBehaviour
{
    public string MixScene = "MixScene";

    public void OnPress()
    {
		if (AudioManager.Instance != null)
			AudioManager.Instance.PlayButtonClick();
            /* FOR NOW: resets coin to zero when you lose. This will need a 
            better solution when more levels are added */
            GameManager.Instance.AddCoins(-GameManager.Instance.Coins);
            CurrentMonster.Instance.ResetToFirstMonster();
            SaveSystem.WriteData();

        SceneManager.LoadScene(MixScene);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverButton : MonoBehaviour
{
    public string MixScene = "MixScene";

    public void OnPress()
    {
            /* FOR NOW: resets coin to zero when you lose. This will need a 
            better solution when more levels are added */
            GameManager.Instance.AddCoins(-GameManager.Instance.Coins);
            CurrentMonster.Instance.ResetToFirstMonster();

        SceneManager.LoadScene(MixScene);
    }
}

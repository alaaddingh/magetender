using UnityEngine;
using UnityEngine.SceneManagement;
using Magetender.Data;

public class TitleMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("MixScene");
    }

    public void StartNewGame()
    {
        SaveSystem.ClearSave();
        SceneManager.LoadScene("MixScene");
    }
}

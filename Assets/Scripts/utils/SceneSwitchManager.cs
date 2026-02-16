/*
Purpose of file: 
general standalone 'on click' function to switch to a new scene, 
used right now to switch to QTE scene */
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public void SwitchScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
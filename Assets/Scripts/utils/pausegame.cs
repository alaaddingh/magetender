using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-200)]
public class PauseGame : MonoBehaviour
{	[SerializeField] private GameObject pauseMenuPanel; 
	[SerializeField] private string menuSceneName = "TitleScreen";
	public bool IsPaused {get; private set;}

	void Awake()
	{
		if (pauseMenuPanel != null)
			pauseMenuPanel.SetActive(false);
	}

	void Start()
	{
		if (pauseMenuPanel == null || GameManager.Instance == null)
			return;
		if (!GameManager.Instance.ConsumeOpenPauseMenuOnNextFightSceneLoad())
			return;
		Pause();
	}

	void OnDisable()
	{
		if (IsPaused)
			ResumeGame();
	}

	public void TogglePause()
	{
		if (IsPaused) ResumeGame();
		else Pause();
	}

	public void Pause()
	{
		IsPaused = true;
		Time.timeScale = 0f;
		AudioListener.pause = true;
		pauseMenuPanel.SetActive(true);
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	public void ResumeGame()
	{
		IsPaused = false;
		Time.timeScale = 1f;
		AudioListener.pause = false;

		if (pauseMenuPanel != null)
			pauseMenuPanel.SetActive(false);
	}

	public void ReturnToMenu()
	{
		IsPaused = false;
		Time.timeScale = 1f;
		AudioListener.pause = false;

		if (AudioManager.Instance != null)
			AudioManager.Instance.StopAllSounds();

		FightSceneQuitSave.TryCommitMidFightSaveIfInFightScene();
		AssessmentMenuExit.TryCommitSaveBeforeReturningToTitle();

		if (!string.IsNullOrEmpty(menuSceneName))
			SceneManager.LoadScene(menuSceneName);
	}
}

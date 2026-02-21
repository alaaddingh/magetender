using UnityEngine;
using UnityEngine.SceneManagement;

public class ContinueButtonUtils : MonoBehaviour
{
    [Header("Optional refs (auto-resolve if empty)")]
    [SerializeField] private CurrentMonster currentMonsterManager;
    [SerializeField] private MonsterStateManager monsterStateManager;
    [SerializeField] private ScoreManager scoreManager;

    [Header("Scene to reload after continue")]
    [SerializeField] private string sceneName = "MixScene";

    [Header("Optional UI")]
    public GameObject continueButtonObject;

    private void Start()
    {
        RefreshContinueButtonVisibility();
    }

    private void Update()
    {
        RefreshContinueButtonVisibility();
    }

    // Hook this to the Continue button OnClick.
    public void ContinueToNextMonster()
    {
        if (currentMonsterManager == null)
            currentMonsterManager = CurrentMonster.Instance;

        bool advanced = currentMonsterManager != null && currentMonsterManager.AdvanceToNextMonster();
        if (!advanced)
            return;

        if (monsterStateManager != null)
            monsterStateManager.SetState("neutral");

        if (scoreManager != null)
            scoreManager.RefreshScoreDisplay();

        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
    }

    private void RefreshContinueButtonVisibility()
    {
        if (currentMonsterManager == null)
            currentMonsterManager = CurrentMonster.Instance;
        if (continueButtonObject == null || currentMonsterManager == null)
            return;

        continueButtonObject.SetActive(currentMonsterManager.HasNextMonsterInCurrentLevel());
    }
}

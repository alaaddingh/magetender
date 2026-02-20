/* This file:
1) Assesses the monster's mood upon serving (by communicating w/ ScoreManager)
2) Sets the monster's state (neutral/satisfied/angry) in MonsterStateManager
3) makes fight button visible if you rly messed up
4) shows coins on assess screen; Next button goes to next day (reload day screen)
*/

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class AssessController : MonoBehaviour
{
    [Header("refs")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private MonsterStateManager MonsterStateManager;
    [SerializeField] private CurrentMonster currentMonsterManager;

    [Header("UI")]
    [SerializeField] private TMP_Text coinsDisplay;
    [SerializeField] private GameObject coinCanvas;
    [SerializeField] private string nextDaySceneName = "MixScene";

    [Header("Coin rewards by mood (sweet spot = most, angry = 0)")]
    [SerializeField] private int coinsSatisfied = 30;
    [SerializeField] private int coinsNeutral = 15;
    [SerializeField] private int coinsAngry = 0;

    public float MixAccuracy; /* just so we can see the accuracy as a perctenage */

    public GameObject FightButton;

    private void Awake()
    {
        if (currentMonsterManager == null)
            currentMonsterManager = CurrentMonster.Instance;

        MixAccuracy = AssessAccuracy();
        AssessState(MixAccuracy);
        AwardCoinsForDrink();

        if (coinCanvas != null)
            coinCanvas.SetActive(true);

        RefreshCoinsDisplay();
    }

    private void AwardCoinsForDrink()
    {
        if (GameManager.Instance == null) return;
        string state = MonsterStateManager.MonsterState;
        if (state == "satisfied") GameManager.Instance.AddCoins(coinsSatisfied);
        else if (state == "neutral") GameManager.Instance.AddCoins(coinsNeutral);
        else if (state == "angry") GameManager.Instance.AddCoins(coinsAngry);
    }

    /* call from Assess Next button OnClick: go to next day and show day screen again */
    public void OnNextCustomerPressed()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.IncrementDay();
        if (currentMonsterManager != null)
            currentMonsterManager.AdvanceToNextMonster();
        if (!string.IsNullOrEmpty(nextDaySceneName))
            SceneManager.LoadScene(nextDaySceneName);
    }

    private void RefreshCoinsDisplay()
    {
        if (coinsDisplay == null) return;
        int coins = GameManager.Instance != null ? GameManager.Instance.Coins : 0;
        coinsDisplay.text = coins + " coins";
    }

    /* assesses percentage difference of the final mix's X and Y,
     returns accuracy in form of a whole number*/
    private float AssessAccuracy()
    {
        ScorePair goal = currentMonsterManager != null ? currentMonsterManager.GetGoalScore() : null;
        if (goal == null)
            return 0f;

        /* live values (no stale data) */
        float finalX = scoreManager.CurrMoodBoardX;
        float finalY = scoreManager.CurrMoodBoardY;
        float targetX = goal.x;
        float targetY = goal.y;

        float diffX = finalX - targetX;
        float diffY = finalY - targetY;
        float distance = Mathf.Sqrt((diffX * diffX) + (diffY * diffY));

        float graphSize = 2f;   /* since graph goes from [-1, 1] => 1 -(-)1 = 2 */
        float maxDistance = Mathf.Sqrt(2f * graphSize * graphSize);

        /* convert to accuracy (as whole number percentage) */
        float accuracy = 1f - (distance / maxDistance);
        float accuracyPercent = accuracy * 100f;

        return Mathf.Clamp(accuracyPercent, 0f, 100f);
    }

    private void AssessState(float accuracy)
    {
        if (currentMonsterManager == null)
        {
            MonsterStateManager.SetState("neutral");
            return;
        }

        float angerTolerance = currentMonsterManager.GetAngerTolerance();
        float satisfiedTolerance = currentMonsterManager.GetSatisfiedTolerance();
        float error = 100f - accuracy;

        if (error >= angerTolerance)
        {
            /* accuracy is too low for monster */
            FightButton.SetActive(true);
            MonsterStateManager.SetState("angry");
        }
        else if (error <= satisfiedTolerance)
        {
            /* accuracy is exceptional */
            MonsterStateManager.SetState("satisfied");
        }
        else
        {
            /* not bad or good */
            MonsterStateManager.SetState("neutral");
        }
    }
}


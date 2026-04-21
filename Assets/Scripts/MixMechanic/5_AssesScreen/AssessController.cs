/* This file:
1) Assesses the monster's mood upon serving (by communicating w/ ScoreManager)
2) Sets the monster's state (neutral/satisfied/angry) in MonsterStateManager
3) makes fight button visible if you rly messed up
4) shows coins on assess screen; Next button goes to next day (reload day screen)
*/

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class AssessController : MonoBehaviour
{
    [Header("refs")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private MonsterStateManager MonsterStateManager;
    [SerializeField] private CurrentMonster currentMonsterManager;


    [SerializeField] private TimerUI timerUI;

    [SerializeField] private MixManager mixManager;

    [SerializeField] private GameObject moodgraph;

    public LoseManager loseManager;

    [Header("UI")]
    [SerializeField] private TMP_Text coinsDisplay;
    [SerializeField] private TMP_Text coinGainPopup;
    [SerializeField] private float coinGainFadeDuration = 1.5f;
    [SerializeField] private float coinGainMoveUp = 30f;
    [SerializeField] private GameObject coinCanvas;
    [SerializeField] private string nextDaySceneName = "MixScene";
    public GameObject nextDayButtonObject;
    public DialogueController serveDialogueController;

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

        if (serveDialogueController == null)
        {
            var go = GameObject.Find("AssessDialogueController");
            if (go != null)
                serveDialogueController = go.GetComponent<DialogueController>();
        }

        if (nextDayButtonObject != null)
            nextDayButtonObject.SetActive(false);
        RefreshNextDayButton();

        MixAccuracy = AssessAccuracy();
        AssessState(MixAccuracy);
        if (AudioManager.Instance != null)
        {
            if (MonsterStateManager.MonsterState == "neutral")
                AudioManager.Instance.PlayAmbience();
            else
                AudioManager.Instance.PlayAssessAmbience(MonsterStateManager.MonsterState);
        }
        int amount = AwardCoinsForDrink();
        if (amount > 0)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayRegisterChaChing();
            ShowCoinGainPopup(amount);
        }

        if (coinCanvas != null)
            coinCanvas.SetActive(true);

        RefreshCoinsDisplay();
    }

    private void Update()
    {
        RefreshNextDayButton();
		RefreshFightButton();
    }

    private void RefreshNextDayButton()
    {
        if (currentMonsterManager == null)
            currentMonsterManager = CurrentMonster.Instance;

        bool hasNextMonster = currentMonsterManager != null && currentMonsterManager.HasNextMonsterInCurrentLevel();

        if (nextDayButtonObject != null)
        {
            bool dialogueFinished = serveDialogueController != null && serveDialogueController.IsDialogueFinished;
            bool angry = MonsterStateManager.MonsterState == "angry";
            nextDayButtonObject.SetActive(!angry && !hasNextMonster && dialogueFinished);
        }
    }

	private void RefreshFightButton()
	{
		if (FightButton == null)
			return;

		bool dialogueFinished = serveDialogueController != null && serveDialogueController.IsDialogueFinished;
		bool angry = MonsterStateManager.MonsterState == "angry";
		FightButton.SetActive(angry && dialogueFinished);
	}

    private int AwardCoinsForDrink()
    {
        if (GameManager.Instance == null) return 0;
        string state = MonsterStateManager.MonsterState;
        int amount = 0;
        if (state == "satisfied") { amount = coinsSatisfied; GameManager.Instance.AddCoins(amount); }
        else if (state == "neutral") { amount = coinsNeutral; GameManager.Instance.AddCoins(amount); }
        else if (state == "angry") { amount = coinsAngry; GameManager.Instance.AddCoins(amount); }
        return amount;
    }

    private void ShowCoinGainPopup(int amount)
    {
        if (coinGainPopup == null) return;
        coinGainPopup.text = "+" + amount;
        coinGainPopup.gameObject.SetActive(true);
        CanvasGroup cg = coinGainPopup.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = coinGainPopup.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        var rect = coinGainPopup.rectTransform;
        Vector2 startPos = rect.anchoredPosition;
        StartCoroutine(FadeOutCoinGainPopup(rect, startPos));
    }

    private IEnumerator FadeOutCoinGainPopup(RectTransform rect, Vector2 startPos)
    {
        float elapsed = 0f;
        CanvasGroup cg = coinGainPopup.GetComponent<CanvasGroup>();
        while (elapsed < coinGainFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / coinGainFadeDuration;
            if (cg != null)
                cg.alpha = 1f - t;
            rect.anchoredPosition = startPos + new Vector2(0f, coinGainMoveUp * t);
            yield return null;
        }
        if (cg != null)
            cg.alpha = 0f;
        coinGainPopup.gameObject.SetActive(false);
        rect.anchoredPosition = startPos;
    }

    /* call from Assess Next button OnClick: go to next day and show day screen again */
    public void OnNextCustomerPressed()
    {
        if (currentMonsterManager == null)
            currentMonsterManager = CurrentMonster.Instance;

        bool hasNextMonster = currentMonsterManager != null && currentMonsterManager.HasNextMonsterInCurrentLevel();

        if (!hasNextMonster)
        {
            if (loseManager != null && loseManager.CheckLoseAndLoad())
                return;
        }

        if (currentMonsterManager != null)
            currentMonsterManager.PlanNextVisit(hasNextMonster);

        if (AudioManager.Instance != null)
            AudioManager.Instance.StopAmbience();

        if (!string.IsNullOrEmpty(nextDaySceneName))
            SceneManager.LoadScene(nextDaySceneName);
    }

    private void RefreshCoinsDisplay()
    {
        if (coinsDisplay == null) return;
        int coins = GameManager.Instance != null ? GameManager.Instance.Coins : 0;
        coinsDisplay.text = coins.ToString();
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
       
       if (timerUI.TimeUp == true)
        {
            MonsterStateManager.SetState("angry");
            moodgraph.SetActive(false);
            
            return;
        }
        float satisfiedTolerance = currentMonsterManager.GetSatisfiedTolerance();
        float error = 100f - accuracy;
        bool sameQuadrantAsGoal = IsFinalScoreInGoalQuadrant();

        if (error <= satisfiedTolerance)
        {
            /* inside satisfied circle */
            MonsterStateManager.SetState("satisfied");
        }
        else if (sameQuadrantAsGoal)
        {
            /* in goal quadrant but outside satisfied circle */
            MonsterStateManager.SetState("neutral");
        }
        else
        {
            /* outside goal quadrant */
            MonsterStateManager.SetState("angry");
        }

        /* now assess TOPPINGS */
        AssessToppings();
    }

    private void AssessToppings()
    {
        string toppingPreference = currentMonsterManager.Data.toppingsPreference;
        bool CorrectTopping = mixManager.SelectedToppings.Contains(toppingPreference);

        /* promote one mood up */
        if(CorrectTopping){
            print("CORRECT TOPPING!");
            if(MonsterStateManager.MonsterState == "angry") {
                    MonsterStateManager.SetState("neutral");
                    return;
                }
            else if (MonsterStateManager.MonsterState == "neutral") {
                // Keep state vocabulary consistent with the rest of the game.
                MonsterStateManager.SetState("satisfied");
                return;
            }

        }
        /* demote one mood down */
        else {
            print("INCORRECT TOPPING!");
            if(MonsterStateManager.MonsterState == "satisfied") {
                    MonsterStateManager.SetState("neutral");
                    return;
                }
            else if (MonsterStateManager.MonsterState == "neutral") {
                MonsterStateManager.SetState("angry");
                return;
            }
        }
    }

    private bool IsFinalScoreInGoalQuadrant()
    {
        if (scoreManager == null || currentMonsterManager == null)
            return false;

        ScorePair goal = currentMonsterManager.GetGoalScore();
        if (goal == null)
            return false;

        int goalQuadrant = GetQuadrant(goal.x, goal.y);
        int finalQuadrant = GetQuadrant(scoreManager.CurrMoodBoardX, scoreManager.CurrMoodBoardY);
        return goalQuadrant != 0 && goalQuadrant == finalQuadrant;
    }

    private int GetQuadrant(float x, float y)
    {
        if (x > 0f && y > 0f) return 1;
        if (x < 0f && y > 0f) return 2;
        if (x < 0f && y < 0f) return 3;
        if (x > 0f && y < 0f) return 4;
        return 0; // on axis/origin
    }
}


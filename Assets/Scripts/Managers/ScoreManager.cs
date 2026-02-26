/*
This file determines the score of your mix based off current customer, it interacts with mixmanager to fire events
off when quantities update (glass choice, base drips, ingredients).

Glass: correct = 30% closer to target
Base Drips: Using interpolation, each drip nudges to base's X Y 50% 
Ingredients choices: Also using interpolation, each ingredient nudges 35% the way there
*/

using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public GameObject MixManagerObject;
    [SerializeField] private CurrentMonster currentMonsterManager;

    private MixManager mixManager;
    private IngredientsFile ingredientsFile;

    [Header("current score given as X & Y coords")]
    public float CurrMoodBoardX;
    public float CurrMoodBoardY;

    [SerializeField] private TMP_Text ScoreDisplay; /* text until mood graph */

    [Header("tuning knobs")]
    [SerializeField] private float glassScorePercent = 0.3f;
    [SerializeField] private float glassFinalInfluencePercent = 0.3f;
    [SerializeField] private float baseMaxStepPercent = 0.7f;   
    [SerializeField] private float toppingStepPercent = 0.2f; 
    [SerializeField] private float wrongBottlePenaltyPercent = 0.15f;
    [SerializeField] private float wrongBottleFinalPenaltyPercent = 0.2f;
    [SerializeField] private float minToppingInfluenceWhenBaseMisaligned = 0.02f;
    [SerializeField] private float wrongBottleToppingMultiplier = 0.7f;
    [SerializeField] private bool clampMoodBoard = true;
    [SerializeField] private float clampMin = -1f;
    [SerializeField] private float clampMax = 1f;
    private bool initializedFromMonster;

    private void LoadIngredients()
    {
        TextAsset json = Resources.Load<TextAsset>("Data/Ingredients");
        ingredientsFile = JsonUtility.FromJson<IngredientsFile>(json.text);
    }

    /* run before any Start() so score display and mood graph see correct starting mood on first frame and every new day (scene load) */
    private void Awake()
    {
        LoadIngredients();
        if (MixManagerObject != null)
            mixManager = MixManagerObject.GetComponent<MixManager>();
        TryInitializeFromCurrentMonster();
    }

    private void Start()
    {
        if (mixManager != null)
            mixManager.OnStateChanged += OnMixManagerChanged;
    }

    private void Update()
    {
        if (!initializedFromMonster)
            TryInitializeFromCurrentMonster();
    }

    private bool ResolveCurrentMonster()
    {
        if (currentMonsterManager == null)
            currentMonsterManager = CurrentMonster.Instance;
        return currentMonsterManager != null;
    }

    private void TryInitializeFromCurrentMonster()
    {
        if (!ResolveCurrentMonster()) return;

        ScorePair starting = GetCurrentStartingScore();
        if (starting == null) return;

        ResetToMonsterStart();
        if (mixManager != null)
            RecalculateFullMood();
        UpdateScoreText();
        initializedFromMonster = true;
    }

    private void ResetToMonsterStart()
    {
        ScorePair starting = GetCurrentStartingScore();
        if (starting == null) return;
        CurrMoodBoardX = starting.x;
        CurrMoodBoardY = starting.y;
    }

    public MonsterData GetCurrentMonster()
    {
        ResolveCurrentMonster();
        return currentMonsterManager != null ? currentMonsterManager.Data : null;
    }

    public ScorePair GetCurrentStartingScore()
    {
        ResolveCurrentMonster();
        return currentMonsterManager != null ? currentMonsterManager.GetStartingScore() : null;
    }

    public ScorePair GetCurrentGoalScore()
    {
        ResolveCurrentMonster();
        return currentMonsterManager != null ? currentMonsterManager.GetGoalScore() : null;
    }

    private void OnMixManagerChanged()
    {
        RecalculateFullMood();
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (ScoreDisplay != null)
            ScoreDisplay.text = $"Mood: ({CurrMoodBoardX:F2}, {CurrMoodBoardY:F2})";
    }

    /* call when showing score display (e.g. after new day); shows monster starting mood only */
    public void RefreshScoreDisplay()
    {
        ScorePair starting = GetCurrentStartingScore();
        if (starting == null) return;
        if (mixManager == null && MixManagerObject != null)
            mixManager = MixManagerObject.GetComponent<MixManager>();
        if (mixManager != null)
            mixManager.ResetForNewDay();
        ResetToMonsterStart();
        UpdateScoreText();
    }

    private void RecalculateFullMood()
    {
        MonsterData currentMonster = GetCurrentMonster();
        ScorePair starting = GetCurrentStartingScore();
        ScorePair goal = GetCurrentGoalScore();
        if (currentMonster == null || starting == null || goal == null) return;

        /* start from monster's starting */
        float x = starting.x;
        float y = starting.y;

        bool correctBottle = EvalGlassChoice(currentMonster, goal, ref x, ref y);

        Vector2 baseTarget;
        float baseStep;
        bool hasBase = TryGetBaseBlendTarget(out baseTarget, out baseStep);
        if (hasBase)
        {
            x += baseStep * (baseTarget.x - x);
            y += baseStep * (baseTarget.y - y);
        }

        Vector2 toppingsTarget;
        float toppingsStep;
        int toppingCount;
        bool hasToppings = TryGetToppingsBlendTarget(out toppingsTarget, out toppingsStep, out toppingCount);
        if (hasToppings)
        {
            float baseAlignment = ComputeAlignmentToGoal(
                new Vector2(starting.x, starting.y),
                new Vector2(goal.x, goal.y),
                hasBase ? baseTarget : new Vector2(starting.x, starting.y));

            float baseGate = Mathf.Lerp(minToppingInfluenceWhenBaseMisaligned, 1f, baseAlignment);
            float bottleGate = correctBottle ? 1f : wrongBottleToppingMultiplier;
            float finalToppingStep = toppingsStep * baseGate * bottleGate;

            x += finalToppingStep * (toppingsTarget.x - x);
            y += finalToppingStep * (toppingsTarget.y - y);
        }

        ApplyFinalGlassInfluence(goal, correctBottle, ref x, ref y);

        CurrMoodBoardX = x;
        CurrMoodBoardY = y;

        if (clampMoodBoard)
        {
            CurrMoodBoardX = Mathf.Clamp(CurrMoodBoardX, clampMin, clampMax);
            CurrMoodBoardY = Mathf.Clamp(CurrMoodBoardY, clampMin, clampMax);
        }
    }

    private bool EvalGlassChoice(MonsterData currentMonster, ScorePair goal, ref float x, ref float y)
    {
        if (mixManager == null || currentMonster == null || goal == null || string.IsNullOrEmpty(mixManager.SelectedBottle))
            return false;

        string selectedBottle = mixManager.SelectedBottle.Trim().ToLowerInvariant();
        string preferredBottle = currentMonster.glassPreference != null
            ? currentMonster.glassPreference.Trim().ToLowerInvariant()
            : string.Empty;

        if (selectedBottle == preferredBottle)
        {
            x += glassScorePercent * (goal.x - x);
            y += glassScorePercent * (goal.y - y);
            return true;
        }

        x -= wrongBottlePenaltyPercent * (goal.x - x);
        y -= wrongBottlePenaltyPercent * (goal.y - y);
        return false;
    }

    private void ApplyFinalGlassInfluence(ScorePair goal, bool correctBottle, ref float x, ref float y)
    {
        if (goal == null) return;

        if (correctBottle)
        {
            x += glassFinalInfluencePercent * (goal.x - x);
            y += glassFinalInfluencePercent * (goal.y - y);
        }
        else
        {
            x -= wrongBottleFinalPenaltyPercent * (goal.x - x);
            y -= wrongBottleFinalPenaltyPercent * (goal.y - y);
        }
    }

    /* map key used in mix mannager to ingredient id in JSON */
    private string NormalizeBaseKey(string baseKey)
    {
        if (string.IsNullOrEmpty(baseKey)) return baseKey;
        string id = baseKey.ToLower();
        if (id == "holywater") id = "holy_water"; /* weird mapping */
        return id;
    }

    private IngredientEffect GetEffectForBase(string baseKey)
    {
        string id = NormalizeBaseKey(baseKey);

        foreach (var ing in ingredientsFile.ingredients)
        {
            if (ing.id == id) return ing.effect;
        }
        return null;
    }

    private IngredientEffect GetEffectForIngredient(string ingredientKey)
    {
        if (string.IsNullOrEmpty(ingredientKey)) return null;

        string id = ingredientKey.ToLower();

        foreach (var ing in ingredientsFile.ingredients)
        {
            if (ing.id == id) return ing.effect;
        }
        return null;
    }

    private bool TryGetBaseBlendTarget(out Vector2 target, out float stepPercent)
    {
        target = Vector2.zero;
        stepPercent = 0f;
        if (mixManager == null || mixManager.FillLevel <= 0f) return false;

        float totalFill = mixManager.FillLevel;
        float fillStrength = Mathf.Clamp01(mixManager.FillLevel);
        stepPercent = baseMaxStepPercent * fillStrength;
        float weightedTargetX = 0f;
        float weightedTargetY = 0f;
        float totalValidWeight = 0f;

        foreach (var kvp in mixManager.BaseAmounts)
        {
            string baseKey = kvp.Key;
            float amount = kvp.Value;
            if (amount <= 0f) continue;

            var eff = GetEffectForBase(baseKey);
            if (eff == null) continue;

            float weight = amount / totalFill;
            weightedTargetX += eff.x * weight;
            weightedTargetY += eff.y * weight;
            totalValidWeight += weight;
        }

        if (totalValidWeight <= 0f) return false;

        weightedTargetX /= totalValidWeight;
        weightedTargetY /= totalValidWeight;
        target = new Vector2(weightedTargetX, weightedTargetY);
        return true;
    }


    private bool TryGetToppingsBlendTarget(out Vector2 target, out float combinedStep, out int validCount)
    {
        target = Vector2.zero;
        combinedStep = 0f;
        validCount = 0;
        if (mixManager == null) return false;

        if (mixManager.SelectedIngredients == null || mixManager.SelectedIngredients.Count == 0)
        {
            return false;
        }

        float avgX = 0f;
        float avgY = 0f;

        foreach (var ingredientKey in mixManager.SelectedIngredients)
        {
            var eff = GetEffectForIngredient(ingredientKey);
            if (eff == null) continue;

            avgX += eff.x;
            avgY += eff.y;
            validCount++;
        }

        if (validCount <= 0) return false;

        avgX /= validCount;
        avgY /= validCount;
        target = new Vector2(avgX, avgY);
        combinedStep = 1f - Mathf.Pow(1f - toppingStepPercent, validCount);
        return true;
    }

    private float ComputeAlignmentToGoal(Vector2 starting, Vector2 goal, Vector2 baseTarget)
    {
        Vector2 toGoal = goal - starting;
        Vector2 toBase = baseTarget - starting;
        if (toGoal.sqrMagnitude <= 0.0001f || toBase.sqrMagnitude <= 0.0001f)
            return 0f;

        float dot = Vector2.Dot(toGoal.normalized, toBase.normalized);
        return Mathf.Clamp01((dot + 1f) * 0.5f);
    }

    private void OnDestroy()
    {
        if (mixManager != null)
            mixManager.OnStateChanged -= OnMixManagerChanged;
    }
}

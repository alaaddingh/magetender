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
    [SerializeField] private string monstersJsonResourcePath = "Data/Monsters";
    public GameObject MixManagerObject;

    private MixManager mixManager;
    private MonsterData CurrentMonster;
    private IngredientsFile ingredientsFile;

    [Header("current score given as X & Y coords")]
    public float CurrMoodBoardX;
    public float CurrMoodBoardY;

    [SerializeField] private TMP_Text ScoreDisplay; /* text until mood graph */

    [Header("tuning knobs")]
    [SerializeField] private float glassScorePercent = 0.3f;
    [SerializeField] private float baseMaxStepPercent = 0.5f;   
    [SerializeField] private float toppingStepPercent = 0.35f; 
    [SerializeField] private bool clampMoodBoard = true;
    [SerializeField] private float clampMin = -1f;
    [SerializeField] private float clampMax = 1f;

    private void LoadMonster()
    {
        TextAsset json = Resources.Load<TextAsset>(monstersJsonResourcePath);
        if (json == null) return;

        MonstersFile file = JsonUtility.FromJson<MonstersFile>(json.text);
        CurrentMonster = file.monsters[0];  /* temporary. THIS WILL NEED TO BE RESET FOR EVERY MONSTER */
    }

    private void LoadIngredients()
    {
        TextAsset json = Resources.Load<TextAsset>("Data/Ingredients");
        ingredientsFile = JsonUtility.FromJson<IngredientsFile>(json.text);
    }

    /* run before any Start() so score display and mood graph see correct starting mood on first frame and every new day (scene load) */
    private void Awake()
    {
        LoadMonster();
        LoadIngredients();
        if (MixManagerObject != null)
            mixManager = MixManagerObject.GetComponent<MixManager>();
        if (CurrentMonster == null || CurrentMonster.starting_score == null) return;
        ResetToMonsterStart();
        if (mixManager != null)
            RecalculateFullMood();
        UpdateScoreText();
    }

    private void Start()
    {
        if (mixManager != null)
            mixManager.OnStateChanged += OnMixManagerChanged;
    }

    private void ResetToMonsterStart()
    {
        CurrMoodBoardX = CurrentMonster.starting_score.x;
        CurrMoodBoardY = CurrentMonster.starting_score.y;
    }

    public MonsterData GetCurrentMonster()
    {
        return CurrentMonster;
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
        LoadMonster();
        if (CurrentMonster == null || CurrentMonster.starting_score == null) return;
        if (mixManager == null && MixManagerObject != null)
            mixManager = MixManagerObject.GetComponent<MixManager>();
        if (mixManager != null)
            mixManager.ResetForNewDay();
        ResetToMonsterStart();
        UpdateScoreText();
    }

    private void RecalculateFullMood()
    {
        /* start from monster's starting */
        float x = CurrentMonster.starting_score.x;
        float y = CurrentMonster.starting_score.y;

        EvalGlassChoice(ref x, ref y);

        EvalBaseChoice(ref x, ref y);

        EvalToppingsChoice(ref x, ref y);

        CurrMoodBoardX = x;
        CurrMoodBoardY = y;

        if (clampMoodBoard)
        {
            CurrMoodBoardX = Mathf.Clamp(CurrMoodBoardX, clampMin, clampMax);
            CurrMoodBoardY = Mathf.Clamp(CurrMoodBoardY, clampMin, clampMax);
        }
    }

    private void EvalGlassChoice(ref float x, ref float y)
    {
        if (string.IsNullOrEmpty(mixManager.SelectedBottle)) return;

        if (mixManager.SelectedBottle == CurrentMonster.glassPreference)
        {
            x += glassScorePercent * (CurrentMonster.goal_score.x - x);
            y += glassScorePercent * (CurrentMonster.goal_score.y - y);
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

    /* for each base Eval its drips
       and tug CurrMoodBoardX/Y by `stepPercent` toward 
       the base's destination from ingredients.json.
       
    */
    private void EvalBaseChoice(ref float x, ref float y)
    {
        if (mixManager.FillLevel <= 0f) return;

        float totalFill = mixManager.FillLevel;
        float fillStrength = Mathf.Clamp01(mixManager.FillLevel);
        float stepPercent = baseMaxStepPercent * fillStrength;

        foreach (var kvp in mixManager.BaseAmounts)
        {
            string baseKey = kvp.Key;
            float amount = kvp.Value;
            if (amount <= 0f) continue;

            var eff = GetEffectForBase(baseKey);
            if (eff == null) continue;

            float weight = amount / totalFill;
            x += stepPercent * weight * (eff.x - x);
            y += stepPercent * weight * (eff.y - y);
        }
    }


    /* similar to base choice, nudges towards ingredients.json but by 35% */
    private void EvalToppingsChoice(ref float x, ref float y)
    {
        if (mixManager.SelectedIngredients == null || mixManager.SelectedIngredients.Count == 0)
        {
            return;
        }

        foreach (var ingredientKey in mixManager.SelectedIngredients)
        {
            var eff = GetEffectForIngredient(ingredientKey);
            if (eff == null) continue;

            x += toppingStepPercent * (eff.x - x);
            y += toppingStepPercent * (eff.y - y);
        }
    }

    private void OnDestroy()
    {
        if (mixManager != null)
            mixManager.OnStateChanged -= OnMixManagerChanged;
    }
}

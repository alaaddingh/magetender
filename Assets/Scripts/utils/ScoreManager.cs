/*
This file determines the score of your mix based off current customer.
It now listens for drips added in MixManager and nudges the current mood
board 5% toward the weighted base-effect each time a drip is added.
*/

using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Use existing data types in Assets/Scripts/Data/IngredientData.cs:
// IngredientsFile, IngredientData, IngredientEffect

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

    [SerializeField] private TMP_Text ScoreDisplay; /* temporary until mood graph */

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
        if (file != null && file.monsters != null && file.monsters.Count > 0)
            CurrentMonster = file.monsters[0];  /* temporary */
    }

    private void LoadIngredients()
    {
        TextAsset json = Resources.Load<TextAsset>("Data/Ingredients");
        if (json == null) return;

        ingredientsFile = JsonUtility.FromJson<IngredientsFile>(json.text);
    }

    void Start()
    {
        LoadMonster();
        LoadIngredients();

        mixManager = MixManagerObject.GetComponent<MixManager>();
        if (mixManager != null)
        {
            mixManager.OnStateChanged += OnMixManagerChanged;
        }

        /* init to monster start */
        ResetToMonsterStart();
        RecalculateFullMood();
        UpdateScoreText();
    }

    private void ResetToMonsterStart()
    {
        CurrMoodBoardX = CurrentMonster.starting_score.x;
        CurrMoodBoardY = CurrentMonster.starting_score.y;
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
        if (mixManager.SelectedIngredients == null || mixManager.SelectedIngredients.Count == 0) {
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

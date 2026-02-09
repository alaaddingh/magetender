/* purpose of file: store data regarding potion decisions by user */
using UnityEngine;
using System;
using System.Collections.Generic;

public class MixManager : MonoBehaviour
{
    [Header("Bottle selection")]
    public string SelectedBottle = "";
    public string SelectedBase = "";

    [Header("Ingredients")]
    public List<string> SelectedIngredients = new List<string>();

    [Header("Fill Data")]
    public float FillLevel = 0f;
    public Dictionary<string, float> BaseAmounts = new Dictionary<string, float>();

    [Header("Base Quantities")]
    public float BloodAmount = 0f;
    public float HolyWaterAmount = 0f;
    public float SpiritsAmount = 0f;
    public float MoonShineAmount = 0f;

    [Header("Base colors (for reference)")]

    /* store each individual base quantity for score-keeping */
    public Color BloodColor = new Color(0.7f, 0.1f, 0.1f, 1f);
    public Color HolyWaterColor = new Color(0.7f, 0.9f, 1f, 1f);
    public Color SpiritsColor = new Color(0.85f, 0.95f, 0.8f, 1f);
    public Color MoonShineColor = new Color(0.9f, 0.7f, 0.9f, 1f);

    public void SetBottle(string bottleKey)
    {
        SelectedBottle = bottleKey;
        OnStateChanged?.Invoke();
        OnBottleChanged?.Invoke(bottleKey);
    }

    public void SetBase(string baseKey)
    {
        SelectedBase = baseKey;
        OnStateChanged?.Invoke();
    }

    public void AddIngredient(string ingredientKey)
    {
        SelectedIngredients.Add(ingredientKey);
        OnIngredientAdded?.Invoke(ingredientKey);
        OnStateChanged?.Invoke();
    }

    public void AddDrip(string baseKey, float amount)
    {
        if (BaseAmounts.ContainsKey(baseKey))
        {
            BaseAmounts[baseKey] += amount;
        }
        else
        {
            BaseAmounts[baseKey] = amount;
        }
        FillLevel += amount;
        UpdateBaseQuantityDisplay(baseKey);
        OnDripAdded?.Invoke(baseKey, amount);
        OnStateChanged?.Invoke();
    }

    // event fired when a drip is added: (baseKey, amount) for scoremanager.cs */
    public Action<string, float> OnDripAdded;
    // general event fired whenever MixManager state changes
    public Action OnStateChanged;
    // event fired when the selected bottle changes
    public Action<string> OnBottleChanged;

    /* event fired when an ingredient is added */
    public Action<string> OnIngredientAdded;

    private void UpdateBaseQuantityDisplay(string baseKey)
    {
        switch (baseKey.ToLower())
        {
            case "blood":
                BloodAmount = BaseAmounts["blood"];
                break;
            case "holywater":
                HolyWaterAmount = BaseAmounts["holywater"];
                break;
            case "spirits":
                SpiritsAmount = BaseAmounts["spirits"];
                break;
            case "moonshine":
                MoonShineAmount = BaseAmounts["moonshine"];
                break;
        }
    }

    public void ResetFillData()
    {
        FillLevel = 0f;
        BaseAmounts.Clear();
        SelectedBase = "";
        BloodAmount = 0f;
        HolyWaterAmount = 0f;
        SpiritsAmount = 0f;
        MoonShineAmount = 0f;
        OnStateChanged?.Invoke();
    }

    public Color GetBaseColor(string baseKey)
    {
        switch (baseKey.ToLower())
        {
            case "blood":
                return BloodColor;
            case "holywater":
                return HolyWaterColor;
            case "spirits":
                return SpiritsColor;
            case "moonshine":
                return MoonShineColor;
            default:
                return Color.white;
        }
    }
}

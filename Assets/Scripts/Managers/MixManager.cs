using UnityEngine;
using System;
using System.Collections.Generic;

public class MixManager : MonoBehaviour
{
    public string SelectedBase = "";

    [Header("Ingredients")]
    public List<string> SelectedIngredients = new List<string>();
    [SerializeField, Min(1)] private int maxSelectedIngredients = 3;

    [Header("Toppings")]
    public List<string> SelectedToppings = new List<string>();
    [SerializeField, Min(0)] private int maxSelectedToppings = 3;

    [Header("Fill Data")]
    public float FillLevel = 0f;
    public Dictionary<string, float> BaseAmounts = new Dictionary<string, float>();

    [Header("Base Quantities")]
    public float BloodAmount = 0f;
    public float HolyWaterAmount = 0f;
    public float SpiritsAmount = 0f;
    public float MoonShineAmount = 0f;

    [Header("Base colors (for reference)")]
    public Color BloodColor = new Color(0.7f, 0.1f, 0.1f, 1f);
    public Color HolyWaterColor = new Color(0.7f, 0.9f, 1f, 1f);
    public Color SpiritsColor = new Color(0.85f, 0.95f, 0.8f, 1f);
    public Color MoonShineColor = new Color(0.9f, 0.7f, 0.9f, 1f);

    public void SetBase(string baseKey)
    {
        SelectedBase = baseKey;
        OnStateChanged?.Invoke();
    }

    public bool AddIngredient(string ingredientKey)
    {
        if (string.IsNullOrEmpty(ingredientKey)) return false;
        if (SelectedIngredients.Count >= maxSelectedIngredients) return false;
        if (SelectedIngredients.Contains(ingredientKey)) return false;

        SelectedIngredients.Add(ingredientKey);
        OnIngredientAdded?.Invoke(ingredientKey);
        OnStateChanged?.Invoke();
        return true;
    }

    public bool RemoveIngredient(string ingredientKey)
    {
        if (string.IsNullOrEmpty(ingredientKey)) return false;
        if (SelectedIngredients.Count == 0) return false;

        bool removed = SelectedIngredients.Remove(ingredientKey);
        if (!removed) return false;

        OnIngredientRemoved?.Invoke(ingredientKey);
        OnStateChanged?.Invoke();
        return true;
    }

    public bool AddTopping(string toppingKey)
    {
        if (string.IsNullOrEmpty(toppingKey)) return false;
        if (maxSelectedToppings > 0 && SelectedToppings.Count >= maxSelectedToppings) return false;
        if (SelectedToppings.Contains(toppingKey)) return false;

        SelectedToppings.Add(toppingKey);
        OnToppingAdded?.Invoke(toppingKey);
        OnStateChanged?.Invoke();
        return true;
    }

    public bool RemoveTopping(string toppingKey)
    {
        if (SelectedToppings.Count == 0) return false;

        bool removed = SelectedToppings.Remove(toppingKey);
        if (!removed) return false;

        OnToppingRemoved?.Invoke(toppingKey);
        OnStateChanged?.Invoke();
        return true;
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
        UpdateBaseDisplay(baseKey);
        OnDripAdded?.Invoke(baseKey, amount);
        OnStateChanged?.Invoke();
    }

    public void DrainFill(float amount)
    {
        if (FillLevel <= 0f) return;
        amount = Mathf.Min(amount, FillLevel);
        float factor = (FillLevel - amount) / FillLevel;
        FillLevel -= amount;
        foreach (var key in new List<string>(BaseAmounts.Keys))
        {
            BaseAmounts[key] *= factor;
        }
        SyncAllBaseQuantitiesFromDictionary();
        OnStateChanged?.Invoke();
    }

    public Action<string, float> OnDripAdded;
    public Action OnStateChanged;
    public Action<string> OnIngredientAdded;
    public Action<string> OnIngredientRemoved;
    public Action<string> OnToppingAdded;
    public Action<string> OnToppingRemoved;

    private void UpdateBaseDisplay(string baseKey)
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

    private void SyncAllBaseQuantitiesFromDictionary()
    {
        BloodAmount = BaseAmounts.TryGetValue("blood", out float b) ? b : 0f;
        HolyWaterAmount = BaseAmounts.TryGetValue("holywater", out float h) ? h : 0f;
        SpiritsAmount = BaseAmounts.TryGetValue("spirits", out float s) ? s : 0f;
        MoonShineAmount = BaseAmounts.TryGetValue("moonshine", out float m) ? m : 0f;
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

    public void ResetForNewDay()
    {
        SelectedBase = "";
        SelectedIngredients.Clear();
        SelectedToppings.Clear();
        ResetFillData();
    }

    private void Awake()
    {
        ResetForNewDay();
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

    public void SetMaxIngredients(int max)
    {
        maxSelectedIngredients = Mathf.Max(1, max);
    }

    public void SetMaxToppings(int max)
    {
        maxSelectedToppings = Mathf.Max(0, max);
    }
}

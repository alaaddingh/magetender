using UnityEngine;
using Magetender.Data;
using System;
using System.Collections.Generic;

/* Holds coins and day. Put one GameManager in the scene; it survives scene load via DontDestroyOnLoad. */
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Starting values")]
    [SerializeField] private int startingCoins = 0;
    [SerializeField] private int startingDay = 1;

    [SerializeField] private GameObject LoseManager;



    [Header("Economy")]
    [SerializeField] private int maintenanceCost = 80;
    public int MaintenanceCost => maintenanceCost;

	[Header("Shop (run-only unlocks)")]
	[SerializeField] private bool unlockAllIngredientsForDebug = false;
	[SerializeField] private List<string> startingUnlockedIngredients = new List<string>
	{
		// Small starter set so day 1 remains playable.
		"pixie_dust",
		"cupid_arrow",
		"lotus_flower",
		"rose_thorn",
	};

	private readonly HashSet<string> unlockedIngredients = new HashSet<string>();
	public event Action OnIngredientUnlocksChanged;

    public int Coins { get; private set; }
    public int Day { get; private set; }
    public bool TutorialCompleted { get; private set; }

    private int EncounterIndex;
    private bool SavedEncounterIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Coins = startingCoins;
        Day = startingDay;

        var data = SaveSystem.LoadGame();
        if (data != null)
        {
            Coins = data.coins;
            Day = Mathf.Max(1, data.day);
            TutorialCompleted = data.tutorialCompleted;
            EncounterIndex = data.currentEncounterIndex;
            SavedEncounterIndex = true;

			ResetIngredientUnlocksToDefaults();
			if (data.unlockedIngredientIds != null)
				ApplyUnlockedIngredientIds(data.unlockedIngredientIds);
        }
		else
		{
            TutorialCompleted = false;
			ResetIngredientUnlocksToDefaults();
		}
    }

    private void Start()
    {
        if (SavedEncounterIndex && CurrentMonster.Instance != null)
        {
            CurrentMonster.Instance.ApplySaveProgress(EncounterIndex);
            SavedEncounterIndex = false;
        }
    }

    public void ResetForNewGame(bool preserveTutorialCompleted = true)
    {
        TutorialCompleted = preserveTutorialCompleted && TutorialCompleted;
        Coins = startingCoins;
        Day = startingDay;
        EncounterIndex = 0;
        SavedEncounterIndex = false;
		ResetIngredientUnlocksToDefaults();
    }

    public void MarkTutorialCompleted()
    {
        if (TutorialCompleted)
            return;

        TutorialCompleted = true;
        SaveSystem.WriteData();
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        SaveSystem.WriteData();
    }

    public void IncrementDay()
    {
        Day++;

        if (Day > 1)
        {
            Coins -= maintenanceCost;
        }

        SaveSystem.WriteData();
    }

	public bool IsIngredientUnlocked(string ingredientId)
	{
		if (unlockAllIngredientsForDebug)
			return true;

		if (string.IsNullOrEmpty(ingredientId))
			return false;

		return unlockedIngredients.Contains(ingredientId);
	}

	public bool TryUnlockIngredient(string ingredientId)
	{
		if (unlockAllIngredientsForDebug)
			return false;

		if (string.IsNullOrEmpty(ingredientId))
			return false;

		bool added = unlockedIngredients.Add(ingredientId);
		if (added)
		{
			OnIngredientUnlocksChanged?.Invoke();
			SaveSystem.WriteData();
		}
		return added;
	}

	public string[] GetUnlockedIngredientIds()
	{
		if (unlockedIngredients.Count == 0)
			return Array.Empty<string>();

		string[] ids = new string[unlockedIngredients.Count];
		unlockedIngredients.CopyTo(ids);
		return ids;
	}

	private void ApplyUnlockedIngredientIds(IEnumerable<string> ids)
	{
		bool anyAdded = false;
		foreach (string id in ids)
		{
			if (string.IsNullOrEmpty(id))
				continue;
			if (unlockedIngredients.Add(id))
				anyAdded = true;
		}

		if (anyAdded)
			OnIngredientUnlocksChanged?.Invoke();
	}

	public void ResetIngredientUnlocksToDefaults()
	{
		unlockedIngredients.Clear();

		if (unlockAllIngredientsForDebug)
		{
			OnIngredientUnlocksChanged?.Invoke();
			return;
		}

		if (startingUnlockedIngredients != null)
		{
			foreach (string id in startingUnlockedIngredients)
			{
				if (!string.IsNullOrEmpty(id))
					unlockedIngredients.Add(id);
			}
		}

		OnIngredientUnlocksChanged?.Invoke();
	}
}

using UnityEngine;
using UnityEngine.SceneManagement;
using Magetender.Data;
using System;
using System.Collections.Generic;
using System.Globalization;

/* Holds coins and day. Put one GameManager in the scene; it survives scene load via DontDestroyOnLoad. */
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

	private const string TutorialExitMaintenancePendingKey = "TutorialExitMaintenancePending";
	private const string TutorialExitMaintenanceAmountKey = "TutorialExitMaintenanceAmount";

    [Header("Starting values")]
    [SerializeField] private int startingCoins = 0;
    [SerializeField] private int startingDay = 1;

    [SerializeField] private GameObject LoseManager;
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

	private bool pendingBarFight;
	private bool skipDayPanelNextMixLoad;
	private int pendingBarFightEncounterIndex;
	private bool openPauseMenuOnNextFightSceneLoad;

	public bool PendingBarFight => pendingBarFight;
	public bool SkipDayPanelNextMixLoad => skipDayPanelNextMixLoad;
	public int PendingBarFightEncounterIndex => pendingBarFightEncounterIndex;

	public string PlaythroughId { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
		SceneManager.sceneLoaded += OnSceneLoadedClearStaleFightPauseRequest;

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

			PlaythroughId = string.IsNullOrEmpty(data.playthroughId)
				? Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture)
				: data.playthroughId;

			ResetIngredientUnlocksToDefaults();
			if (data.unlockedIngredientIds != null)
				ApplyUnlockedIngredientIds(data.unlockedIngredientIds);

			ApplySaveFlags(data);
        }
		else
		{
            TutorialCompleted = false;
			PlaythroughId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
			ResetIngredientUnlocksToDefaults();
		}
    }

	private void OnDestroy()
	{
		if (Instance == this)
			SceneManager.sceneLoaded -= OnSceneLoadedClearStaleFightPauseRequest;
	}

	private void OnSceneLoadedClearStaleFightPauseRequest(Scene scene, LoadSceneMode mode)
	{
		const string qteScene = "QTECombatScene";
		if (openPauseMenuOnNextFightSceneLoad && scene.name != qteScene)
			openPauseMenuOnNextFightSceneLoad = false;
	}

    private void Start()
    {
		GameAnalytics.InitializeIfNeeded();
        if (SavedEncounterIndex && CurrentMonster.Instance != null)
        {
            CurrentMonster.Instance.ApplySaveProgress(EncounterIndex);
            SavedEncounterIndex = false;
        }
    }

	public void RequestOpenPauseMenuOnNextFightSceneLoad()
	{
		openPauseMenuOnNextFightSceneLoad = true;
	}

	public bool ConsumeOpenPauseMenuOnNextFightSceneLoad()
	{
		if (!openPauseMenuOnNextFightSceneLoad)
			return false;
		openPauseMenuOnNextFightSceneLoad = false;
		return true;
	}

    public void ResetForNewGame(bool preserveTutorialCompleted = true)
    {
        TutorialCompleted = preserveTutorialCompleted && TutorialCompleted;
        Coins = startingCoins;
        Day = startingDay;
        EncounterIndex = 0;
        SavedEncounterIndex = false;
		PlaythroughId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
		ClearProgressFlowFlags();
		ResetIngredientUnlocksToDefaults();
    }

	public void LoadProgressFromSave(SaveData data)
	{
		if (data == null)
			return;

		Coins = data.coins;
		Day = Mathf.Max(1, data.day);
		TutorialCompleted = data.tutorialCompleted;
		SavedEncounterIndex = false;

		PlaythroughId = string.IsNullOrEmpty(data.playthroughId)
			? Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture)
			: data.playthroughId;

		ResetIngredientUnlocksToDefaults();
		if (data.unlockedIngredientIds != null)
			ApplyUnlockedIngredientIds(data.unlockedIngredientIds);

		ApplySaveFlags(data);
	}

	public void SetPendingBarFight(bool value, int encounterIndexWhenPending)
	{
		pendingBarFight = value;
		pendingBarFightEncounterIndex = value ? encounterIndexWhenPending : 0;
	}

	public void SetSkipDayPanelNextMixLoad(bool value)
	{
		skipDayPanelNextMixLoad = value;
	}

	public bool ConsumeSkipDayPanelNextMixLoad()
	{
		if (!skipDayPanelNextMixLoad)
			return false;

		skipDayPanelNextMixLoad = false;
		SaveSystem.WriteData();
		return true;
	}

	private void ApplySaveFlags(SaveData data)
	{
		if (data == null)
			return;

		pendingBarFight = data.pendingBarFight;
		skipDayPanelNextMixLoad = data.skipDayPanelNextMixLoad;
		pendingBarFightEncounterIndex = data.pendingBarFightEncounterIndex;
	}

	private void ClearProgressFlowFlags()
	{
		pendingBarFight = false;
		skipDayPanelNextMixLoad = false;
		pendingBarFightEncounterIndex = 0;
		openPauseMenuOnNextFightSceneLoad = false;
	}

	public void WriteTutorialExitMaintenancePlayerPrefs(int wipedCoins)
	{
		PlayerPrefs.SetInt(TutorialExitMaintenanceAmountKey, Mathf.Max(0, wipedCoins));
		PlayerPrefs.SetInt(TutorialExitMaintenancePendingKey, 1);
		PlayerPrefs.Save();
	}

    public void MarkTutorialCompleted()
    {
        if (TutorialCompleted)
            return;

		// Fair start: when leaving tutorial, wipe coins to 0 and remember the amount
		// so the Day screen can show it as a "maintenance cost" popup (no lose condition).
		int wiped = Coins;
		WriteTutorialExitMaintenancePlayerPrefs(wiped);
		Coins = 0;

        TutorialCompleted = true;
        SaveSystem.WriteData();
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
		if (amount > 0)
			CoinFlyAnimator.NotifyCoinsAdded(amount);
		// Persist coins only at checkpoints; saving here allowed duplicating coins if reload preceded encounter save.
    }

    public void IncrementDay(int maintenanceCost)
    {
        Day++;

        if (Day > 1 && maintenanceCost > 0)
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

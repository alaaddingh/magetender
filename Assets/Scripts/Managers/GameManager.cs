using UnityEngine;
using Magetender.Data;

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


    public int Coins { get; private set; }
    public int Day { get; private set; }

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
            EncounterIndex = data.currentEncounterIndex;
            SavedEncounterIndex = true;
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
}

using UnityEngine;

/* Holds coins and day. Put one GameManager in the scene; it survives scene load via DontDestroyOnLoad. */
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Starting values")]
    [SerializeField] private int startingCoins = 0;
    [SerializeField] private int startingDay = 1;

    public int Coins { get; private set; }
    public int Day { get; private set; }

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
    }

    public void AddCoins(int amount)
    {
        Coins = Mathf.Max(0, Coins + amount);
    }

    public void IncrementDay()
    {
        Day++;
    }
}

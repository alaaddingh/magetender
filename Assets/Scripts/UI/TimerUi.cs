using TMPro;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    public float duration = 30f;
    private float timeRemaining;
    private bool isRunning = false;

    [SerializeField] private bool timeUp = false;
    
    public bool TimeUp = false;
    


    public TextMeshProUGUI timerText;

    [Header("Panels")]
    [SerializeField] private GameObject TimerPanel;
    [SerializeField] private GameObject BasePanel;
    [SerializeField] private GameObject IngredientsPanel;
    [SerializeField] private GameObject ServePanel;




    


    void OnEnable()
    {
        StartTimer();
    }

    void Update()
    {
        if (!isRunning) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            TimeUp = true; 
            timeRemaining = 0f;
            isRunning = false;
            TimerPanel.SetActive(false);
            BasePanel.SetActive(false);
            IngredientsPanel.SetActive(false);
            ServePanel.SetActive(true);
           
            /* set monster to angry */
            
            Debug.Log("Timer finished");
        }

        UpdateUI();
    }

    void StartTimer()
    {
        timeRemaining = duration;
        isRunning = true;
    }

    void UpdateUI()
    {
        timerText.text = Mathf.Ceil(timeRemaining).ToString();
    }
}
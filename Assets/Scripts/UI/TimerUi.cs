using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    public float duration = 30f;
    private float timeRemaining;
    private bool isRunning = false;

    [SerializeField] private bool timeUp = false;
    
    public bool TimeUp = false;
    
    public TextMeshProUGUI timerText;
    public Image ClockImg;
    private Vector3 clockOriginalPos;

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
        timerText.color = Color.white;
        clockOriginalPos = ClockImg.rectTransform.localPosition;
    }

    void UpdateUI()
    {
        timerText.text = Mathf.Ceil(timeRemaining).ToString();
        timerText.color = timeRemaining <= 10f ? Color.red : Color.white;
            if (timeRemaining > 0f && timeRemaining <= 10f)
                ShakeClockImage();
          
    }

    void ShakeClockImage()
    {
        float intensity = 200f;
        Vector3 shakeOffset = new Vector3(
            Random.Range(-intensity, intensity),
            Random.Range(-intensity, intensity),
            0f) * 0.02f;
        ClockImg.rectTransform.localPosition = clockOriginalPos + shakeOffset;
    }
}
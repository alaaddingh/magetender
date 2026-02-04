using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QTECombatManager : MonoBehaviour
{
    [Header("UI References")]
    public Image customerSprite;
    public TextMeshProUGUI keyPromptText;
    public Image screenFlashOverlay;
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI customerHealthText;
    public Image timerCircle;
    public Canvas mainCanvas;
    
    [Header("Combat Settings")]
    public float qteTimeLimit = 1.5f;
    public int playerMaxHealth = 100;
    public int customerMaxHealth = 100;
    public int damagePerHit = 20;
    
    [Header("Visual Settings")]
    public Color damageFlashColor = Color.red;
    public float flashDuration = 0.2f;
    public float shakeIntensity = 15f;
    public float shakeDuration = 0.3f;
    
    [Header("Button Randomization")]
    public RectTransform buttonContainer; // The object that holds KeyPrompt and TimerCircle
    public float minX = -300f;
    public float maxX = 300f;
    public float minY = -150f;
    public float maxY = 150f;
    
    // Private variables
    private KeyCode[] possibleKeys = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.Space };
    private KeyCode currentKey;
    private float qteTimer;
    private bool waitingForInput;
    
    private int playerHealth;
    private int customerHealth;
    
    private Color originalCustomerColor;
    private Vector3 originalCustomerPosition;
    private Vector3 originalCanvasPosition;
    
    void Start()
    {
        playerHealth = playerMaxHealth;
        customerHealth = customerMaxHealth;
        originalCustomerColor = customerSprite.color;
        originalCustomerPosition = customerSprite.transform.localPosition;
        originalCanvasPosition = mainCanvas.transform.localPosition;
        
        // Make sure screen flash is invisible at start
        Color overlayColor = screenFlashOverlay.color;
        overlayColor.a = 0f;
        screenFlashOverlay.color = overlayColor;
        
        // Initialize timer circle
        timerCircle.fillAmount = 1f;
        
        UpdateHealthDisplays();
        StartNewQTE();
    }
    
    void Update()
    {
        if (!waitingForInput) return;
        
        // Update timer
        qteTimer -= Time.deltaTime;
        
        // Update circular timer fill
        float timerProgress = qteTimer / qteTimeLimit;
        timerCircle.fillAmount = timerProgress;
        
        // Change timer color based on time remaining
        if (timerProgress < 0.33f)
        {
            timerCircle.color = Color.red;
        }
        else if (timerProgress < 0.66f)
        {
            timerCircle.color = new Color(1f, 0.6f, 0f); // Orange
        }
        else
        {
            timerCircle.color = Color.yellow;
        }
        
        // Check for key press
        if (Input.GetKeyDown(currentKey))
        {
            // Correct key pressed!
            CustomerTakesDamage();
            StartCoroutine(WaitAndStartNewQTE(0.5f));
        }
        else if (CheckForWrongKeyPress())
        {
            // Wrong key pressed!
            PlayerTakesDamage();
            StartCoroutine(WaitAndStartNewQTE(0.5f));
        }
        
        // Time ran out
        if (qteTimer <= 0f)
        {
            PlayerTakesDamage();
            StartCoroutine(WaitAndStartNewQTE(0.5f));
        }
    }
    
    void StartNewQTE()
    {
        // Check if fight is over
        if (customerHealth <= 0)
        {
            keyPromptText.text = "VICTORY!";
            waitingForInput = false;
            timerCircle.fillAmount = 0f;
            return;
        }
        
        if (playerHealth <= 0)
        {
            keyPromptText.text = "DEFEATED!";
            waitingForInput = false;
            timerCircle.fillAmount = 0f;
            return;
        }
        
        // Pick random key
        currentKey = possibleKeys[Random.Range(0, possibleKeys.Length)];
        
        // Display it (just the key, no "PRESS:" prefix)
        keyPromptText.text = currentKey.ToString();
        
        // Randomize button position
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);
        buttonContainer.anchoredPosition = new Vector2(randomX, randomY);
        
        // Reset timer
        qteTimer = qteTimeLimit;
        timerCircle.fillAmount = 1f;
        timerCircle.color = Color.yellow;
        waitingForInput = true;
    }
    
    bool CheckForWrongKeyPress()
    {
        // Check if ANY key was pressed this frame
        if (Input.anyKeyDown)
        {
            // If it wasn't the correct key, it's wrong
            if (!Input.GetKeyDown(currentKey))
            {
                return true;
            }
        }
        return false;
    }
    
    void CustomerTakesDamage()
    {
        waitingForInput = false;
        customerHealth -= damagePerHit;
        customerHealth = Mathf.Max(0, customerHealth);
        
        UpdateHealthDisplays();
        Debug.Log("Customer Hit! Health: " + customerHealth);
        StartCoroutine(FlashCustomer());
    }
    
    void PlayerTakesDamage()
    {
        waitingForInput = false;
        playerHealth -= damagePerHit;
        playerHealth = Mathf.Max(0, playerHealth);
        
        UpdateHealthDisplays();
        Debug.Log("Player Hit! Health: " + playerHealth);
        StartCoroutine(FlashScreen());
    }
    
    IEnumerator FlashCustomer()
    {
        // Get all Image components in customer and children
        Image[] allImages = customerSprite.GetComponentsInChildren<Image>();
        Color[] originalColors = new Color[allImages.Length];
        
        // Store original colors and flash red
        for (int i = 0; i < allImages.Length; i++)
        {
            originalColors[i] = allImages[i].color;
            allImages[i].color = damageFlashColor;
        }
        
        // Shake
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            // Random offset for shake
            float xOffset = Random.Range(-shakeIntensity, shakeIntensity);
            float yOffset = Random.Range(-shakeIntensity, shakeIntensity);
            customerSprite.transform.localPosition = originalCustomerPosition + new Vector3(xOffset, yOffset, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Return to original position and colors
        customerSprite.transform.localPosition = originalCustomerPosition;
        for (int i = 0; i < allImages.Length; i++)
        {
            allImages[i].color = originalColors[i];
        }
    }
    
    IEnumerator FlashScreen()
    {
        Color overlayColor = screenFlashOverlay.color;
        overlayColor.a = 0.4f;
        screenFlashOverlay.color = overlayColor;
        
        // Screen shake
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            // Random offset for shake
            float xOffset = Random.Range(-shakeIntensity, shakeIntensity);
            float yOffset = Random.Range(-shakeIntensity, shakeIntensity);
            mainCanvas.transform.localPosition = originalCanvasPosition + new Vector3(xOffset, yOffset, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Return to original position
        mainCanvas.transform.localPosition = originalCanvasPosition;
        
        overlayColor.a = 0f;
        screenFlashOverlay.color = overlayColor;
    }
    
    IEnumerator WaitAndStartNewQTE(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNewQTE();
    }
    
    void UpdateHealthDisplays()
    {
        playerHealthText.text = playerHealth + " / " + playerMaxHealth;
        customerHealthText.text = customerHealth + " / " + customerMaxHealth;
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class QTECombatManager : MonoBehaviour
{
    [Header("UI References")]
    public Image customerSprite;
    public Image screenFlashOverlay;
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI customerHealthText;
    public Canvas mainCanvas;
    public GameObject promptPrefab; // Prefab containing: Background, TimerCircle, KeyText
    public Transform promptContainer; // Parent object to hold all active prompts
    
    [Header("Combat Settings")]
    public float qteTimeLimit = 1.5f;
    public int playerMaxHealth = 100;
    public int customerMaxHealth = 100;
    public int damagePerHit = 20;
    
    [Header("Prompt Settings")]
    public int minPromptsPerWave = 1;
    public int maxPromptsPerWave = 3;
    public float promptSpacing = 200f; // Minimum distance between prompts
    
    [Header("Visual Settings")]
    public Color damageFlashColor = Color.red;
    public float flashDuration = 0.2f;
    public float shakeIntensity = 15f;
    public float shakeDuration = 0.3f;
    
    [Header("Spawn Area")]
    public float minX = -350f;
    public float maxX = 350f;
    public float minY = -200f;
    public float maxY = 200f;
    
    [Header("Prompt Colors")]
    public Color defenseColor = new Color(1f, 0.3f, 0.3f, 0.8f);
    public Color attackColor = new Color(0.3f, 0.6f, 1f, 0.8f);
    
    // Private variables
    private KeyCode[] defenseKeys = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };
    private KeyCode[] attackKeys = { KeyCode.H, KeyCode.U, KeyCode.J, KeyCode.K };
    
    private List<PromptInstance> activePrompts = new List<PromptInstance>();
    private List<KeyCode> usedKeys = new List<KeyCode>(); // Track keys already in use
    
    private int playerHealth;
    private int customerHealth;
    
    private Color originalCustomerColor;
    private Vector3 originalCustomerPosition;
    private Vector3 originalCanvasPosition;
    private Coroutine currentCustomerFlash;
    private Coroutine currentScreenFlash;
    
    private class PromptInstance
    {
        public GameObject promptObject;
        public KeyCode keyCode;
        public bool isDefense;
        public float timer;
        public Image background;
        public Image timerCircle;
        public TextMeshProUGUI keyText;
        public Vector2 position;
    }
    
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
        
        UpdateHealthDisplays();
        StartNewWave();
    }
    
    void Update()
    {
        if (activePrompts.Count == 0) return;
        
        // Update all active prompts
        for (int i = activePrompts.Count - 1; i >= 0; i--)
        {
            PromptInstance prompt = activePrompts[i];
            prompt.timer -= Time.deltaTime;
            
            // Update visual timer
            float progress = prompt.timer / qteTimeLimit;
            prompt.timerCircle.fillAmount = progress;
            
            // Change color based on time
            if (progress < 0.33f)
                prompt.timerCircle.color = Color.red;
            else if (progress < 0.66f)
                prompt.timerCircle.color = new Color(1f, 0.6f, 0f);
            else
                prompt.timerCircle.color = Color.yellow;
            
            // Check if time ran out
            if (prompt.timer <= 0f)
            {
                HandleTimeout(prompt);
                RemovePrompt(i);
                
                // Check if wave is complete
                if (activePrompts.Count == 0)
                {
                    StartCoroutine(WaitAndStartNewWave(0.5f));
                }
            }
        }
        
        // Check for key presses
        CheckKeyPresses();
    }
    
    void CheckKeyPresses()
    {
        // Check all possible keys
        foreach (KeyCode key in defenseKeys)
        {
            if (Input.GetKeyDown(key))
            {
                HandleKeyPress(key);
                return;
            }
        }
        
        foreach (KeyCode key in attackKeys)
        {
            if (Input.GetKeyDown(key))
            {
                HandleKeyPress(key);
                return;
            }
        }
    }
    
    void HandleKeyPress(KeyCode pressedKey)
    {
        // Find matching prompt
        for (int i = 0; i < activePrompts.Count; i++)
        {
            if (activePrompts[i].keyCode == pressedKey)
            {
                // Correct key!
                HandleCorrectKey(activePrompts[i]);
                RemovePrompt(i);
                
                // Check if wave is complete
                if (activePrompts.Count == 0)
                {
                    StartCoroutine(WaitAndStartNewWave(0.5f));
                }
                return;
            }
        }
        
        // Wrong key pressed - check if it's a defense key
        if (System.Array.IndexOf(defenseKeys, pressedKey) >= 0)
        {
            // Pressed a defense key that doesn't match any prompt - minor penalty?
            Debug.Log("Wrong defense key pressed!");
        }
        else if (System.Array.IndexOf(attackKeys, pressedKey) >= 0)
        {
            // Pressed an attack key that doesn't match - no penalty
            Debug.Log("Wrong attack key pressed - no penalty");
        }
    }
    
    void HandleCorrectKey(PromptInstance prompt)
    {
        if (prompt.isDefense)
        {
            Debug.Log("Defense successful! Blocked attack!");
        }
        else
        {
            CustomerTakesDamage();
        }
    }
    
    void HandleTimeout(PromptInstance prompt)
    {
        if (prompt.isDefense)
        {
            // Failed to defend - take damage
            PlayerTakesDamage();
        }
        else
        {
            // Missed attack opportunity - no penalty
            Debug.Log("Attack missed - no damage");
        }
    }
    
    void StartNewWave()
    {
        // Check if fight is over
        if (customerHealth <= 0 || playerHealth <= 0)
        {
            EndFight();
            return;
        }
        
        // Clear used keys
        usedKeys.Clear();
        
        // Determine number of prompts
        int promptCount = Random.Range(minPromptsPerWave, maxPromptsPerWave + 1);
        
        // Create prompts
        List<Vector2> spawnPositions = GenerateSpawnPositions(promptCount);
        
        for (int i = 0; i < promptCount; i++)
        {
            CreatePrompt(spawnPositions[i]);
        }
    }
    
    List<Vector2> GenerateSpawnPositions(int count)
    {
        List<Vector2> positions = new List<Vector2>();
        int maxAttempts = 50;
        
        for (int i = 0; i < count; i++)
        {
            Vector2 newPos = Vector2.zero;
            bool validPosition = false;
            
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                newPos = new Vector2(
                    Random.Range(minX, maxX),
                    Random.Range(minY, maxY)
                );
                
                // Check distance from existing positions
                validPosition = true;
                foreach (Vector2 existingPos in positions)
                {
                    if (Vector2.Distance(newPos, existingPos) < promptSpacing)
                    {
                        validPosition = false;
                        break;
                    }
                }
                
                if (validPosition) break;
            }
            
            positions.Add(newPos);
        }
        
        return positions;
    }
    
    void CreatePrompt(Vector2 position)
    {
        // Instantiate prompt
        GameObject promptObj = Instantiate(promptPrefab, promptContainer);
        RectTransform rectTransform = promptObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        
        // Get components
        Image background = promptObj.GetComponent<Image>();
        Image timerCircle = promptObj.transform.Find("TimerCircle").GetComponent<Image>();
        TextMeshProUGUI keyText = promptObj.transform.Find("TimerCircle/KeyText").GetComponent<TextMeshProUGUI>();
        
        // Randomly choose defense or attack
        bool isDefense = Random.value > 0.5f;
        KeyCode[] keyPool = isDefense ? defenseKeys : attackKeys;
        
        // Pick a key that's not already in use
        KeyCode selectedKey = GetUnusedKey(keyPool);
        usedKeys.Add(selectedKey);
        
        // Set visuals
        background.color = isDefense ? defenseColor : attackColor;
        keyText.text = selectedKey.ToString();
        timerCircle.fillAmount = 1f;
        timerCircle.color = Color.yellow;
        
        // Create instance
        PromptInstance instance = new PromptInstance
        {
            promptObject = promptObj,
            keyCode = selectedKey,
            isDefense = isDefense,
            timer = qteTimeLimit,
            background = background,
            timerCircle = timerCircle,
            keyText = keyText,
            position = position
        };
        
        activePrompts.Add(instance);
    }
    
    KeyCode GetUnusedKey(KeyCode[] keyPool)
    {
        List<KeyCode> availableKeys = new List<KeyCode>();
        
        foreach (KeyCode key in keyPool)
        {
            if (!usedKeys.Contains(key))
            {
                availableKeys.Add(key);
            }
        }
        
        // If all keys are used, allow repeats
        if (availableKeys.Count == 0)
        {
            return keyPool[Random.Range(0, keyPool.Length)];
        }
        
        return availableKeys[Random.Range(0, availableKeys.Count)];
    }
    
    void RemovePrompt(int index)
    {
        Destroy(activePrompts[index].promptObject);
        activePrompts.RemoveAt(index);
    }
    
    void CustomerTakesDamage()
    {
        customerHealth -= damagePerHit;
        customerHealth = Mathf.Max(0, customerHealth);
        
        UpdateHealthDisplays();
        Debug.Log("Customer Hit! Health: " + customerHealth);
        
        // Stop previous flash if still running
        if (currentCustomerFlash != null)
        {
            StopCoroutine(currentCustomerFlash);
        }
        
        // Start new flash
        currentCustomerFlash = StartCoroutine(FlashCustomer());
    }
    
    void PlayerTakesDamage()
    {
        playerHealth -= damagePerHit;
        playerHealth = Mathf.Max(0, playerHealth);
        
        UpdateHealthDisplays();
        Debug.Log("Player Hit! Health: " + playerHealth);
        
        // Stop previous flash if still running
        if (currentScreenFlash != null)
        {
            StopCoroutine(currentScreenFlash);
        }
        
        // Start new flash
        currentScreenFlash = StartCoroutine(FlashScreen());
    }
    
    void EndFight()
    {
        // Clear all prompts
        foreach (var prompt in activePrompts)
        {
            Destroy(prompt.promptObject);
        }
        activePrompts.Clear();
        
        // Show result
        if (customerHealth <= 0)
        {
            Debug.Log("VICTORY!");
        }
        else
        {
            Debug.Log("DEFEATED!");
        }
    }
    
    IEnumerator FlashCustomer()
    {
        Image[] allImages = customerSprite.GetComponentsInChildren<Image>();
        Color[] originalColors = new Color[allImages.Length];
        
        for (int i = 0; i < allImages.Length; i++)
        {
            originalColors[i] = allImages[i].color;
            allImages[i].color = damageFlashColor;
        }
        
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float xOffset = Random.Range(-shakeIntensity, shakeIntensity);
            float yOffset = Random.Range(-shakeIntensity, shakeIntensity);
            customerSprite.transform.localPosition = originalCustomerPosition + new Vector3(xOffset, yOffset, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
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
        
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float xOffset = Random.Range(-shakeIntensity, shakeIntensity);
            float yOffset = Random.Range(-shakeIntensity, shakeIntensity);
            mainCanvas.transform.localPosition = originalCanvasPosition + new Vector3(xOffset, yOffset, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        mainCanvas.transform.localPosition = originalCanvasPosition;
        overlayColor.a = 0f;
        screenFlashOverlay.color = overlayColor;
    }
    
    IEnumerator WaitAndStartNewWave(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNewWave();
    }
    
    void UpdateHealthDisplays()
    {
        playerHealthText.text = playerHealth + " / " + playerMaxHealth;
        customerHealthText.text = customerHealth + " / " + customerMaxHealth;
    }
}
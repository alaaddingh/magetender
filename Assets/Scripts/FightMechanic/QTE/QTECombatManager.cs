using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

// QTE Bank Combat System
// Two banks (defend/attack) with key sequences
// Player switches between them with SPACE
public class QTECombatManager : MonoBehaviour
{
	[Header("UI References")]
	public Image customerSprite;
	public Image backgroundImage;
	public Image screenFlashOverlay;
	public TextMeshProUGUI playerHealthText;
	public TextMeshProUGUI customerHealthText;
	public TextMeshProUGUI playerHealthLabel;
	public TextMeshProUGUI customerHealthLabel;
	public Canvas mainCanvas;
	public RectTransform leftHand;
	public RectTransform rightHand;
	
	[Header("Back to bar (show when fight ends)")]
	public GameObject backToBarButton;
	public string backToBarSceneName = "MixScene";
	
	[Header("Prompt Bank References")]
	public GameObject defendBankContainer;
	public GameObject attackBankContainer;
	public Image defendBankGlow;
	public Image attackBankGlow;
	public Image defendTimerBar;
	public Image attackTimerBar;
	public TextMeshProUGUI[] defendKeyTexts;
	public TextMeshProUGUI[] attackKeyTexts;
	
	[Header("Combat Settings")]
	public int playerMaxHealth = 100;
	public int customerMaxHealth = 100;
	public int damagePerHit = 20;
	public float bankTimeLimit = 5f;
	public int sequenceLength = 5;
	[SerializeField] private int coinsForWin = 10;
	
	[Header("Visual Settings")]
	public float shakeIntensity = 15f;
	public float shakeDuration = 0.3f;
	public Color activeGlowColor = new Color(1f, 1f, 0.5f, 0.6f);
	public Color inactiveGlowColor = new Color(0.3f, 0.3f, 0.3f, 0.2f);
	
	// WASD only per TA feedback - simpler and more accessible
	private KeyCode[] availableKeys = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };
	private int playerHealth;
	private int customerHealth;
	private Color originalCustomerColor;
	private Vector3 originalCustomerPosition;
	private Vector3 originalCanvasPosition;
	private Coroutine currentCustomerFlash;
	private Coroutine currentScreenFlash;
	private Vector2 leftHandOriginalPos;
	private Vector2 rightHandOriginalPos;
	private bool useLeftHand = true;
	
	// Loaded customer sprites
	private Sprite customerAngrySprite;
	private Sprite customerNeutralSprite;
	private Sprite customerHappySprite;
	
	// Bank system variables
	private List<KeyCode> defendSequence = new List<KeyCode>();
	private List<KeyCode> attackSequence = new List<KeyCode>();
	private int defendProgress = 0;
	private int attackProgress = 0;
	private float defendTimer;
	private float attackTimer;
	private bool defendBankActive = true; // start on defend since it's more urgent
    private bool combatStarted = false; // Start paused for tutorial
	
	void Start()
	{
		// Load monster data from CurrentMonster
		LoadMonsterData();
		
		playerHealth = playerMaxHealth;
		customerHealth = customerMaxHealth;
		
		if (customerSprite != null)
		{
			originalCustomerColor = customerSprite.color;
			originalCustomerPosition = customerSprite.transform.localPosition;
			
			// Set angry sprite at fight start
			if (customerAngrySprite != null)
			{
				customerSprite.sprite = customerAngrySprite;
			}
		}
		
		originalCanvasPosition = mainCanvas.transform.localPosition;
		
		if (leftHand != null)
		{
			leftHandOriginalPos = leftHand.anchoredPosition;
		}
		if (rightHand != null)
		{
			rightHandOriginalPos = rightHand.anchoredPosition;
		}
		
		if (screenFlashOverlay != null)
		{
			Color overlayColor = screenFlashOverlay.color;
			overlayColor.a = 0f;
			screenFlashOverlay.color = overlayColor;
		}
		
		UpdateHealthDisplays();
        // Don't start sequences yet - wait for tutorial
	}

    // Public method for tutorial to call
    public void StartCombat()
    {
        combatStarted = true;
        GenerateNewSequence(true);
        GenerateNewSequence(false);
        UpdateBankVisuals();
    }
	
	void LoadMonsterData()
	{
		// Check if CurrentMonster exists
		if (CurrentMonster.Instance == null)
		{
			Debug.LogWarning("CurrentMonster.Instance is null! Using default sprites.");
			return;
		}
		
		MonsterData monsterData = CurrentMonster.Instance.Data;
		if (monsterData == null)
		{
			Debug.LogWarning("Monster data is null! Using default sprites.");
			return;
		}
		
		// Load sprites from Resources using the paths in the JSON
		if (!string.IsNullOrEmpty(monsterData.sprites.angry))
		{
			customerAngrySprite = Resources.Load<Sprite>(monsterData.sprites.angry);
		}
		if (!string.IsNullOrEmpty(monsterData.sprites.neutral))
		{
			customerNeutralSprite = Resources.Load<Sprite>(monsterData.sprites.neutral);
		}
		if (!string.IsNullOrEmpty(monsterData.sprites.happy))
		{
			customerHappySprite = Resources.Load<Sprite>(monsterData.sprites.happy);
		}
		
		Debug.Log($"Loaded monster: {monsterData.name}");
	}
	
	void Update()
	{
		// Don't run combat until tutorial dismissed
        if (!combatStarted)
        {
            return;
        }
        
        if (customerHealth <= 0 || playerHealth <= 0)
        {
            return;
        }
		
		// both timers always run - this creates the pressure!
		defendTimer -= Time.deltaTime;
		attackTimer -= Time.deltaTime;
		
		if (defendTimerBar != null)
		{
			defendTimerBar.fillAmount = Mathf.Max(0, defendTimer / bankTimeLimit);
		}
		if (attackTimerBar != null)
		{
			attackTimerBar.fillAmount = Mathf.Max(0, attackTimer / bankTimeLimit);
		}
		
		// Check for timeouts
		if (defendTimer <= 0f)
		{
			HandleDefendFail();
		}
		if (attackTimer <= 0f)
		{
			HandleAttackFail();
		}
		
		// Check for bank switch
		if (Input.GetKeyDown(KeyCode.Space))
		{
			SwitchActiveBank();
		}
		
		// Check for key presses in active bank
		foreach (KeyCode key in availableKeys)
		{
			if (Input.GetKeyDown(key))
			{
				HandleKeyPress(key);
				break;
			}
		}
	}
	
	void HandleKeyPress(KeyCode pressedKey)
	{
		if (defendBankActive)
		{
			// must press keys in order left to right
			if (defendProgress < defendSequence.Count && pressedKey == defendSequence[defendProgress])
			{
				// Correct key
				defendProgress++;
				UpdateBankVisuals();
				
				// Check if sequence complete
				if (defendProgress >= defendSequence.Count)
				{
					HandleDefendSuccess();
				}
			}
			else
			{
				// Wrong key
				HandleDefendFail();
			}
		}
		else
		{
			if (attackProgress < attackSequence.Count && pressedKey == attackSequence[attackProgress])
			{
				// Correct key
				attackProgress++;
				UpdateBankVisuals();
				
				// Check if sequence complete
				if (attackProgress >= attackSequence.Count)
				{
					HandleAttackSuccess();
				}
			}
			else
			{
				// Wrong key
				HandleAttackFail();
			}
		}
	}
	
	void SwitchActiveBank()
	{
		defendBankActive = !defendBankActive;
		UpdateBankVisuals();
	}
	
	void HandleDefendSuccess()
	{
		Debug.Log("Defense successful! Blocked attack!");
		GenerateNewSequence(true);
		UpdateBankVisuals();
	}
	
	void HandleDefendFail()
	{
		Debug.Log("Defense failed!");
		PlayerTakesDamage();
		GenerateNewSequence(true);
		UpdateBankVisuals();
	}
	
	void HandleAttackSuccess()
	{
		Debug.Log("Attack successful!");
		CustomerTakesDamage();
		GenerateNewSequence(false);
		UpdateBankVisuals();
	}
	
	void HandleAttackFail()
	{
		Debug.Log("Attack failed - missed opportunity");
		GenerateNewSequence(false);
		UpdateBankVisuals();
	}
	
	void GenerateNewSequence(bool isDefend)
	{
		List<KeyCode> newSequence = new List<KeyCode>();
		
		for (int i = 0; i < sequenceLength; i++)
		{
			KeyCode randomKey = availableKeys[Random.Range(0, availableKeys.Length)];
			newSequence.Add(randomKey);
		}
		
		if (isDefend)
		{
			defendSequence = newSequence;
			defendProgress = 0;
			defendTimer = bankTimeLimit;
		}
		else
		{
			attackSequence = newSequence;
			attackProgress = 0;
			attackTimer = bankTimeLimit;
		}
	}
	
	void UpdateBankVisuals()
	{
		// Update glow on active/inactive banks
		if (defendBankGlow != null)
		{
			defendBankGlow.color = defendBankActive ? activeGlowColor : inactiveGlowColor;
		}
		if (attackBankGlow != null)
		{
			attackBankGlow.color = defendBankActive ? inactiveGlowColor : activeGlowColor;
		}
		
		// Update defend bank key displays
		for (int i = 0; i < defendKeyTexts.Length && i < defendSequence.Count; i++)
		{
			if (defendKeyTexts[i] != null)
			{
				defendKeyTexts[i].text = defendSequence[i].ToString();
				
				// gray out done keys, highlight current one yellow
				if (i < defendProgress)
				{
					defendKeyTexts[i].color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
				}
				else if (i == defendProgress)
				{
					defendKeyTexts[i].color = Color.yellow;
				}
				else
				{
					defendKeyTexts[i].color = Color.white;
				}
			}
		}
		
		// Update attack bank key displays
		for (int i = 0; i < attackKeyTexts.Length && i < attackSequence.Count; i++)
		{
			if (attackKeyTexts[i] != null)
			{
				attackKeyTexts[i].text = attackSequence[i].ToString();
				
				if (i < attackProgress)
				{
					attackKeyTexts[i].color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
				}
				else if (i == attackProgress)
				{
					attackKeyTexts[i].color = Color.yellow;
				}
				else
				{
					attackKeyTexts[i].color = Color.white;
				}
			}
		}
	}
	
	void CustomerTakesDamage()
	{
		customerHealth -= damagePerHit;
		customerHealth = Mathf.Max(0, customerHealth);
		
		UpdateHealthDisplays();
		Debug.Log("Customer Hit! Health: " + customerHealth);
		
		if (customerHealth <= 0)
		{
			EndFight(true);
			return;
		}
		
		if (customerSprite != null)
		{
			if (currentCustomerFlash != null)
			{
				StopCoroutine(currentCustomerFlash);
			}
			currentCustomerFlash = StartCoroutine(FlashCustomer());
		}
		
		StartCoroutine(PunchAnimation());
	}
	
	void PlayerTakesDamage()
	{
		playerHealth -= damagePerHit;
		playerHealth = Mathf.Max(0, playerHealth);
		
		UpdateHealthDisplays();
		Debug.Log("Player Hit! Health: " + playerHealth);
		
		if (playerHealth <= 0)
		{
			EndFight(false);
			return;
		}
		
		if (currentScreenFlash != null)
		{
			StopCoroutine(currentScreenFlash);
		}
		currentScreenFlash = StartCoroutine(FlashScreen());
	}
	
	void EndFight(bool playerWon)
	{
		if (playerWon)
		{
			Debug.Log("VICTORY!");
			
			// Show defeated/happy sprite (monster is happy to be defeated and go home)
			if (customerHappySprite != null && customerSprite != null)
			{
				customerSprite.sprite = customerHappySprite;
			}
			else if (customerNeutralSprite != null && customerSprite != null)
			{
				customerSprite.sprite = customerNeutralSprite;
			}
			
			// Add coins for winning
			if (GameManager.Instance != null && coinsForWin > 0)
			{
				GameManager.Instance.AddCoins(coinsForWin);
			}
		}
		else
		{
			Debug.Log("DEFEATED!");
		}
		
		// Show back to bar button
		if (backToBarButton != null)
		{
			backToBarButton.SetActive(true);
		}
	}
	
	/* call from Back to bar button OnClick */
	public void OnBackToBarPressed()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.IncrementDay();
		}
		if (!string.IsNullOrEmpty(backToBarSceneName))
		{
			SceneManager.LoadScene(backToBarSceneName);
		}
	}
	
	IEnumerator FlashCustomer()
	{
		// Swap to hurt/neutral sprite
		if (customerNeutralSprite != null && customerSprite != null)
		{
			customerSprite.sprite = customerNeutralSprite;
		}
		
		Image[] allImages = customerSprite.GetComponentsInChildren<Image>();
		Color[] originalColors = new Color[allImages.Length];
		
		for (int i = 0; i < allImages.Length; i++)
		{
			originalColors[i] = allImages[i].color;
			allImages[i].color = Color.red;
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
		
		// Swap back to angry sprite
		if (customerAngrySprite != null && customerSprite != null)
		{
			customerSprite.sprite = customerAngrySprite;
		}
	}
	
	IEnumerator FlashScreen()
	{
		if (screenFlashOverlay != null)
		{
			Color overlayColor = screenFlashOverlay.color;
			overlayColor.a = 0.4f;
			screenFlashOverlay.color = overlayColor;
		}
		
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
		
		if (screenFlashOverlay != null)
		{
			Color overlayColor = screenFlashOverlay.color;
			overlayColor.a = 0f;
			screenFlashOverlay.color = overlayColor;
		}
	}
	
	IEnumerator PunchAnimation()
	{
		// alternate hands for variety
		RectTransform handToPunch = useLeftHand ? leftHand : rightHand;
		Vector2 originalPos = useLeftHand ? leftHandOriginalPos : rightHandOriginalPos;
		useLeftHand = !useLeftHand;
		
		if (handToPunch == null || customerSprite == null)
		{
			yield break;
		}
		
		RectTransform customerRect = customerSprite.GetComponent<RectTransform>();
		Vector3 handWorldPos = handToPunch.position;
		Vector3 customerWorldPos = customerRect.position;
		Vector2 direction = (customerWorldPos - handWorldPos).normalized;
		
		float punchDistance = 300f;
		Vector2 targetOffset = direction * punchDistance;
		Vector2 targetPos = originalPos + targetOffset;
		
		float punchDuration = 0.08f; // super fast punch
		float elapsed = 0f;
		
		while (elapsed < punchDuration)
		{
			float t = elapsed / punchDuration;
			float easeT = 1f - Mathf.Pow(1f - t, 3f);
			handToPunch.anchoredPosition = Vector2.Lerp(originalPos, targetPos, easeT);
			elapsed += Time.deltaTime;
			yield return null;
		}
		
		handToPunch.anchoredPosition = targetPos;
		yield return new WaitForSeconds(0.02f);
		
		float returnDuration = 0.15f;
		elapsed = 0f;
		
		while (elapsed < returnDuration)
		{
			float t = elapsed / returnDuration;
			handToPunch.anchoredPosition = Vector2.Lerp(targetPos, originalPos, t);
			elapsed += Time.deltaTime;
			yield return null;
		}
		
		handToPunch.anchoredPosition = originalPos;
	}
	
	void UpdateHealthDisplays()
	{
		if (playerHealthText != null)
		{
			playerHealthText.text = playerHealth + " / " + playerMaxHealth;
		}
		
		if (customerHealthText != null)
		{
			customerHealthText.text = customerHealth + " / " + customerMaxHealth;
		}
	}
}
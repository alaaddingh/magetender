using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// Progressive tutorial with spotlight and complete timer control
// Uses LanguageManager and UIStrings JSON for localization
public class CombatTutorial : MonoBehaviour
{
	private const string CombatTutorialCompletedKey = "CombatTutorialCompleted";

	[Header("Tutorial UI")]
	public GameObject tutorialPanel;
	public TextMeshProUGUI tutorialText;
	
	[Header("References")]
	public QTECombatManager combatManager;
	public GameObject defendBankContainer;
	public GameObject attackBankContainer;
	public TutorialSpotlight spotlight;
	public BankVisualFeedback defendBankFeedback;
	public BankVisualFeedback attackBankFeedback;
	
	private enum Stage { ShowingText, Practicing, Complete }
	private enum TutorialPhase { LearnDefend, LearnAttack, FinalWarning }
	
	private TutorialPhase currentPhase = TutorialPhase.LearnDefend;
	private Stage currentStage = Stage.ShowingText;
	
	private Dictionary<string, string> uiStrings;
	
	void Start()
	{
		LoadUIStrings();

		if (combatManager != null)
		{
			combatManager.OnDefendSequenceCompleted += OnDefendCompleted;
			combatManager.OnAttackSequenceCompleted += OnAttackCompleted;
		}

		if (PlayerPrefs.GetInt(CombatTutorialCompletedKey, 0) == 1)
		{
			SkipTutorial();
			return;
		}

		StartPhase_LearnDefend();
	}

	void SkipTutorial()
	{
		currentPhase = TutorialPhase.FinalWarning;
		currentStage = Stage.ShowingText;
		if (spotlight != null)
			spotlight.HideSpotlight();
		if (attackBankContainer != null)
			attackBankContainer.SetActive(true);
		if (combatManager != null)
		{
			combatManager.SetTutorialMode(false);
			combatManager.StartCombat();
			combatManager.PauseCombat();
		}
		SetTutorialText("stage3");
		if (tutorialPanel != null)
			tutorialPanel.SetActive(true);
	}
	
	void OnDestroy()
	{
		if (combatManager != null)
		{
			combatManager.OnDefendSequenceCompleted -= OnDefendCompleted;
			combatManager.OnAttackSequenceCompleted -= OnAttackCompleted;
		}
	}
	
	void Update()
	{
        UpdateBankFeedback();

		// Wait for SPACE to advance
		if (currentStage == Stage.ShowingText && Input.GetKeyDown(KeyCode.Space))
		{
			StartPracticing();
		}
	}
	
	// Load UI strings from JSON based on current language
	void LoadUIStrings()
	{
		uiStrings = new Dictionary<string, string>();
		
		// Get the correct JSON path from LanguageManager
		string path = "Data/UIStrings_en"; // Default
		if (LanguageManager.Instance != null)
		{
			path = LanguageManager.Instance.GetUIStringsResourcePath();
		}
		else
		{
			string lang = PlayerPrefs.GetString("GameLanguage", LanguageManager.LangEnglish);
			if (lang == LanguageManager.LangSpanish) path = "Data/UIStrings_es";
			else if (lang == LanguageManager.LangArabic) path = "Data/UIStrings_ar";
		}
		
		// Load JSON
		TextAsset jsonFile = Resources.Load<TextAsset>(path);
		if (jsonFile == null)
		{
			Debug.LogWarning($"Could not load UI strings from {path}");
			return;
		}
		
		// Parse JSON - expecting array of {key, value} objects
		UIStringsData data = JsonUtility.FromJson<UIStringsData>(jsonFile.text);
		if (data != null && data.entries != null)
		{
			foreach (var entry in data.entries)
			{
				uiStrings[entry.key] = entry.value;
			}
		}
	}
	
	// Get localized string by key
	string GetString(string key, params object[] args)
	{
		if (uiStrings.ContainsKey(key))
		{
			string text = uiStrings[key];
			if (args.Length > 0)
			{
				text = string.Format(text, args);
			}
			return text;
		}
		
		Debug.LogWarning($"UI string key '{key}' not found");
		return key;
	}
	
	// ===== PHASE 1: Learn Defend =====
	void StartPhase_LearnDefend()
	{
		currentPhase = TutorialPhase.LearnDefend;
		currentStage = Stage.ShowingText;
		
		// Setup scene
		combatManager.SetTutorialMode(true);
		combatManager.PauseCombat();  // PAUSE ALL TIMERS
		attackBankContainer.SetActive(false);
		tutorialPanel.SetActive(true);
		
		// Spotlight on defend bank
		if (spotlight != null)
		{
			spotlight.ShowSpotlightOn(defendBankContainer);
		}
		
		// Show text
		SetTutorialText("stage1");
		
		// Start combat (but paused)
		combatManager.StartCombat();
	}
	
	void OnDefendCompleted()
	{
		if (currentPhase == TutorialPhase.LearnDefend && currentStage == Stage.Practicing)
		{
			// Success! Move to next phase
			combatManager.PauseCombat();
			StartPhase_LearnAttack();
		}
	}
	
	// ===== PHASE 2: Learn Attack =====
	void StartPhase_LearnAttack()
	{
		currentPhase = TutorialPhase.LearnAttack;
		currentStage = Stage.ShowingText;
		
		// Show attack bank
		attackBankContainer.SetActive(true);
		
		// Spotlight on attack bank
		if (spotlight != null)
		{
			spotlight.ShowSpotlightOn(attackBankContainer);
		}
		
		// Show text
		SetTutorialText("stage2");
	}
	
	void OnAttackCompleted()
	{
		if (currentPhase == TutorialPhase.LearnAttack && currentStage == Stage.Practicing)
		{
			// Success! Move to final warning
			combatManager.PauseCombat();
			StartPhase_FinalWarning();
		}
	}
	
	// ===== PHASE 3: Final Warning =====
	void StartPhase_FinalWarning()
	{
		currentPhase = TutorialPhase.FinalWarning;
		currentStage = Stage.ShowingText;
		
		// Remove spotlight
		if (spotlight != null)
		{
			spotlight.HideSpotlight();
		}
		
		// Show warning
		SetTutorialText("stage3");
	}
	
	// Start practicing current phase
	void StartPracticing()
	{
		if (currentPhase == TutorialPhase.FinalWarning)
		{
			// Tutorial complete
			CompleteTutorial();
		}
		else
		{
			// Let player practice
			currentStage = Stage.Practicing;
			combatManager.ResumeCombat();  // UNPAUSE - timers run now
			
			// Hide tutorial text while practicing
			if (tutorialPanel != null)
			{
				tutorialPanel.SetActive(false);
			}
		}
	}
	
	void CompleteTutorial()
	{
		currentStage = Stage.Complete;
		PlayerPrefs.SetInt(CombatTutorialCompletedKey, 1);
		PlayerPrefs.Save();

		tutorialPanel.SetActive(false);
		combatManager.ResetHealthToFull();
		combatManager.SetTutorialMode(false);
		combatManager.ResumeCombat();
	}
	
	// ===== Helpers =====
	void SetTutorialText(string stage)
	{
		if (tutorialText == null) return;
		
		string monsterName = GetMonsterName();
		bool usingWASD = combatManager.IsUsingWASD();
		bool usingShift = combatManager.IsUsingShift();
		string text = "";
		
		switch (stage)
		{
			case "stage1":
				string stage1Key = usingWASD ? "tutorial_stage1_instructions_wasd" : "tutorial_stage1_instructions_arrows";
				text = GetString("tutorial_stage1_title", monsterName) + "\n\n" +
					GetString(stage1Key) + "\n\n" +
					GetString("tutorial_stage1_prompt");
				break;
				
			case "stage2":
				// Determine correct key based on both settings
				string stage2Key;
				if (usingWASD && usingShift)
					stage2Key = "tutorial_stage2_instructions_wasd_shift";
				else if (usingWASD && !usingShift)
					stage2Key = "tutorial_stage2_instructions_wasd_space";
				else if (!usingWASD && usingShift)
					stage2Key = "tutorial_stage2_instructions_arrows_shift";
				else
					stage2Key = "tutorial_stage2_instructions_arrows_space";
				
				text = GetString(stage2Key) + "\n\n" +
					GetString("tutorial_stage2_prompt");
				break;
				
			case "stage3":
				text = GetString("tutorial_stage3_warning") + "\n\n" +
					GetString("tutorial_stage3_prompt");
				break;
		}
		
		tutorialText.text = text;
		
		if (tutorialPanel != null)
		{
			tutorialPanel.SetActive(true);
		}
	}
	
	string GetMonsterName()
	{
		if (CurrentMonster.Instance != null && CurrentMonster.Instance.Data != null)
		{
			return CurrentMonster.Instance.Data.name;
		}
		return "This customer";
	}

    // Update visual feedback based on active bank
	void UpdateBankFeedback()
	{
		if (combatManager == null) return;
		
		bool defendActive = combatManager.IsDefendBankActive();
		
		if (defendBankFeedback != null)
		{
			defendBankFeedback.SetActive(defendActive);
		}
		
		if (attackBankFeedback != null)
		{
			attackBankFeedback.SetActive(!defendActive);
		}
	}
}

// JSON structure for UIStrings
[System.Serializable]
public class UIStringsData
{
	public UIStringEntry[] entries;
}

[System.Serializable]
public class UIStringEntry
{
	public string key;
	public string value;
}

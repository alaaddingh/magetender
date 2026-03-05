using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Progressive tutorial with spotlight and complete timer control
public class CombatTutorial : MonoBehaviour
{
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
	
	void Start()
	{
		// Subscribe to combat events
		if (combatManager != null)
		{
			combatManager.OnDefendSequenceCompleted += OnDefendCompleted;
			combatManager.OnAttackSequenceCompleted += OnAttackCompleted;
		}
		
		StartPhase_LearnDefend();
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
		// Update bank feedback based on which bank is active
		UpdateBankFeedback();
		
		// Wait for SPACE to advance
		if (currentStage == Stage.ShowingText && Input.GetKeyDown(KeyCode.Space))
		{
			StartPracticing();
		}
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
		SetTutorialText(
			$"<b>{GetMonsterName()}</b> is angry!\n\n" +
			"Complete the <color=red>DEFEND</color> sequence!\n" +
			"Use <color=yellow>the Arrow Keys</color> in order →\n\n" +
			"<size=28>Press <color=yellow>SPACE</color> to try it...</size>"
		);
		
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
		SetTutorialText(
			"Good! Now you can <color=blue>ATTACK</color>!\n\n" +
			"Press <color=yellow>SPACE</color> to switch banks\n" +
			"Complete an attack sequence!\n\n" +
			"<size=28>Press <color=yellow>SPACE</color> to try it...</size>"
		);
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
		SetTutorialText(
			"<b>Ready for the real fight?</b>\n\n" +
			"⚠️ <color=red>Both banks run at the same time!</color>\n" +
			"⚠️ Don't let DEFEND time out or you take damage!\n\n" +
			"Press <color=yellow>SPACE</color> to begin..."
		);
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
		
		// Hide tutorial
		tutorialPanel.SetActive(false);
		
		// Start real combat
		combatManager.SetTutorialMode(false);
		combatManager.ResumeCombat();
	}
	
	// ===== Helpers =====
	void SetTutorialText(string text)
	{
		if (tutorialText != null)
		{
			tutorialText.text = text;
			tutorialText.isRightToLeftText = false;
			tutorialText.alignment = TMPro.TextAlignmentOptions.Center;
		}
		
		// Show panel
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
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Simple tutorial that shows before combat starts

// TODO:
// - Add localization support (UIStrings.json)
// - Add difficulty system (read in data from customers and edit timer/health)

public class CombatTutorial : MonoBehaviour
{
    [Header("Tutorial UI")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;

    [Header("Combat Reference")]
    public QTECombatManager combatManager;

    private bool tutorialDismissed = false;

    void Start()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            SetupTutorialText();
        }
    }

    void Update()
    {
        // Wait for player to press SPACE to start
        if (!tutorialDismissed && tutorialPanel != null && tutorialPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                DismissTutorial();
            }
        }
    }

    void SetupTutorialText()
    {
        if (tutorialText == null)
        {
            return;
        }

        // Get monster name if available
        string monsterName = "This customer";
        if (CurrentMonster.Instance != null && CurrentMonster.Instance.Data != null)
        {
            monsterName = CurrentMonster.Instance.Data.name;
        }

        // TODO: Move to localization system (UIStrings.json)
        // Current language: English
        tutorialText.text = $"{monsterName} is looking for a fight!\n\n" +
                           "Press <color=yellow>WASD</color> to complete key sequences\n" +
                           "Press <color=yellow>SPACE</color> to switch between banks\n\n" +
                           "<color=red>DEFEND</color> - Don't let the timer run out!\n" +
                           "<color=blue>ATTACK</color> - Complete to deal damage\n\n" +
                           "Press <color=yellow>SPACE</color> to begin...";
    }

    void DismissTutorial()
    {
        tutorialDismissed = true;

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        // Tell combat manager to start
        if (combatManager != null)
        {
            combatManager.StartCombat();
        }
    }
}
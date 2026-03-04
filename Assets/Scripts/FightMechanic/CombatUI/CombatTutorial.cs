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

        string monsterName = CurrentMonster.Instance != null && CurrentMonster.Instance.Data != null
            ? CurrentMonster.Instance.Data.name
            : L.Get("tutorial_default_customer");
        tutorialText.text = L.Get("tutorial_body", monsterName);
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
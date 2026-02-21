/**
 * Purpose:
 * 1) Loads monster data
 * 2) Checks monster state from monsterstatemanager.cs to assure correct dialogue
 * 2) Controls monster dialogue on "Take Order" screen
 * 3) Makes brew button visible when needed
 */
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    [Header("UI variables")]
    [SerializeField] private TMP_Text monsterSpeech;

    [Header("Buttons")]
    [SerializeField] private GameObject brewButtonObject;

    [Header("Data source")]
    [SerializeField] private CurrentMonster currentMonsterManager;

    [Header("State source")]
    [SerializeField] private MonsterStateManager monsterStateManager;

    [Header("Ui Panels (to toggle/hide)")]
    public GameObject orderScreen;
    public GameObject selectingGlassScreen;

    [Header("Global coin canvas - hide when leaving order screen")]
    [SerializeField] private GameObject coinCanvas;

    private int dialogueIndex = 0;

    private void Start()
    {
        brewButtonObject.SetActive(false);

        if (currentMonsterManager == null)
            currentMonsterManager = CurrentMonster.Instance;

        Dialogue();
    }

    /* returns whichever dialogue list should be used based on monster state */
    private List<string> GetActiveDialogue()
    {
        if (currentMonsterManager == null) return new List<string>();

        string state = monsterStateManager.MonsterState;
        return currentMonsterManager.GetDialogue(state);
    }

    /* sets monster name, displays current dialogue index */
    private void Dialogue()
    {
        List<string> activeDialogue = GetActiveDialogue();
        if (activeDialogue == null || activeDialogue.Count == 0)
        {
            monsterSpeech.text = "";
            brewButtonObject.SetActive(true);
            return;
        }

        dialogueIndex = Mathf.Clamp(dialogueIndex, 0, activeDialogue.Count - 1);
        monsterSpeech.text = activeDialogue[dialogueIndex];
    }

    /* handles Next UI Button clicks (iteration + brew button visibility) */
    public void OnNextPressed()
    {
        List<string> activeDialogue = GetActiveDialogue();
        if (activeDialogue == null || activeDialogue.Count == 0)
        {
            brewButtonObject.SetActive(true);
            return;
        }

        /* if at the last line, show brew button. */
        bool last = dialogueIndex >= activeDialogue.Count - 1;
        if (last)
        {
            brewButtonObject.SetActive(true);
            return;
        }

        dialogueIndex++;
        Dialogue();

        if (dialogueIndex == activeDialogue.Count - 1)
        {
            brewButtonObject.SetActive(true);
        }
    }

    public void BrewingPressed()
    {
        if (coinCanvas != null)
            coinCanvas.SetActive(false);
        orderScreen.SetActive(false);
        selectingGlassScreen.SetActive(true);
    }
}

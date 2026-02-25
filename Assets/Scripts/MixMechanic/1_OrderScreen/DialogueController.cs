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
    [SerializeField] private TMP_Text monsterName;
    [SerializeField] private DialogueTypewriter typewriter;

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

    private void Awake()
    {
        if (currentMonsterManager == null)
            currentMonsterManager = CurrentMonster.Instance;
    }

    private void OnEnable()
    {
        if (currentMonsterManager != null)
            currentMonsterManager.OnMonsterChanged += HandleMonsterChanged;
    }

    private void OnDisable()
    {
        if (currentMonsterManager != null)
            currentMonsterManager.OnMonsterChanged -= HandleMonsterChanged;
    }

    private void Start()
    {
        brewButtonObject.SetActive(false);

        if (typewriter == null && monsterSpeech != null)
            typewriter = monsterSpeech.GetComponent<DialogueTypewriter>();

        UpdateMonsterName();
        Dialogue();
    }

    /* returns whichever dialogue list should be used based on monster state */
    private List<string> GetActiveDialogue()
    {
        if (currentMonsterManager == null) return new List<string>();

        string state = monsterStateManager != null ? monsterStateManager.MonsterState : "start";
        return currentMonsterManager.GetDialogue(state);
    }

    /* sets monster name, displays current dialogue index */
    private void Dialogue()
    {
        List<string> activeDialogue = GetActiveDialogue();
        if (activeDialogue == null || activeDialogue.Count == 0)
        {
            if (typewriter != null)
                typewriter.SetInstant(string.Empty);
            else
                monsterSpeech.text = string.Empty;

            brewButtonObject.SetActive(true);
            return;
        }

        dialogueIndex = Mathf.Clamp(dialogueIndex, 0, activeDialogue.Count - 1);
        string line = activeDialogue[dialogueIndex];

        if (typewriter != null)
            typewriter.TypeLine(line);
        else
            monsterSpeech.text = line;
    }

    /* handles Next UI Button clicks (iteration + brew button visibility) */
    public void OnNextPressed()
    {
        if (typewriter != null && typewriter.IsTyping)
        {
            typewriter.SkipTyping();
            return;
        }

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

    private void HandleMonsterChanged(string _)
    {
        dialogueIndex = 0;
        UpdateMonsterName();
        Dialogue();
    }

    private void UpdateMonsterName()
    {
        if (monsterName == null) return;
        if (currentMonsterManager == null || currentMonsterManager.Data == null)
        {
            monsterName.text = string.Empty;
            return;
        }

        monsterName.text = currentMonsterManager.Data.name;
    }
}

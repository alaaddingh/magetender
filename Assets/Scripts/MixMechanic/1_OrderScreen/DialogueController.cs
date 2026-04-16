/**
 * Purpose:
 * 1) Loads monster data
 * 2) Checks monster state from monsterstatemanager.cs to assure correct dialogue
 * 2) Controls monster dialogue on "Take Order" screen
 * 3) Makes brew button visible when needed
 */
using System.Collections.Generic;
using TMPro;
using RTLTMPro;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    [Header("UI variables")]
    [SerializeField] private TMP_Text monsterSpeech;
    [SerializeField] private TMP_Text monsterName;
    [SerializeField] private DialogueTypewriter typewriter;

    [Header("Buttons")]
    [SerializeField] private GameObject brewButtonObject;
    [SerializeField] private GameObject nextButtonObject; // optional (dialogue next)
    public GameObject continueButtonObject; // optional

    [Header("Data source")]
    [SerializeField] private CurrentMonster currentMonsterManager;

    [Header("State source")]
    [SerializeField] private MonsterStateManager monsterStateManager;

    [Header("Arabic (optional)")]
    [SerializeField] private TMP_FontAsset arabicFontOverride;

    [Header("Ui Panels (to toggle/hide)")]
    public GameObject orderScreen;
    public GameObject BaseScreen;

    public GameObject TimerCanvas;


    [Header("Global coin canvas - hide when leaving order screen")]
    [SerializeField] private GameObject coinCanvas;

    private int dialogueIndex = 0;
    private TMP_FontAsset monsterSpeechOriginalFont;
    private TMP_FontAsset monsterNameOriginalFont;
    private bool wasTyping;

    public bool IsDialogueFinished
    {
        get
        {
            var lines = GetActiveDialogue();
            if (lines.Count == 0)
                return true;

            if (dialogueIndex < lines.Count - 1)
                return false;

            if (typewriter != null && typewriter.enabled && typewriter.IsTyping)
                return false;

            return true;
        }
    }

    private void Awake()
    {
        currentMonsterManager = CurrentMonster.Instance;
        if (monsterSpeech == null)
        {
            Debug.LogError("[DialogueController] Missing monsterSpeech reference.", this);
            enabled = false;
            return;
        }

        monsterSpeechOriginalFont = monsterSpeech.font;
        if (monsterName != null)
            monsterNameOriginalFont = monsterName.font;

        if (typewriter == null)
            typewriter = monsterSpeech.GetComponent<DialogueTypewriter>();
        if (typewriter != null)
            typewriter.SetTargetText(monsterSpeech);
    }

    private void OnEnable()
    {
        if (!enabled) return;
        currentMonsterManager.OnMonsterChanged += HandleMonsterChanged;
        LanguageManager.OnLanguageChanged += HandleLanguageChanged;
    }

    private void OnDisable()
    {
        if (!enabled) return;
        currentMonsterManager.OnMonsterChanged -= HandleMonsterChanged;
        LanguageManager.OnLanguageChanged -= HandleLanguageChanged;
    }

    private void Start()
    {
        brewButtonObject.SetActive(false);
        if (continueButtonObject != null) continueButtonObject.SetActive(false);

        RefreshAll();
    }

    private void Update()
    {
        if (typewriter == null || !typewriter.enabled)
            return;

        if (wasTyping && !typewriter.IsTyping)
            UpdateContinueButtonState(GetActiveDialogue());

        wasTyping = typewriter.IsTyping;
    }

    private void HandleLanguageChanged()
    {
        RefreshAll();
    }

    private List<string> GetActiveDialogue()
    {
        string state = monsterStateManager != null ? monsterStateManager.MonsterState : "start";
        return currentMonsterManager.GetDialogue(state) ?? new List<string>();
    }

    private void RefreshAll()
    {
        UpdateTypewriterEnabledState();
        bool isArabic = RtlText.IsArabic();
        ApplyDialogueFontsForLanguage(isArabic);
        UpdateMonsterName();
        ShowCurrentLine();
    }

    public void OnNextPressed()
    {
		if (AudioManager.Instance != null)
			AudioManager.Instance.PlayButtonClick();

        UpdateTypewriterEnabledState();
        if (typewriter != null && typewriter.enabled && typewriter.IsTyping)
        {
            typewriter.SkipTyping();
            return;
        }

        var lines = GetActiveDialogue();
        if (lines.Count == 0)
        {
            brewButtonObject.SetActive(true);
            UpdateContinueButtonState(lines);
            return;
        }

        if (dialogueIndex < lines.Count - 1)
            dialogueIndex++;

        ShowCurrentLine(lines);
    }

    public void BrewingPressed()
    {
		if (AudioManager.Instance != null)
			AudioManager.Instance.PlayButtonClick();
		if (AudioManager.Instance != null)
			AudioManager.Instance.StopAmbience();
        if (coinCanvas != null)
            coinCanvas.SetActive(false);
        orderScreen.SetActive(false);
        /* begin timer */
        TimerCanvas.SetActive(true);
        BaseScreen.SetActive(true);
    }

    private void HandleMonsterChanged(string _)
    {
        dialogueIndex = 0;
        RefreshAll();
    }

    private void UpdateMonsterName()
    {
        if (monsterName == null) return;
        SetText(monsterName, currentMonsterManager.Data != null ? currentMonsterManager.Data.name : string.Empty, preserveNumbers: true);
    }

    private void UpdateTypewriterEnabledState()
    {
        if (typewriter == null)
            return;

        typewriter.enabled = !RtlText.IsArabic();
    }

    private void ApplyDialogueFontsForLanguage(bool isArabic)
    {
        if (monsterSpeech != null)
            monsterSpeech.font = isArabic && arabicFontOverride != null ? arabicFontOverride : monsterSpeechOriginalFont;
        if (monsterName != null)
            monsterName.font = isArabic && arabicFontOverride != null ? arabicFontOverride : monsterNameOriginalFont;
    }

    private void ShowCurrentLine()
    {
        ShowCurrentLine(GetActiveDialogue());
    }

    private void ShowCurrentLine(List<string> lines)
    {
        if (lines.Count == 0)
        {
            if (typewriter != null) typewriter.SetInstant(string.Empty);
            else SetText(monsterSpeech, string.Empty, preserveNumbers: true);

            brewButtonObject.SetActive(true);
            UpdateNextButtonState(lines);
            UpdateContinueButtonState(lines);
            return;
        }

        dialogueIndex = Mathf.Clamp(dialogueIndex, 0, lines.Count - 1);
        string rawLine = lines[dialogueIndex] ?? string.Empty;

        bool isArabic = RtlText.IsArabic();
        bool useTypewriter = typewriter != null && typewriter.enabled && !isArabic;
        if (useTypewriter) typewriter.TypeLine(rawLine);
        else SetText(monsterSpeech, rawLine, preserveNumbers: true);

        brewButtonObject.SetActive(dialogueIndex >= lines.Count - 1);
        UpdateNextButtonState(lines);
        UpdateContinueButtonState(lines);
    }

    private void UpdateNextButtonState(List<string> activeDialogue)
    {
        bool hasNextLine = activeDialogue.Count > 0 && dialogueIndex < activeDialogue.Count - 1;
        nextButtonObject.SetActive(hasNextLine);
    }

    private void SetText(TMP_Text t, string raw, bool preserveNumbers)
    {
        if (t == null) return;
        bool isArabic = RtlText.IsArabic();

        if (t is RTLTextMeshPro rtl)
        {
            rtl.Farsi = false;
            rtl.FixTags = true;
            rtl.PreserveNumbers = preserveNumbers;
            rtl.ForceFix = isArabic;
            rtl.text = raw ?? string.Empty;
            return;
        }

        if (isArabic)
        {
            t.isRightToLeftText = true;
            t.text = RtlText.FixIfArabic(raw, preserveNumbers: preserveNumbers, fixTags: true, reverseOutput: true);
        }
        else
        {
            t.isRightToLeftText = false;
            t.text = raw ?? string.Empty;
        }
    }

    private void UpdateContinueButtonState(List<string> activeDialogue)
    {
        if (continueButtonObject == null)
            return;

        if (monsterStateManager != null && monsterStateManager.MonsterState == "angry")
        {
            continueButtonObject.SetActive(false);
            return;
        }

        bool showContinue = activeDialogue.Count > 0 && dialogueIndex >= activeDialogue.Count - 1;
        if (showContinue && typewriter != null && typewriter.enabled && typewriter.IsTyping)
            showContinue = false;
        continueButtonObject.SetActive(showContinue);

        if (!showContinue)
            return;

        bool isLastMonsterOfDay = !currentMonsterManager.HasNextMonsterInCurrentLevel();
        string key = isLastMonsterOfDay ? "next_day_button" : "continue_button";

        var localizedText = continueButtonObject.GetComponentInChildren<LocalizedTMPText>(true);
        if (localizedText != null)
            localizedText.SetKey(key);
    }
}

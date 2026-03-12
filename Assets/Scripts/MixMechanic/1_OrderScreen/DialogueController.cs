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
    public GameObject continueButtonObject;

    [Header("Data source")]
    [SerializeField] private CurrentMonster currentMonsterManager;

    [Header("State source")]
    [SerializeField] private MonsterStateManager monsterStateManager;

    [Header("Arabic (optional)")]
    [SerializeField] private TMP_FontAsset arabicFontOverride;

    [Header("Ui Panels (to toggle/hide)")]
    public GameObject orderScreen;
    public GameObject selectingGlassScreen;

    [Header("Global coin canvas - hide when leaving order screen")]
    [SerializeField] private GameObject coinCanvas;

    private int dialogueIndex = 0;
    private TMP_FontAsset monsterSpeechOriginalFont;
    private TMP_FontAsset monsterNameOriginalFont;

    public bool IsDialogueFinished
    {
        get
        {
            List<string> activeDialogue = GetActiveDialogue();
            if (activeDialogue == null || activeDialogue.Count == 0)
                return true;

            bool atLast = dialogueIndex >= activeDialogue.Count - 1;
            if (!atLast)
                return false;

            // If typewriter is running, the last line isn't fully displayed yet.
            if (typewriter != null && typewriter.enabled && typewriter.IsTyping)
                return false;

            return true;
        }
    }

    private void Awake()
    {
        currentMonsterManager = CurrentMonster.Instance;
        if (monsterSpeech != null)
            monsterSpeechOriginalFont = monsterSpeech.font;
        if (monsterName != null)
            monsterNameOriginalFont = monsterName.font;
    }

    private void OnEnable()
    {
        if (currentMonsterManager != null)
            currentMonsterManager.OnMonsterChanged += HandleMonsterChanged;

        LanguageManager.OnLanguageChanged += HandleLanguageChanged;
    }

    private void OnDisable()
    {
        if (currentMonsterManager != null)
            currentMonsterManager.OnMonsterChanged -= HandleMonsterChanged;

        LanguageManager.OnLanguageChanged -= HandleLanguageChanged;
    }

    private void Start()
    {
        brewButtonObject.SetActive(false);
        ResolveContinueButtonIfNeeded();
        if (continueButtonObject != null)
            continueButtonObject.SetActive(false);

        if (typewriter == null && monsterSpeech != null)
            typewriter = monsterSpeech.GetComponent<DialogueTypewriter>();
        if (typewriter != null && monsterSpeech != null)
            typewriter.SetTargetText(monsterSpeech);

        UpdateTypewriterEnabledState();
        ApplyDialogueFontsForLanguage();
        UpdateMonsterName();
        Dialogue();
    }

    private void HandleLanguageChanged()
    {
        ApplyDialogueFontsForLanguage();
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
                SetText(monsterSpeech, string.Empty, preserveNumbers: true);

            brewButtonObject.SetActive(true);
            UpdateContinueButtonState(activeDialogue);
            return;
        }

        dialogueIndex = Mathf.Clamp(dialogueIndex, 0, activeDialogue.Count - 1);
        string rawLine = activeDialogue[dialogueIndex] ?? string.Empty;

        UpdateTypewriterEnabledState();
        bool isArabic = RtlText.IsArabic();
        bool useTypewriter = typewriter != null && typewriter.enabled && !isArabic;
        if (useTypewriter)
        {
            typewriter.TypeLine(rawLine);
        }
        else
        {
            // For Arabic, set full string at once to preserve glyph shaping.
            SetText(monsterSpeech, rawLine, preserveNumbers: true);
        }

        UpdateContinueButtonState(activeDialogue);
    }

    /* handles Next UI Button clicks (iteration + brew button visibility) */
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

        List<string> activeDialogue = GetActiveDialogue();
        if (activeDialogue == null || activeDialogue.Count == 0)
        {
            brewButtonObject.SetActive(true);
            UpdateContinueButtonState(activeDialogue);
            return;
        }

        /* if at the last line, show brew button. */
        bool last = dialogueIndex >= activeDialogue.Count - 1;
        if (last)
        {
            brewButtonObject.SetActive(true);
            UpdateContinueButtonState(activeDialogue);
            return;
        }

        dialogueIndex++;
        Dialogue();

        if (dialogueIndex == activeDialogue.Count - 1)
        {
            brewButtonObject.SetActive(true);
        }

        UpdateContinueButtonState(activeDialogue);
    }

    public void BrewingPressed()
    {
		if (AudioManager.Instance != null)
			AudioManager.Instance.PlayButtonClick();
        if (coinCanvas != null)
            coinCanvas.SetActive(false);
        orderScreen.SetActive(false);
        selectingGlassScreen.SetActive(true);
    }

    private void HandleMonsterChanged(string _)
    {
        dialogueIndex = 0;
        UpdateTypewriterEnabledState();
        ApplyDialogueFontsForLanguage();
        UpdateMonsterName();
        Dialogue();
    }

    private void UpdateMonsterName()
    {
        if (monsterName == null) return;
        if (currentMonsterManager == null || currentMonsterManager.Data == null)
        {
            SetText(monsterName, string.Empty, preserveNumbers: true);
            return;
        }

        SetText(monsterName, currentMonsterManager.Data.name, preserveNumbers: true);
    }

    private void UpdateTypewriterEnabledState()
    {
        if (typewriter == null && monsterSpeech != null)
            typewriter = monsterSpeech.GetComponent<DialogueTypewriter>();
        if (typewriter == null)
            return;

        if (monsterSpeech != null)
            typewriter.SetTargetText(monsterSpeech);

        typewriter.enabled = !RtlText.IsArabic();
    }

    private void ApplyDialogueFontsForLanguage()
    {
        bool isArabic = RtlText.IsArabic();

        ApplyFont(monsterSpeech, monsterSpeechOriginalFont, isArabic);
        ApplyFont(monsterName, monsterNameOriginalFont, isArabic);
    }

    private void ApplyFont(TMP_Text t, TMP_FontAsset originalFont, bool isArabic)
    {
        if (t == null)
            return;
        if (isArabic && arabicFontOverride != null)
            t.font = arabicFontOverride;
        else if (originalFont != null)
            t.font = originalFont;
    }

    private void SetText(TMP_Text t, string raw, bool preserveNumbers)
    {
        if (t == null)
            return;

        bool isArabic = RtlText.IsArabic();

        // If the scene is already using RTLTextMeshPro, let the plugin do the shaping.
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
        ResolveContinueButtonIfNeeded();
        if (continueButtonObject == null)
            return;

        bool showContinue = activeDialogue != null && activeDialogue.Count > 0 && dialogueIndex >= activeDialogue.Count - 1;
        if (showContinue && typewriter != null && typewriter.enabled && typewriter.IsTyping)
            showContinue = false;
        continueButtonObject.SetActive(showContinue);

        if (!showContinue)
            return;

        bool isLastMonsterOfDay = currentMonsterManager == null || !currentMonsterManager.HasNextMonsterInCurrentLevel();
        string key = isLastMonsterOfDay ? "next_day_button" : "continue_button";

        var localizedText = continueButtonObject.GetComponentInChildren<LocalizedTMPText>(true);
        if (localizedText != null)
            localizedText.SetKey(key);
    }

    private void ResolveContinueButtonIfNeeded()
    {
        if (continueButtonObject != null)
            return;

        // OrderDialogueController in MixScene doesn't have this wired; try to find it under the order screen first.
        if (orderScreen != null)
        {
            var t = orderScreen.transform.Find("ContinueButton") ?? orderScreen.transform.Find("ContinueButtonController");
            if (t != null)
            {
                continueButtonObject = t.gameObject;
                return;
            }
        }

        // Fallback: scene-wide lookup (last resort).
        var go = GameObject.Find("ContinueButton") ?? GameObject.Find("ContinueButtonController");
        if (go != null)
            continueButtonObject = go;
    }
}

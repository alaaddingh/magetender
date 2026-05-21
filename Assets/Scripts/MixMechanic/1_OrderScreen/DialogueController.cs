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
    [SerializeField] private TMP_Text serveSpeech;
    [SerializeField] private TMP_Text monsterName;
    [SerializeField] private DialogueTypewriter typewriter;

    [Header("Buttons")]
    [SerializeField] private GameObject brewButtonObject;
    [SerializeField] private GameObject nextButtonObject; // optional (dialogue next)
    [SerializeField] private GameObject fightButtonObject;
    public GameObject continueButtonObject; // optional

    [Header("Data source")]
    [SerializeField] private CurrentMonster currentMonsterManager;

    [Header("State source")]
    [SerializeField] private MonsterStateManager monsterStateManager;

    [Header("Arabic (optional)")]
    [SerializeField] private TMP_FontAsset arabicFontOverride;

    [Header("Ink Dialogue (optional)")]
    [SerializeField] private InkyDialogueController inkyDialogueController;
    [SerializeField] private InkDialoguePresenter inkDialoguePresenter;
    [SerializeField] private TextAsset orderInkJsonAsset;
    [SerializeField] private TextAsset serveInkJsonAsset;

    [SerializeField] private GameObject alienIDCanvas;

    [Header("Ui Panels (to toggle/hide)")]
    public GameObject orderScreen;
    public GameObject BaseScreen;

    public GameObject TimerCanvas;


    [Header("Global coin canvas - hide when leaving order screen")]
    [SerializeField] private GameObject coinCanvas;

    private int dialogueIndex = 0;
	private bool dialogueFightOfferAnalyticsSent;
    private TMP_FontAsset monsterSpeechOriginalFont;
    private TMP_FontAsset serveSpeechOriginalFont;
    private TMP_FontAsset monsterNameOriginalFont;
    private bool wasTyping;

    public bool IsDialogueFinished
    {
        get
        {
            if (UsesInkDialogue())
            {
                if (inkDialoguePresenter != null && inkDialoguePresenter.IsTyping)
                    return false;

                return inkyDialogueController == null || inkyDialogueController.IsFinished;
            }

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
        if (serveSpeech != null)
            serveSpeechOriginalFont = serveSpeech.font;
        if (monsterName != null)
            monsterNameOriginalFont = monsterName.font;

        if (typewriter == null)
            typewriter = GetActiveSpeechText()?.GetComponent<DialogueTypewriter>();
        if (typewriter != null)
            typewriter.SetTargetText(GetActiveSpeechText());
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
        //To ensure character voiceovers plays on first dialogue
        CharacterVoiceover();

        Debug.Log($"[DialogueController] Starting with monster='{currentMonsterManager.Data?.id}' state='{monsterStateManager?.MonsterState}'");
        brewButtonObject.SetActive(false);
        if (fightButtonObject != null) fightButtonObject.SetActive(false);
        if (continueButtonObject != null) continueButtonObject.SetActive(false);

        RefreshAll();
    }

    private void Update()
    {
        if (UsesInkDialogue())
        {
            bool isTyping = inkDialoguePresenter != null && inkDialoguePresenter.IsTyping;
            if (wasTyping && !isTyping)
            {
                RefreshInkButtonState();
                UpdateNextButtonStateInk();
                UpdateContinueButtonStateInk();
            }

            wasTyping = isTyping;
            return;
        }

        if (typewriter == null || !typewriter.enabled)
            return;

        if (wasTyping && !typewriter.IsTyping)
        {
            RefreshFightButton();
            UpdateContinueButtonState(GetActiveDialogue());
        }

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
        if (UsesInkDialogue() && TryStartInkDialogueForCurrentState())
        {
            SyncMonsterStateFromInkVariables();
            UpdateMonsterName();
            return;
        }

        UpdateTypewriterEnabledState();
        bool isArabic = RtlText.IsArabic();
        ApplyDialogueFontsForLanguage(isArabic);
        UpdateMonsterName();
        ShowCurrentLine();
    }

    public void OnNextPressed()
    {       

        CharacterVoiceover();

		if (AudioManager.Instance != null)
			AudioManager.Instance.PlayButtonClick();

        if (UsesInkDialogue())
        {
            if (inkDialoguePresenter != null)
                inkDialoguePresenter.AdvanceAndPresent();

            SyncMonsterStateFromInkVariables();
            RefreshInkButtonState();
            TryMakeTutorialToadAngryInk();
            CharacterVoiceover();
            return;
        }


        UpdateTypewriterEnabledState();
        if (typewriter != null && typewriter.enabled && typewriter.IsTyping)
        {
            typewriter.SkipTyping();
            AudioManager.Instance.StopVoice();
            return;
        }

        var lines = GetActiveDialogue();
        if (lines.Count == 0)
        {
            brewButtonObject.SetActive(CanShowBrewButton());
            UpdateContinueButtonState(lines);
            return;
        }

        if (dialogueIndex < lines.Count - 1)
            dialogueIndex++;

        ShowCurrentLine(lines);
    }

    public void CharacterVoiceover()
    {
        if (currentMonsterManager == null || currentMonsterManager.Data == null || monsterStateManager == null)
        {
            Debug.LogWarning("CharacterVoiceover: Missing manager references!");
            return;
        }

        if (AudioManager.Instance == null)
            return;

        string monsterId = currentMonsterManager.Data.id.ToLower().Trim();
        string monsterState = monsterStateManager.MonsterState.ToLower().Trim();

        Debug.Log($"CharacterVoiceover: Playing voice for monsterId='{monsterId}' state='{monsterState}'");

        //Toad
        if (monsterId == "toad")
        {
            if (monsterState == "start")
            {
                Debug.Log("Playing generic toad voice"); 
                AudioManager.Instance.PlayToadVoiceGeneric();
            }
            else if (monsterState == "neutral")
            {
                Debug.Log("Playing neutral toad voice"); 
                AudioManager.Instance.PlayToadVoiceAngry();
            }
            return;
        }

        //Alien
        if (monsterId == "alien")
        {
            if (monsterState == "start" || monsterState == "neutral")
            {
                Debug.Log("Playing generic alien voice"); 
                AudioManager.Instance.PlayAlienVoiceGeneric();
            }
            else if (monsterState == "satisfied")
            {
                Debug.Log("Playing satisfied alien voice"); 
                AudioManager.Instance.PlayAlienVoiceSatisfied();
            }
            else if (monsterState == "angry")
            {
                Debug.Log("Playing angry alien voice"); 
                AudioManager.Instance.PlayAlienVoiceAngry();
            }
            return;
        }

        //Unicorn
        if (monsterId == "unicorn")
        {
            if (monsterState == "start" || monsterState == "neutral")
            {
                Debug.Log("Playing generic unicorn voice"); 
                AudioManager.Instance.PlayUnicornVoiceGeneric();
            }
            else if (monsterState == "satisfied")
            {
                Debug.Log("Playing satisfied unicorn voice"); 
                AudioManager.Instance.PlayUnicornVoiceSatisfied();
            }
            else if (monsterState == "angry")
            {
                Debug.Log("Playing angry unicorn voice"); 
                AudioManager.Instance.PlayUnicornVoiceAngry();
            }
            return;
        }

        //Slime
        if (monsterId == "slime")
        {
            if (monsterState == "start" || monsterState == "neutral")
            {
                Debug.Log("Playing generic slime voice"); 
                AudioManager.Instance.PlayBlobVoiceGeneric();
            }
            else if (monsterState == "satisfied")
            {
                Debug.Log("Playing satisfied slime voice"); 
                AudioManager.Instance.PlayBlobVoiceSatisfied();
            }
            else if (monsterState == "angry")
            {
                Debug.Log("Playing angry slime voice"); 
                AudioManager.Instance.PlayBlobVoiceAngry();
            }
            return;
        }

        //Rocky (Golem)
        if (monsterId == "rocky")
        {
            if (monsterState == "start" || monsterState == "neutral")
            {
                Debug.Log("Playing generic rocky voice"); 
                AudioManager.Instance.PlayRockyVoiceGeneric();
            }
            else if (monsterState == "satisfied")
            {
                Debug.Log("Playing satisfied rocky voice"); 
                AudioManager.Instance.PlayRockyVoiceSatisfied();
            }
            else if (monsterState == "angry")
            {
                Debug.Log("Playing angry rocky voice"); 
                AudioManager.Instance.PlayRockyVoiceAngry();
            }
            return;
        }

        //Knight
        if (monsterId == "knight")
        {
            if (monsterState == "start" || monsterState == "neutral")
            {
                Debug.Log("Playing generic knight voice"); 
                AudioManager.Instance.PlayKnightVoiceGeneric();
            }
            else if (monsterState == "satisfied")
            {
                Debug.Log("Playing satisfied knight voice"); 
                AudioManager.Instance.PlayKnightVoiceSatisfied();
            }
            else if (monsterState == "angry")
            {
                Debug.Log("Playing angry knight voice"); 
                AudioManager.Instance.PlayKnightVoiceAngry();
            }
            return;
        }

        //Hamster
        if (monsterId == "hamster")
        {
            if (monsterState == "start" || monsterState == "neutral")
            {
                Debug.Log("Playing generic hamster voice"); 
                AudioManager.Instance.PlayHamsterVoiceGeneric();
            }
            else if (monsterState == "satisfied")
            {
                Debug.Log("Playing satisfied hamster voice"); 
                AudioManager.Instance.PlayHamsterVoiceSatisfied();
            }
            else if (monsterState == "angry")
            {
                Debug.Log("Playing angry hamster voice"); 
                AudioManager.Instance.PlayHamsterVoiceAngry();
            }
            return;
        }

        //Dragon
        if (monsterId == "dragon")
        {
            if (monsterState == "start" || monsterState == "neutral")
            {
                Debug.Log("Playing generic dragon voice"); 
                AudioManager.Instance.PlayDragonVoiceGeneric();
            }
            else if (monsterState == "satisfied")
            {
                Debug.Log("Playing satisfied dragon voice"); 
                AudioManager.Instance.PlayDragonVoiceSatisfied();
            }
            else if (monsterState == "angry")
            {
                Debug.Log("Playing angry dragon voice"); 
                AudioManager.Instance.PlayDragonVoiceAngry();
            }
            return;
        }

        Debug.LogWarning($"CharacterVoiceover in DialogueController: Unknown monster monsterId='{monsterId}' or state='{monsterState}'");
    }
/*
    public void StopCharacterVoiceoverAfterDialogue()
    {
        if (IsDialogueFinished == true)
        {
            AudioManager.Instance.StopVoice();
        }
    }
*/
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
		dialogueFightOfferAnalyticsSent = false;
        RefreshAll();
    }

    private bool CanShowBrewButton()
    {
        return monsterStateManager == null || monsterStateManager.MonsterState != "angry";
    }

    private void RefreshFightButton()
    {
        if (fightButtonObject == null)
            return;

		if (IsAssessDialogueController())
		{
			bool assessShow = monsterStateManager != null && monsterStateManager.MonsterState == "angry" && IsDialogueFinished;
			fightButtonObject.SetActive(assessShow);
			return;
		}

        bool dialogueFinished = IsDialogueFinished;
        bool angry = monsterStateManager != null && monsterStateManager.MonsterState == "angry";
		bool shouldShow = angry && dialogueFinished;
		if (shouldShow && !dialogueFightOfferAnalyticsSent)
		{
			dialogueFightOfferAnalyticsSent = true;
			GameAnalytics.RecordDialogueFightButtonBecameAvailable(GameAnalytics.DialogueFightSurfaceTakeOrder);
		}
		else if (!shouldShow)
		{
			dialogueFightOfferAnalyticsSent = false;
		}
        fightButtonObject.SetActive(shouldShow);
    }

	private bool IsAssessDialogueController()
	{
		return gameObject != null && gameObject.name == "AssessDialogueController";
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
        if (serveSpeech != null)
            serveSpeech.font = isArabic && arabicFontOverride != null ? arabicFontOverride : serveSpeechOriginalFont;
        if (monsterName != null)
            monsterName.font = isArabic && arabicFontOverride != null ? arabicFontOverride : monsterNameOriginalFont;
    }

    private void ShowCurrentLine()
    {
        ShowCurrentLine(GetActiveDialogue());
    }

    private TMP_Text GetActiveSpeechText()
    {
        if (serveSpeech != null && gameObject.name == "AssessDialogueController")
            return serveSpeech;

        return monsterSpeech;
    }

    private void ShowCurrentLine(List<string> lines)
    {
        TMP_Text activeSpeech = GetActiveSpeechText();

        if (lines.Count == 0)
        {
            if (typewriter != null) typewriter.SetInstant(string.Empty);
            else SetText(activeSpeech, string.Empty, preserveNumbers: true);

            brewButtonObject.SetActive(CanShowBrewButton());
            UpdateNextButtonState(lines);
            UpdateContinueButtonState(lines);
            return;
        }

        dialogueIndex = Mathf.Clamp(dialogueIndex, 0, lines.Count - 1);
        string rawLine = lines[dialogueIndex] ?? string.Empty;

        bool isArabic = RtlText.IsArabic();
        bool useTypewriter = typewriter != null && typewriter.enabled && !isArabic;
        if (useTypewriter) typewriter.TypeLine(rawLine);
        else SetText(activeSpeech, rawLine, preserveNumbers: true);

        brewButtonObject.SetActive(dialogueIndex >= lines.Count - 1 && CanShowBrewButton());
        UpdateNextButtonState(lines);
        UpdateContinueButtonState(lines);
        TryMakeTutorialToadAngry(lines);
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

    private void TryMakeTutorialToadAngry(List<string> activeDialogue)
    { 
        if (!IsTutorialToadNeutralServeDialogue())
            return;

        if (activeDialogue == null || activeDialogue.Count == 0 || dialogueIndex < activeDialogue.Count - 1)
            return;

        monsterStateManager.SetState("angry");
    }

    private bool IsTutorialToadNeutralServeDialogue()
    {
        if (gameObject.name != "AssessDialogueController")
            return false;

        if (monsterStateManager == null || currentMonsterManager == null || currentMonsterManager.Data == null || currentMonsterManager.CurrentEncounter == null)
            return false;

        if (monsterStateManager.MonsterState != "neutral")
            return false;

        if (currentMonsterManager.Data.id != "toad")
            return false;
        CharacterVoiceover();
        string dialogueKey = currentMonsterManager.CurrentEncounter.dialogue_key;
        return dialogueKey == "tutorial" || dialogueKey == "tutorial_1";
    }

    private bool UsesInkDialogue()
    {
        return currentMonsterManager != null &&
               currentMonsterManager.Data != null &&
               currentMonsterManager.Data.branching &&
               inkyDialogueController != null &&
               inkDialoguePresenter != null;
    }

    private bool TryStartInkDialogueForCurrentState()
    {
        if (!UsesInkDialogue())
            return false;

        TextAsset inkAsset = GetAssignedInkDialogueAsset() ?? LoadInkDialogueTextAssetFromResources();
        if (inkAsset == null)
        {
            string dialogueResourceId = GetInkDialogueResourceId();
            Debug.LogWarning($"[DialogueController] Branching monster requires compiled Ink JSON. No assigned TextAsset found and resource lookup failed for '{dialogueResourceId}'. Falling back to standard dialogue.", this);
            return false;
        }

        UpdateMonsterName();
        inkyDialogueController.SetInkJsonAsset(inkAsset);
        inkDialoguePresenter.SetDialogueTextTarget(GetActiveSpeechText());
        inkDialoguePresenter.SetSpeakerName(currentMonsterManager != null && currentMonsterManager.Data != null
            ? currentMonsterManager.Data.name
            : string.Empty);
        inkDialoguePresenter.StartKnotAndPresent(GetInkKnotNameForCurrentState());
        RefreshInkButtonState();
        TryMakeTutorialToadAngryInk();
        return true;
    }

    private TextAsset GetAssignedInkDialogueAsset()
    {
        bool isAssessState = monsterStateManager != null && monsterStateManager.MonsterState != "start";
        bool isAssessScreen = gameObject.name == "AssessDialogueController";

        if ((isAssessState || isAssessScreen) && serveInkJsonAsset != null)
            return serveInkJsonAsset;

        if (!isAssessState && orderInkJsonAsset != null)
            return orderInkJsonAsset;

        return null;
    }

    private TextAsset LoadInkDialogueTextAssetFromResources()
    {
        string dialogueResourceId = GetInkDialogueResourceId();
        return string.IsNullOrWhiteSpace(dialogueResourceId) ? null : Resources.Load<TextAsset>(dialogueResourceId);
    }

    private string GetInkDialogueResourceId()
    {
        if (currentMonsterManager == null || currentMonsterManager.Data == null)
            return string.Empty;

        MonsterData data = currentMonsterManager.Data;

        if (!string.IsNullOrWhiteSpace(data.inkDialogueId))
            return data.GetInkDialogueIdForCurrentLanguage();

        bool isAssessState = monsterStateManager != null && monsterStateManager.MonsterState != "start";
        if (isAssessState && !string.IsNullOrWhiteSpace(data.assessDialogueId))
            return data.GetAssessInkDialogueIdForCurrentLanguage();

        bool isAssessScreen = gameObject.name == "AssessDialogueController";
        if (isAssessScreen && !string.IsNullOrWhiteSpace(data.assessDialogueId))
            return data.GetAssessInkDialogueIdForCurrentLanguage();

        return data.dialogueId;
    }

    private string GetInkKnotNameForCurrentState()
    {
        string state = monsterStateManager != null ? monsterStateManager.MonsterState : "start";
        if (string.IsNullOrWhiteSpace(state))
            state = "start";

        string knotName = currentMonsterManager != null ? currentMonsterManager.GetCurrentInkKnot() : string.Empty;
        if (string.IsNullOrWhiteSpace(knotName))
            return state;

        return $"{knotName}.{state}";
    }

    private void RefreshInkButtonState()
    {
        SyncMonsterStateFromInkVariables();
        RefreshFightButton();
        UpdateNextButtonStateInk();
        UpdateContinueButtonStateInk();

        if (brewButtonObject != null)
            brewButtonObject.SetActive(inkyDialogueController != null &&
                                       inkyDialogueController.IsFinished &&
                                       CanShowBrewButton());
    }

    private void UpdateNextButtonStateInk()
    {
        if (nextButtonObject == null)
            return;

        bool showNext = inkyDialogueController != null &&
                        !inkyDialogueController.IsFinished &&
                        !inkyDialogueController.HasChoices &&
                        (inkDialoguePresenter == null || !inkDialoguePresenter.IsTyping);
        nextButtonObject.SetActive(showNext);
    }

    private void UpdateContinueButtonStateInk()
    {
        if (continueButtonObject == null)
            return;

        if (monsterStateManager != null && monsterStateManager.MonsterState == "angry")
        {
            continueButtonObject.SetActive(false);
            return;
        }

        bool showContinue = inkyDialogueController != null &&
                            inkyDialogueController.IsFinished &&
                            (inkDialoguePresenter == null || !inkDialoguePresenter.IsTyping);
        continueButtonObject.SetActive(showContinue);

        if (!showContinue)
            return;

        bool isLastMonsterOfDay = !currentMonsterManager.HasNextMonsterInCurrentLevel();
        string key = isLastMonsterOfDay ? "next_day_button" : "continue_button";

        var localizedText = continueButtonObject.GetComponentInChildren<LocalizedTMPText>(true);
        if (localizedText != null)
            localizedText.SetKey(key);
    }

    private void TryMakeTutorialToadAngryInk()
    {
        if (!UsesInkDialogue())
            return;

        if (!IsTutorialToadNeutralServeDialogue())
            return;

        if (inkyDialogueController == null || !inkyDialogueController.IsFinished)
            return;

        monsterStateManager.SetState("angry");
    }

    private void SyncMonsterStateFromInkVariables()
{
    if (monsterStateManager == null || inkyDialogueController == null)
        return;

    if (inkyDialogueController.TryGetBoolVariable("is_angry", out bool isAngry) && isAngry)
        monsterStateManager.SetState("angry");

    if (alienIDCanvas != null)
    {
        bool showLicense = false;

        if (inkyDialogueController.TryGetBoolVariable("show_license", out bool inkShowLicense))
            showLicense = inkShowLicense;

        alienIDCanvas.SetActive(showLicense);
    }

    RefreshFightButton();
}
}

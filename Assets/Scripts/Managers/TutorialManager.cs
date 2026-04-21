using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Magetender.Data;

[System.Serializable]
public class TutorialStep
{
    public string key;
    public GameObject panel;
    public bool pauseGame = true;
}

[System.Serializable]
public class StepGroup
{
    public GameObject trigger;
    public int[] steps;

    [Header("Optional Pointer")]
    public bool showPointer;
    public Vector2 pointerPosition;
}

public class TutorialManager : MonoBehaviour
{
    [Header("Steps")]
    [SerializeField] private TutorialStep[] steps;

    [Header("Groups")]
    [SerializeField] private StepGroup[] groups;

    [Header("GLOBAL BLACKLIST")]
    [SerializeField] private GameObject[] InactiveItems;

    [Header("UI Refs")]
    [SerializeField] private GameObject ShopButton;
    [SerializeField] private GameObject GreetingText;
    [SerializeField] private GameObject DayText;
    [SerializeField] private GameObject tutorialTimerCanvas;
    [SerializeField] private TextMeshProUGUI NextButton;
    [SerializeField] private Button tutorialBaseNextButton;
    [SerializeField] private CanvasGroup tutorialBaseNextButtonCanvasGroup;

    [Header("Tutorial State Refs")]
    [SerializeField] private MixManager mixManager;

    [Header("Pointer (UI Image)")]
    [SerializeField] private Image pointerPrefab;
    [SerializeField] private RectTransform pointerParent;

    [Header("Tutorial Monster Layout")]
    [SerializeField] private RectTransform tutorialOrderMonsterRect;
    [SerializeField] private Vector3 tutorialOrderMonsterScale = Vector3.one;
    [SerializeField] private RectTransform tutorialServeMonsterRect;
    [SerializeField] private Vector3 tutorialServeMonsterScale = Vector3.one;

    [Header("Tutorial Monster Sprites")]
    [SerializeField] private Image tutorialOrderMonsterImage;
    [SerializeField] private Sprite tutorialOrderMonsterSprite;
    [SerializeField] private Image tutorialServeMonsterImage;
    [SerializeField] private Sprite tutorialServeMonsterSprite;

    private Image activePointer;

    private static Dictionary<string, string> uiDict;

    private bool[] groupTriggered;

    private int activeGroupIndex = -1;
    private int activeStepInGroup = -1;

    private bool panelActive = false;
    private bool tutorialMonsterLayoutApplied;
    private bool activeStepPausesGame = true;

    private bool ShouldRestrictBaseNextButton
    {
        get
        {
            return activeGroupIndex == 1 || (activeGroupIndex == -1 && groupTriggered != null && groupTriggered.Length > 1 && groupTriggered[1] && !GroupsCompleted());
        }
    }

    void Awake()
    {
        if (ShouldDisableImmediately())
            gameObject.SetActive(false);
    }

    void Start()
    {
        if (ShouldDisableTutorialManager())
            return;

        LoadUI();
        Setup();

        if (mixManager == null)
            mixManager = FindFirstObjectByType<MixManager>();

        DisableTutorialTimer();
        groupTriggered = new bool[groups.Length];
        DeactivateItems(false);
        ApplyTutorialMonsterSpritesOnce();
        ApplyTutorialMonsterLayoutOnce();
    }

    bool ShouldDisableTutorialManager()
    {
        if (ShouldDisableImmediately())
        {
            gameObject.SetActive(false);
            return true;
        }

        CurrentMonster currentMonster = CurrentMonster.Instance;
        if (currentMonster == null)
            return false;

        if (currentMonster.GetCurrentLevelId() == "tutorial")
            return false;

        MarkTutorialCompleted();
        gameObject.SetActive(false);
        return true;
    }

    void MarkTutorialCompleted()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
            return;

        gameManager.MarkTutorialCompleted();
    }

    void LoadUI()
    {
        string path = LanguageManager.Instance.GetUIStringsResourcePath();
        TextAsset jsonFile = Resources.Load<TextAsset>(path);
        UIStringsData data = JsonUtility.FromJson<UIStringsData>(jsonFile.text);

        uiDict = new Dictionary<string, string>();
        foreach (var entry in data.entries)
        {
            uiDict[entry.key] = entry.value;
        }
    }

    string GetUI(string key)
    {
        return uiDict.TryGetValue(key, out var value) ? value : key;
    }

    void Setup()
    {
        ShopButton.SetActive(false);
        DayText.SetActive(false);
        GreetingText.SetActive(true);

        NextButton.text = GetUI("begin_button");

        HideAllPanels();
    }

    void HideAllPanels()
    {
        foreach (var step in steps)
        {
            if (step.panel != null)
                step.panel.SetActive(false);
        }
    }

    void DeactivateItems(bool enabled)
    {
        if (InactiveItems == null) return;

        foreach (var obj in InactiveItems)
        {
            if (obj == null) continue;

            var graphics = obj.GetComponentsInChildren<Graphic>(true);
            foreach (var g in graphics)
            {
                g.raycastTarget = enabled;
            }
        }
    }

    void StartGroup(int groupIndex)
    {
        Debug.Log("starting group: " + groupIndex);

        if (activePointer != null)
        {
            Destroy(activePointer.gameObject);
            activePointer = null;
        }

        activeGroupIndex = groupIndex;
        activeStepInGroup = 0;

        ShowCurrentStep(); // 🔥 direct call, no delay
    }

    void ShowCurrentStep()
    {
        StepGroup g = groups[activeGroupIndex];

        if (g.steps == null || g.steps.Length == 0)
        {
            EndGroup();
            return;
        }

        if (activeStepInGroup >= g.steps.Length)
        {
            EndGroup();
            return;
        }

        int stepIndex = g.steps[activeStepInGroup];

        if (stepIndex < 0 || stepIndex >= steps.Length)
        {
            activeStepInGroup++;
            ShowCurrentStep();
            return;
        }

        TutorialStep step = steps[stepIndex];

        HideAllPanels();

        if (step.panel != null)
        {
            step.panel.SetActive(true);

            TextMeshProUGUI txt = step.panel.GetComponentInChildren<TextMeshProUGUI>(true);
            if (txt != null)
                txt.text = GetUI(step.key);
        }

        panelActive = true;
        activeStepPausesGame = step.pauseGame;
        Time.timeScale = activeStepPausesGame ? 0f : 1f;
    }

    void AdvanceStep()
    {
        activeStepInGroup++;
        ShowCurrentStep(); // 🔥 immediate
    }

    void EndGroup()
    {
        Debug.Log("Ending group: " + activeGroupIndex);

        StepGroup g = groups[activeGroupIndex];

        HideAllPanels();
        panelActive = false;
        activeStepPausesGame = true;
        Time.timeScale = 1f;

        if (g.showPointer)
        {
            ShowPointer(g.pointerPosition);
        }

        activeGroupIndex = -1;
        activeStepInGroup = -1;

        if (GroupsCompleted())
        {
            Debug.Log("Tutorial complete, re-enabling UI");
            DeactivateItems(true);
        }
    }

    bool GroupsCompleted()
    {
        foreach (bool triggered in groupTriggered)
        {
            if (!triggered) return false;
        }
        return true;
    }

    void ShowPointer(Vector2 position)
    {
        if (pointerPrefab == null || pointerParent == null) return;

        if (activePointer != null)
            Destroy(activePointer.gameObject);

        activePointer = Instantiate(pointerPrefab, pointerParent);

        RectTransform rect = activePointer.rectTransform;
        rect.anchoredPosition = position;

        activePointer.gameObject.SetActive(true);

        StartCoroutine(AnimatePointer(rect));
    }

	IEnumerator AnimatePointer(RectTransform rect)
	{
		Vector3 baseScale = Vector3.one;

		while (activePointer != null && rect != null)
		{
			float t = 0f;

			while (t < 1f)
			{
				if (activePointer == null || rect == null)
				{
					yield break;
				}

				t += Time.unscaledDeltaTime * 2f;
				rect.localScale = Vector3.Lerp(baseScale, baseScale * 1.2f, t);
				yield return null;
			}

			t = 0f;
			while (t < 1f)
			{
				if (activePointer == null || rect == null)
				{
					yield break;
				}

				t += Time.unscaledDeltaTime * 2f;
				rect.localScale = Vector3.Lerp(baseScale * 1.2f, baseScale, t);
				yield return null;
			}
		}
	}

    void Update()
    {
        if (TryCompleteAndDisableTutorial())
            return;

        DisableTutorialTimer();

        if (activePointer != null && Input.GetMouseButtonDown(0))
        {
            Destroy(activePointer.gameObject);
            activePointer = null;
        }

        UpdateTutorialBaseNextButton();

        if (activeGroupIndex == -1 && !panelActive)
        {
            for (int i = 0; i < groups.Length; i++)
            {
                if (!groupTriggered[i] && groups[i].trigger != null && groups[i].trigger.activeSelf)
                {
                    groupTriggered[i] = true;
                    StartGroup(i);
                    break;
                }
            }
        }

        bool advancePressed = Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
        if (panelActive && advancePressed && activeGroupIndex >= 0)
        {
            HideAllPanels();
            panelActive = false;
            activeStepPausesGame = true;
            Time.timeScale = 1f;

            AdvanceStep();
        }
    }

    bool TryCompleteAndDisableTutorial()
    {
        if (ShouldDisableImmediately())
        {
            gameObject.SetActive(false);
            return true;
        }

        CurrentMonster currentMonster = CurrentMonster.Instance;
        if (currentMonster == null)
            return false;

        if (currentMonster.GetCurrentLevelId() == "tutorial")
            return false;

        MarkTutorialCompleted();
        gameObject.SetActive(false);
        return true;
    }

    bool IsTutorialCompleted()
    {
        GameManager gameManager = GameManager.Instance;
        SaveData saveData = SaveSystem.LoadGame();
        return (gameManager != null && gameManager.TutorialCompleted) || (saveData != null && saveData.tutorialCompleted);
    }

    bool ShouldDisableImmediately()
    {
        if (IsTutorialCompleted())
            return true;

        CurrentMonster currentMonster = CurrentMonster.Instance;
        if (currentMonster == null)
            return false;

        return currentMonster.IsPlannedVisitNextDay();
    }

    void UpdateTutorialBaseNextButton()
    {
        if (tutorialBaseNextButton == null)
            return;

        if (!ShouldRestrictBaseNextButton)
        {
            tutorialBaseNextButton.interactable = true;
            if (tutorialBaseNextButtonCanvasGroup != null)
                tutorialBaseNextButtonCanvasGroup.alpha = 1f;
            return;
        }

        bool bottleIsFull = mixManager != null && mixManager.FillLevel >= 1f;
        tutorialBaseNextButton.interactable = bottleIsFull;

        if (tutorialBaseNextButtonCanvasGroup != null)
            tutorialBaseNextButtonCanvasGroup.alpha = bottleIsFull ? 1f : 0.5f;
    }

    void DisableTutorialTimer()
    {
        if (tutorialTimerCanvas != null && tutorialTimerCanvas.activeSelf)
            tutorialTimerCanvas.SetActive(false);
    }

    void ApplyTutorialMonsterLayoutOnce()
    {
        if (tutorialMonsterLayoutApplied)
            return;

        ApplyTutorialMonsterRectLayout(tutorialOrderMonsterRect, tutorialOrderMonsterScale);
        ApplyTutorialMonsterRectLayout(tutorialServeMonsterRect, tutorialServeMonsterScale);

        tutorialMonsterLayoutApplied = true;
    }

    void ApplyTutorialMonsterRectLayout(RectTransform targetRect, Vector3 targetScale)
    {
        if (targetRect == null)
            return;

        targetRect.localScale = targetScale;

        Transform parent = targetRect.parent;
        if (parent == null)
            return;

        int siblingIndex = targetRect.GetSiblingIndex();
        int lastIndex = parent.childCount - 1;
        targetRect.SetSiblingIndex(Mathf.Min(siblingIndex + 1, lastIndex));
    }

    void ApplyTutorialMonsterSpritesOnce()
    {
        if (tutorialOrderMonsterImage != null && tutorialOrderMonsterSprite != null)
            tutorialOrderMonsterImage.sprite = tutorialOrderMonsterSprite;

        if (tutorialServeMonsterImage != null && tutorialServeMonsterSprite != null)
            tutorialServeMonsterImage.sprite = tutorialServeMonsterSprite;
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoodGraphUI : MonoBehaviour
{
    [Header("ui refs")]
    [SerializeField] private RectTransform graphRect;
    [SerializeField] private RectTransform markerRect;
    [SerializeField] private Image moodMarkerIcon;
    [SerializeField] private MoodGraphMonsterIconEntry[] monsterMarkerIcons;
    [SerializeField] private Sprite fallbackMoodMarkerSprite;
    [SerializeField] private RectTransform goalMarkerRect;
    [SerializeField] private TMP_Text nameText;

    [Header("data source")]
    [SerializeField] private CurrentMonster currentMonster;

    [Header("optional: assess behavior")]
    public GameObject assessPanel;
    [SerializeField] private GameObject ingredientsPanel;
    [SerializeField] private ScoreManager scoreManager;

    [Header("graph range")]
    [SerializeField] private float minX = -1f;
    [SerializeField] private float maxX = 1f;
    [SerializeField] private float minY = -1f;
    [SerializeField] private float maxY = 1f;

    [Header("tuning")]
    [SerializeField] private float updateEpsilon = 0.001f;

    private string lastMonsterId = "";
    private float lastX = float.NaN;
    private float lastY = float.NaN;
    private float lastGoalX = float.NaN;
    private float lastGoalY = float.NaN;

    private void Awake()
    {
        currentMonster = CurrentMonster.Instance;
        if (moodMarkerIcon == null && markerRect != null)
            moodMarkerIcon = markerRect.GetComponent<Image>();
    }

    private void OnEnable()
    {
        currentMonster = CurrentMonster.Instance;

        if (currentMonster != null)
            currentMonster.OnMonsterChanged += HandleMonsterChanged;
    }

    private void OnDisable()
    {
        if (currentMonster != null)
            currentMonster.OnMonsterChanged -= HandleMonsterChanged;
    }

    private void Start()
    {
        RefreshAll(true);
    }

    private void Update()
    {
        RefreshAll(false);
    }

    private void HandleMonsterChanged(string _)
    {
        ForceRefresh();
    }

    public void ForceRefresh()
    {
        lastMonsterId = "";
        lastX = float.NaN;
        lastY = float.NaN;
        lastGoalX = float.NaN;
        lastGoalY = float.NaN;

        RefreshAll(true);
    }

    private void RefreshAll(bool force)
    {
        currentMonster = CurrentMonster.Instance;

        MonsterData monster = currentMonster != null ? currentMonster.Data : null;
        if (monster == null)
        {
            SetMarkerVisible(markerRect, false);
            SetMarkerVisible(goalMarkerRect, false);
            if (nameText != null) nameText.text = "";
            return;
        }

        bool monsterChanged = force || monster.id != lastMonsterId;
        if (monsterChanged)
        {
            lastMonsterId = monster.id;

            if (nameText != null)
                nameText.text = monster.name;

            ApplyMoodMarkerIcon(monster.id);

            lastGoalX = float.NaN;
            lastGoalY = float.NaN;
        }

        bool isAssessActive = assessPanel != null && assessPanel.activeInHierarchy;
        bool isIngredientsActive = IsIngredientsPanelActive();

        ScorePair start = currentMonster.GetStartingScore();
        float x = start != null ? start.x : float.NaN;
        float y = start != null ? start.y : float.NaN;
        bool hasMood = start != null;

        if (isAssessActive || isIngredientsActive)
        {
            if (scoreManager == null)
                scoreManager = FindFirstObjectByType<ScoreManager>();
            if (scoreManager != null)
            {
                x = scoreManager.CurrMoodBoardX;
                y = scoreManager.CurrMoodBoardY;
                hasMood = true;
            }
        }

        if (hasMood)
        {
            bool moodChanged =
                force ||
                float.IsNaN(lastX) ||
                Mathf.Abs(x - lastX) > updateEpsilon ||
                Mathf.Abs(y - lastY) > updateEpsilon;

            if (moodChanged)
            {
                lastX = x;
                lastY = y;

                SetMarkerVisible(markerRect, true);
                UpdateMarkerPosition(markerRect, new Vector2(x, y));
            }
        }
        else
        {
            SetMarkerVisible(markerRect, false);
        }

        ScorePair goal = currentMonster.GetGoalScore();
        if (goal == null)
        {
            SetMarkerVisible(goalMarkerRect, false);
            return;
        }

        float gx = goal.x;
        float gy = goal.y;

        bool goalChanged =
            force ||
            float.IsNaN(lastGoalX) ||
            Mathf.Abs(gx - lastGoalX) > updateEpsilon ||
            Mathf.Abs(gy - lastGoalY) > updateEpsilon;

        if (goalChanged)
        {
            lastGoalX = gx;
            lastGoalY = gy;

            SetMarkerVisible(goalMarkerRect, true);
            UpdateMarkerPosition(goalMarkerRect, new Vector2(gx, gy));
        }
    }

    private void UpdateMarkerPosition(RectTransform rect, Vector2 mood)
    {
        if (graphRect == null || rect == null) return;

        float nx = Mathf.InverseLerp(minX, maxX, mood.x);
        float ny = Mathf.InverseLerp(minY, maxY, mood.y);

        nx = Mathf.Clamp01(nx);
        ny = Mathf.Clamp01(ny);

        Rect r = graphRect.rect;

        float localX = Mathf.Lerp(r.xMin, r.xMax, nx);
        float localY = Mathf.Lerp(r.yMin, r.yMax, ny);

        Vector3 worldPos = graphRect.TransformPoint(new Vector3(localX, localY, 0f));

        RectTransform parentRect = rect.parent as RectTransform;
        if (parentRect == null)
        {
            rect.position = worldPos;
            return;
        }

        Vector2 anchored;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            RectTransformUtility.WorldToScreenPoint(null, worldPos),
            null,
            out anchored
        );

        rect.anchoredPosition = anchored;

        ClampMarkerInsideGraph(rect);
    }

    private void ClampMarkerInsideGraph(RectTransform rect)
    {
        if (graphRect == null || rect == null) return;

        Rect gr = graphRect.rect;
        Vector2 p = rect.anchoredPosition;

        float halfW = rect.rect.width * 0.5f;
        float halfH = rect.rect.height * 0.5f;

        p.x = Mathf.Clamp(p.x, gr.xMin + halfW, gr.xMax - halfW);
        p.y = Mathf.Clamp(p.y, gr.yMin + halfH, gr.yMax - halfH);

        rect.anchoredPosition = p;
    }

    private void SetMarkerVisible(RectTransform rect, bool visible)
    {
        if (rect != null)
            rect.gameObject.SetActive(visible);
    }

    private void ApplyMoodMarkerIcon(string monsterId)
    {
        if (moodMarkerIcon == null)
            return;

        Sprite icon = ResolveMarkerIcon(monsterId);
        if (icon != null)
            moodMarkerIcon.sprite = icon;
        else if (fallbackMoodMarkerSprite != null)
            moodMarkerIcon.sprite = fallbackMoodMarkerSprite;
    }

    private Sprite ResolveMarkerIcon(string monsterId)
    {
        if (string.IsNullOrEmpty(monsterId) || monsterMarkerIcons == null)
            return null;

        for (int i = 0; i < monsterMarkerIcons.Length; i++)
        {
            MoodGraphMonsterIconEntry e = monsterMarkerIcons[i];
            if (e == null || string.IsNullOrEmpty(e.monsterId))
                continue;
            if (string.Equals(e.monsterId, monsterId, StringComparison.OrdinalIgnoreCase))
                return e.icon;
        }

        return null;
    }

    private bool IsIngredientsPanelActive()
    {
        if (ingredientsPanel == null)
        {
            var ingredientsController = FindFirstObjectByType<IngredientsController>();
            if (ingredientsController != null)
                ingredientsPanel = ingredientsController.CurrentScreen;
        }

        return ingredientsPanel != null && ingredientsPanel.activeInHierarchy;
    }
}

[Serializable]
public class MoodGraphMonsterIconEntry
{
    public string monsterId;
    public Sprite icon;
}

using TMPro;
using UnityEngine;

public class MoodGraphUI : MonoBehaviour
{
    [Header("scene refs")]
    [SerializeField] private ScoreManager scoreManager;

    [Header("ui refs")]
    [SerializeField] private RectTransform graphRect;
    [SerializeField] private RectTransform markerRect;
    [SerializeField] private RectTransform goalMarkerRect;
    [SerializeField] private TMP_Text nameText;

    [Header("data source")]
    [SerializeField] private CurrentMonster currentMonster;

    [Header("behavior")]
    [SerializeField] private bool useStartingScoreFallbackForCurrentMarker = true;

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
        if (scoreManager == null)
            scoreManager = FindFirstObjectByType<ScoreManager>();

        if (currentMonster == null)
            currentMonster = CurrentMonster.Instance;
    }

    private void OnEnable()
    {
        if (currentMonster == null)
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
        //force update of both markers + name when encounter changes
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
        //resolve current monster source
        if (currentMonster == null)
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

            
            lastGoalX = float.NaN;
            lastGoalY = float.NaN;
        }

        bool hasLiveMood = scoreManager != null;
        float x = hasLiveMood ? scoreManager.CurrMoodBoardX : 0f;
        float y = hasLiveMood ? scoreManager.CurrMoodBoardY : 0f;

        if (!hasLiveMood && useStartingScoreFallbackForCurrentMarker && currentMonster != null)
        {
            ScorePair start = currentMonster.GetStartingScore();
            if (start != null)
            {
                x = start.x;
                y = start.y;
                hasLiveMood = true;
            }
        }

        if (hasLiveMood)
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

        ScorePair goal = currentMonster != null ? currentMonster.GetGoalScore() : null;
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
}
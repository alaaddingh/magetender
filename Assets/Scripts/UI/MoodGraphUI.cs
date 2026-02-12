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

    private void Awake()
    {
        if (scoreManager == null)
            scoreManager = FindFirstObjectByType<ScoreManager>();
    }

    private void Start()
    {
        RefreshAll(true);
    }

    private void Update()
    {
        RefreshAll(false);
    }

    private void RefreshAll(bool force)
    {
        if (scoreManager == null) return;

        MonsterData monster = scoreManager.GetCurrentMonster();
        if (monster == null || monster.starting_score == null || monster.goal_score == null)
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

            Vector2 goal = new Vector2(monster.goal_score.x, monster.goal_score.y);
            SetMarkerVisible(goalMarkerRect, true);
            UpdateMarkerPosition(goalMarkerRect, goal);
        }

        float x = scoreManager.CurrMoodBoardX;
        float y = scoreManager.CurrMoodBoardY;

        bool moodChanged =
            force ||
            float.IsNaN(lastX) ||
            Mathf.Abs(x - lastX) > updateEpsilon ||
            Mathf.Abs(y - lastY) > updateEpsilon;

        if (!moodChanged) return;

        lastX = x;
        lastY = y;

        SetMarkerVisible(markerRect, true);
        UpdateMarkerPosition(markerRect, new Vector2(x, y));
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
        Debug.Log($"y={mood.y} -> ny={ny} (minY={minY}, maxY={maxY}) plotH={graphRect.rect.height}");

    }

    // Helper function ensures marker stays in bounds
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
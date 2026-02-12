using TMPro;
using UnityEngine;

public class MoodGraphUI : MonoBehaviour
{
    [Header("scene refs")]
    [SerializeField] private ScoreManager scoreManager;

    [Header("ui refs")]
    [SerializeField] private RectTransform graphRect;
    [SerializeField] private RectTransform markerRect;
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
        if (monster == null || monster.starting_score == null) {
            SetMarkerVisible(false);
            if (nameText != null) nameText.text = "";
            return;
        }

        if (force || monster.id != lastMonsterId)
        {
            lastMonsterId = monster.id;
            if (nameText != null) nameText.text = monster.name;
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

        SetMarkerVisible(true);
        UpdateMarkerPosition(new Vector2(x, y));
    }

    private void UpdateMarkerPosition(Vector2 mood)
    {
        if (graphRect == null || markerRect == null) return;

        float nx = Mathf.InverseLerp(minX, maxX, mood.x);
        float ny = Mathf.InverseLerp(minY, maxY, mood.y);

        Rect r = graphRect.rect;
        float localX = (nx - 0.5f) * r.width;
        float localY = (ny - 0.5f) * r.height;

        //marker should be a child of graphRect
        markerRect.anchoredPosition = new Vector2(localX, localY);
    }

    private void SetMarkerVisible(bool visible)
    {
        if (markerRect != null)
            markerRect.gameObject.SetActive(visible);
    }
}
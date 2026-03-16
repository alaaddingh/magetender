using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoodMarkerTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [Header("tooltip ui")]
    [SerializeField] private GameObject tooltipRoot;
    [SerializeField] private TMP_Text tooltipText;

    [Header("label key (from UIStrings; if empty uses mood_tooltip_label)")]
    [SerializeField] private string labelOverride = "";

    [Header("positioning")]
    [SerializeField] private Vector2 screenOffset = new Vector2(8f, -8f);
    [SerializeField] private float screenPadding = 6f;

    private RectTransform tooltipRect;
    private RectTransform tooltipParentRect;
    private Canvas rootCanvas;

    private void Awake()
    {
        if (tooltipRoot != null)
        {
            tooltipRect = tooltipRoot.GetComponent<RectTransform>();
            if (tooltipRect != null)
                tooltipParentRect = tooltipRect.parent as RectTransform;

            rootCanvas = tooltipRoot.GetComponentInParent<Canvas>();
        }

        Hide();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Show();
        FollowPointer(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Hide();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        FollowPointer(eventData);
    }

    private void Show()
    {
        if (tooltipRoot == null || tooltipText == null) return;

        string key = !string.IsNullOrEmpty(labelOverride) ? labelOverride : "mood_tooltip_label";
        tooltipText.text = L.Get(key);
        tooltipRoot.SetActive(true);
    }

    private void Hide()
    {
        if (tooltipRoot == null) return;
        tooltipRoot.SetActive(false);
    }

    private void FollowPointer(PointerEventData eventData)
    {
        if (tooltipRoot == null || tooltipRect == null || tooltipParentRect == null || rootCanvas == null) return;
        if (!tooltipRoot.activeSelf) return;

        Camera cam = eventData.pressEventCamera != null
            ? eventData.pressEventCamera
            : (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera);

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tooltipParentRect,
            eventData.position,
            cam,
            out localPoint
        );

        float w = tooltipRect.rect.width;
        float h = tooltipRect.rect.height;
        bool leftSideOfScreen = eventData.position.x < Screen.width * 0.5f;
        float offsetX = leftSideOfScreen ? screenOffset.x : -(w + Mathf.Abs(screenOffset.x));
        Vector2 target = localPoint + new Vector2(offsetX, screenOffset.y);

        //clamp inside tooltip parent so it doesn't go offscreen

        Rect pr = tooltipParentRect.rect;

        float minX = pr.xMin + screenPadding;
        float maxX = pr.xMax - w - screenPadding;
        float minY = pr.yMin + h + screenPadding;
        float maxY = pr.yMax - screenPadding;

        target.x = Mathf.Clamp(target.x, minX, maxX);
        target.y = Mathf.Clamp(target.y, minY, maxY);

        tooltipRect.anchoredPosition = target;
    }
}
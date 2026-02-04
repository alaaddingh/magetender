using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;


/* purpose of file: 
    1) gets chosen glass from mixmanager then displays
    2) when user clicks on base, applies a tint to the glass
    3) write selected base to mixmanager
    4) sets "next" button to visible when neccesary
    */
public class BaseController : MonoBehaviour
{
    [Header("Target bottle shown on Base screen")]
    public Image BaseBottle;

    [Header("Source bottle UI images (existing in scene)")]
    public Image smallBottleUI;
    public Image mediumBottleUI;
    public Image largeBottleUI;

    [Header("Base colors")]
    public Color BloodColor = new Color(0.7f, 0.1f, 0.1f, 1f); // red
    public Color HolyWaterColor = new Color(0.7f, 0.9f, 1f, 1f); // blue
    public Color SpiritsColor = new  Color(0.85f, 0.95f, 0.8f, 1f); // ghostly green
    public Color MoonShineColor = new Color(0.9f, 0.7f, 0.9f, 1f); // purple/pink

    [Header("Manager")]
    [SerializeField] private MixManager mixManager;

    public GameObject nextButton;
    public GameObject CurrentScreen;
    public GameObject NextScreen;

    private bool isDragging = false;
    private Vector2 dragStartMousePos;
    private Vector2 dragStartBottlePos;

    private void Awake()
    {
        if (mixManager == null)
            mixManager = FindFirstObjectByType<MixManager>();
    }

    private void OnEnable()
    {
        ApplySelectedBottleToBaseBottle();
        ApplySavedBaseTint();

        nextButton.SetActive(false);
    }

    private void Update()
    {
        if (BaseBottle == null) return;
        if (Input.GetMouseButtonDown(0))
        {
            if (IsClickingOnBaseBottle())
            {
                isDragging = true;
                dragStartMousePos = Input.mousePosition;
                dragStartBottlePos = BaseBottle.rectTransform.anchoredPosition;
            }
            // Otherwise check for base jar clicks
            else if (!isDragging)
            {
                Image hoveredBase = GetHoveredBase();
                if (hoveredBase != null)
                {
                    string key = hoveredBase.gameObject.name.ToLower();

                    if (key.Contains("blood"))
                    {
                        SetBase("blood", BloodColor);
                    }
                    else if (key.Contains("holy"))
                    {
                        SetBase("holywater", HolyWaterColor);
                    }
                    else if (key.Contains("spirits"))
                    {
                        SetBase("spirits", SpiritsColor);
                    }
                    else if (key.Contains("moon") || key.Contains("shine"))
                    {
                        SetBase("moonshine", MoonShineColor);
                    }
                }
            }
        }

        // Handle dragging
        if (isDragging && Input.GetMouseButton(0))
        {
            Canvas canvas = BaseBottle.canvas;
            if (canvas != null)
            {
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                
                // Convert mouse position to canvas local space
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    Input.mousePosition,
                    canvas.worldCamera,
                    out Vector2 currentMousePos);

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    dragStartMousePos,
                    canvas.worldCamera,
                    out Vector2 startMousePos);

                // Calculate delta and update position
                float deltaX = currentMousePos.x - startMousePos.x;
                float newX = dragStartBottlePos.x + deltaX;
                
                // Update position: new x, keep y at -200
                BaseBottle.rectTransform.anchoredPosition = new Vector2(newX, -200f);
            }
        }

        // Handle mouse up
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    private void ApplySelectedBottleToBaseBottle()
    {
        if (mixManager == null || BaseBottle == null) return;

        switch (mixManager.SelectedBottle)
        {
            case "small":
                UIImgUtil.CopyAppearance(smallBottleUI, BaseBottle);
                break;
            case "medium":
                UIImgUtil.CopyAppearance(mediumBottleUI, BaseBottle);
                break;
            case "large":
                UIImgUtil.CopyAppearance(largeBottleUI, BaseBottle);
                break;
            default:
                break;
        }
    }

    private void SetBase(string baseKey, Color tint)
    {
        mixManager.SetBase(baseKey);
        BaseBottle.color = tint;

        nextButton.SetActive(true);

    }

    private void ApplySavedBaseTint()
    {
        if (mixManager == null || BaseBottle == null) return;

        switch (mixManager.SelectedBase)
        {
            case "blood":
                BaseBottle.color = BloodColor;
                break;
            case "holywater":
                BaseBottle.color = HolyWaterColor;
                break;
            case "spirits":
                BaseBottle.color = SpiritsColor;
                break;
            case "moonshine":
                BaseBottle.color = MoonShineColor;
                break;
        }
    }

    public void NextPressed()
    {

        if (CurrentScreen != null) CurrentScreen.SetActive(false);
        if (NextScreen != null) NextScreen.SetActive(true);
    }

    private Image GetHoveredBase()
    {
        if (EventSystem.current == null) return null;

        var pointer = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);

        foreach (var r in results)
        {
            if (!r.gameObject.CompareTag("Base")) continue;

            var img = r.gameObject.GetComponent<Image>();
            if (img != null) return img;
        }

        return null;
    }

    private bool IsClickingOnBaseBottle()
    {
        if (BaseBottle == null || EventSystem.current == null) return false;

        PointerEventData pointer = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);

        // Check if the top-most hit is the BaseBottle
        foreach (var result in results)
        {
            if (result.gameObject == BaseBottle.gameObject)
            {
                return true;
            }
            // If we hit something else that blocks, stop checking
            if (result.gameObject.GetComponent<Image>() != null)
            {
                break;
            }
        }
        return false;
    }
}

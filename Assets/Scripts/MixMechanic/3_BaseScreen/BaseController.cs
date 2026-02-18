using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
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

    [Header("Manager")]
    [SerializeField] private MixManager mixManager;

    [Header("Pouring Settings")]
    public float dropSpawnInterval = 0.01f;
    public float dropSize = 15f;
    public float dropSpawnOffsetY = 10f;

    public GameObject nextButton;
    public GameObject CurrentScreen;
    public GameObject NextScreen;
    [Tooltip("Panel to show when Back is pressed (e.g. Pick Glass screen).")]
    public GameObject PreviousScreen;

    private bool isDragging = false;
    private Vector2 dragStartMousePos;
    private Vector2 dragStartBottlePos;
    
    private Image currentPouringBase = null;
    private Coroutine pouringCoroutine = null;
    private Canvas canvas;
    private int skipInputFrames;
    private bool allowInput; // set true after short delay when panel enables so Next button release doesn't block drag/pour

    [Header("Fill Settings")]
    public float fillAmountPerDrop = 0.01f;
    public float maxFillLevel = 1.0f;
    [Range(0f, 1f)]
    public float fillWidthMultiplier = 0.8f;
    [Range(0f, 1f)]
    public float fillHeightMultiplier = 0.5f;
    [Range(0f, 1f)]
    public float fillAlpha = 0.7f;

    [Header("Base screen bottle position (per size so they align)")]
    public Vector2 baseBottlePosSmall = new Vector2(0f, -120f);
    public Vector2 baseBottlePosMedium = new Vector2(0f, -160f);
    public Vector2 baseBottlePosLarge = new Vector2(0f, -200f);

    [Header("Trash drain")]
    [Tooltip("When the bottle is over the trash, fill drains at this rate per second.")]
    public float drainRatePerSecond = 0.3f;
    public RectTransform trashRect;

    private float currentFillLevel = 0f;
    private Dictionary<string, float> baseAmounts = new Dictionary<string, float>(); // base name -> amount (0.0 to 1.0)
    private Image fillImage;

    private void Awake()
    {
        if (mixManager == null)
            mixManager = FindFirstObjectByType<MixManager>();
        
        if (BaseBottle != null)
            canvas = BaseBottle.canvas;
    }

    private void OnEnable()
    {
        if (BaseBottle != null)
            canvas = BaseBottle.canvas;
        isDragging = false;
        currentPouringBase = null;
        allowInput = false;
        if (pouringCoroutine != null)
        {
            StopCoroutine(pouringCoroutine);
            pouringCoroutine = null;
        }
        skipInputFrames = 2;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        Canvas c = canvas;
        if (c != null)
        {
            c.sortingOrder = 100;
            if (c.GetComponent<UnityEngine.UI.GraphicRaycaster>() != null)
                c.GetComponent<UnityEngine.UI.GraphicRaycaster>().enabled = true;
        }

        ApplySelectedBottleToBaseBottle();
        ApplySavedBaseTint();

        nextButton.SetActive(false);
        if (mixManager != null)
            mixManager.ResetFillData();
        InitializeFillRectangle();

        StartCoroutine(AllowInputAfterDelay(0.25f));
    }

    private IEnumerator AllowInputAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        allowInput = true;
    }

    private void OnDisable()
    {
        isDragging = false;
        StopPouring();
    }

    private void InitializeFillRectangle()
    {
        if (BaseBottle == null) return;

        Transform fillTransform = BaseBottle.transform.Find("FillRectangle");
        
        if (fillTransform == null)
        {
            GameObject fillObj = new GameObject("FillRectangle");
            fillObj.transform.SetParent(BaseBottle.transform, false);

            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            
            // Anchor to bottom-center, stretch horizontally
            fillRect.anchorMin = new Vector2(0.1f, 0f);
            fillRect.anchorMax = new Vector2(0.9f, 0f);
            fillRect.pivot = new Vector2(0.5f, 0f); // Pivot at bottom
            fillRect.anchoredPosition = Vector2.zero;
            fillRect.sizeDelta = new Vector2(0, 0); // Start with no height

            // Add Image component
            fillImage = fillObj.AddComponent<Image>();
            Color initialColor = Color.white;
            initialColor.a = fillAlpha;
            fillImage.color = initialColor;
            fillImage.raycastTarget = false;
        }
        else
        {
            fillImage = fillTransform.GetComponent<Image>();
        }

        UpdateFillVisual();
    }

    private void Update()
    {
        if (BaseBottle == null) return;
        if (skipInputFrames > 0)
            skipInputFrames--;

        if (Input.GetMouseButtonDown(0) && allowInput && skipInputFrames <= 0)
        {
            if (IsClickingOnBaseBottle())
            {
                isDragging = true;
                dragStartMousePos = Input.mousePosition;
                dragStartBottlePos = BaseBottle.rectTransform.anchoredPosition;
            }
            else if (!isDragging)
            {
                Image hoveredBase = GetHoveredBase();
                if (hoveredBase != null)
                {
                    StartPouring(hoveredBase);
                }
            }
        }

        if (Input.GetMouseButton(0) && !isDragging && currentPouringBase != null)
        {
            Image hoveredBase = GetHoveredBase();
            if (hoveredBase != currentPouringBase)
            {
                StopPouring();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopPouring();
        }

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

                // Calculate delta and update position (keep same Y for this bottle size)
                float deltaX = currentMousePos.x - startMousePos.x;
                float newX = dragStartBottlePos.x + deltaX;
                BaseBottle.rectTransform.anchoredPosition = new Vector2(newX, dragStartBottlePos.y);
            }
        }

        // Handle mouse up
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        // Drain fill when bottle is over trash (same color/ratios; mood updates via OnStateChanged)
        if (trashRect != null && mixManager != null && mixManager.FillLevel > 0f && RectOverlaps(BaseBottle.rectTransform, trashRect))
        {
            mixManager.DrainFill(drainRatePerSecond * Time.deltaTime);
            UpdateFillVisual();
            if (mixManager.FillLevel <= 0f && nextButton != null)
                nextButton.SetActive(false);
        }
    }

    private static bool RectOverlaps(RectTransform a, RectTransform b)
    {
        Vector3[] cornersA = new Vector3[4];
        Vector3[] cornersB = new Vector3[4];
        a.GetWorldCorners(cornersA);
        b.GetWorldCorners(cornersB);
        float minAx = Mathf.Min(cornersA[0].x, cornersA[2].x);
        float maxAx = Mathf.Max(cornersA[0].x, cornersA[2].x);
        float minAy = Mathf.Min(cornersA[0].y, cornersA[2].y);
        float maxAy = Mathf.Max(cornersA[0].y, cornersA[2].y);
        float minBx = Mathf.Min(cornersB[0].x, cornersB[2].x);
        float maxBx = Mathf.Max(cornersB[0].x, cornersB[2].x);
        float minBy = Mathf.Min(cornersB[0].y, cornersB[2].y);
        float maxBy = Mathf.Max(cornersB[0].y, cornersB[2].y);
        return maxAx > minBx && minAx < maxBx && maxAy > minBy && minAy < maxBy;
    }

 private void ApplySelectedBottleToBaseBottle()
{

    /* update: only scale is copied over from small/medium/large glass (since separate sprites) */
    Vector2 pos = BaseBottle.rectTransform.anchoredPosition;
    Vector3 scale = BaseBottle.rectTransform.localScale;

    switch (mixManager.SelectedBottle)
    {
        case "small":
            pos = baseBottlePosSmall;
            scale = smallBottleUI.rectTransform.localScale;
            break;

        case "medium":
            pos = baseBottlePosMedium;
            scale =  mediumBottleUI.rectTransform.localScale;
            break;

        case "large":
            pos = baseBottlePosLarge;
            scale = largeBottleUI.rectTransform.localScale;
            break;
    }

    BaseBottle.rectTransform.anchoredPosition = pos;
    BaseBottle.rectTransform.localScale = scale;
}

    private void SetBase(string baseKey, Color tint)
    {
        mixManager.SetBase(baseKey);
        BaseBottle.color = tint;

        nextButton.SetActive(true);
    }

    private void StartPouring(Image baseJar)
    {
        if (baseJar == null) return;

        currentPouringBase = baseJar;
        
        // Start coroutine to spawn drops continuously
        if (pouringCoroutine != null)
            StopCoroutine(pouringCoroutine);
        
        pouringCoroutine = StartCoroutine(PourLiquid(baseJar));
    }

    private void StopPouring()
    {
        currentPouringBase = null;
        
        if (pouringCoroutine != null)
        {
            StopCoroutine(pouringCoroutine);
            pouringCoroutine = null;
        }
    }

    private IEnumerator PourLiquid(Image baseJar)
    {
        // Get base color and key
        string key = baseJar.gameObject.name.ToLower();
        Color baseColor = Color.white;
        string baseKey = "";

        if (key.Contains("blood"))
        {
            baseKey = "blood";
            baseColor = mixManager.GetBaseColor(baseKey);
        }
        else if (key.Contains("holy"))
        {
            baseKey = "holywater";
            baseColor = mixManager.GetBaseColor(baseKey);
        }
        else if (key.Contains("spirits"))
        {
            baseKey = "spirits";
            baseColor = mixManager.GetBaseColor(baseKey);
        }
        else if (key.Contains("moon") || key.Contains("shine"))
        {
            baseKey = "moonshine";
            baseColor = mixManager.GetBaseColor(baseKey);
        }

        // Spawn drops continuously
        while (currentPouringBase == baseJar)
        {
            SpawnDrop(baseJar, baseColor, baseKey);
            yield return new WaitForSeconds(dropSpawnInterval);
        }
    }

    private void SpawnDrop(Image baseJar, Color dropColor, string baseKey)
    {
        if (canvas == null || baseJar == null) return;

        GameObject dropObj = new GameObject("LiquidDrop");
        dropObj.transform.SetParent(canvas.transform, false);

        RectTransform dropRect = dropObj.AddComponent<RectTransform>();
        
        Vector2 jarPos = baseJar.rectTransform.anchoredPosition;
        float jarHeight = baseJar.rectTransform.sizeDelta.y * baseJar.rectTransform.localScale.y;
        dropRect.anchoredPosition = new Vector2(jarPos.x, jarPos.y - jarHeight * 0.5f + dropSpawnOffsetY);
        dropRect.sizeDelta = new Vector2(dropSize, dropSize);
        dropRect.anchorMin = new Vector2(0.5f, 0.5f);
        dropRect.anchorMax = new Vector2(0.5f, 0.5f);
        dropRect.pivot = new Vector2(0.5f, 0.5f);

        Image dropImage = dropObj.AddComponent<Image>();
        dropImage.color = dropColor;
        dropImage.raycastTarget = false; // Don't block clicks

        // Use default white sprite (Unity creates a white square)
        dropImage.sprite = null; // Will use default white sprite

        // Add Pour script
        Pour pourScript = dropObj.AddComponent<Pour>();
        pourScript.SetBaseInfo(baseKey); // Tell the drop which base it represents
        
        // Add CanvasRenderer (required for UI)
        dropObj.AddComponent<CanvasRenderer>();
    }

    public bool CheckDropCollision(RectTransform dropRect)
    {
        if (BaseBottle == null || dropRect == null) return false;

        RectTransform bottleRect = BaseBottle.rectTransform;
        Vector2 dropPos = dropRect.anchoredPosition;
        Vector2 bottlePos = bottleRect.anchoredPosition;
        Vector2 bottleSize = bottleRect.sizeDelta * bottleRect.localScale;

        // Simple bounding box collision check (accounting for drop radius)
        float dropRadius = dropRect.sizeDelta.x * dropRect.localScale.x * 0.5f;
        
        // Check if drop center is within bottle bounds (expanded by drop radius)
        bool inX = dropPos.x >= (bottlePos.x - bottleSize.x * 0.5f - dropRadius) && 
                   dropPos.x <= (bottlePos.x + bottleSize.x * 0.5f + dropRadius);
        bool inY = dropPos.y >= (bottlePos.y - bottleSize.y * 0.5f - dropRadius) && 
                   dropPos.y <= (bottlePos.y + bottleSize.y * 0.5f + dropRadius);

        return inX && inY;
    }

    public void CatchDrop(string baseKey, Color dropColor)
    {
        if (mixManager == null) return;
        if (mixManager.FillLevel >= maxFillLevel) return;

        mixManager.AddDrip(baseKey, fillAmountPerDrop);
        UpdateFillVisual();

        // Show next button when bottle has some liquid
        if (mixManager.FillLevel > 0 && nextButton != null)
        {
            nextButton.SetActive(true);
        }
    }

    private Color CalculateMixedColor()
    {
        if (mixManager == null || mixManager.FillLevel <= 0f || mixManager.BaseAmounts.Count == 0)
        {
            return Color.white;
        }

        // Weighted color blending: each base contributes based on its percentage
        Color mixedColor = Color.black;
        float totalAmount = 0f;

        foreach (var kvp in mixManager.BaseAmounts)
        {
            totalAmount += kvp.Value;
        }

        if (totalAmount <= 0f) return Color.white;

        foreach (var kvp in mixManager.BaseAmounts)
        {
            string baseKey = kvp.Key;
            float amount = kvp.Value;
            
            if (amount > 0f)
            {
                Color baseColor = mixManager.GetBaseColor(baseKey);
                float weight = amount / totalAmount;
                
                mixedColor.r += baseColor.r * weight;
                mixedColor.g += baseColor.g * weight;
                mixedColor.b += baseColor.b * weight;
            }
        }

        mixedColor.a = 1f;
        return mixedColor;
    }

    private void UpdateFillVisual()
    {
        if (fillImage == null || BaseBottle == null || mixManager == null) return;

        RectTransform fillRect = fillImage.rectTransform;
        RectTransform bottleRect = BaseBottle.rectTransform;
        
        // Use rect instead of sizeDelta to get actual rendered size (accounts for sprite not filling RectTransform)
        float bottleHeight = bottleRect.rect.height;
        float bottleWidth = bottleRect.rect.width;
        float fillHeight = bottleHeight * fillHeightMultiplier * mixManager.FillLevel;
        float fillWidth = bottleWidth * fillWidthMultiplier;
        
        fillRect.sizeDelta = new Vector2(fillWidth, fillHeight);

        Color mixedColor = CalculateMixedColor();
        mixedColor.a = fillAlpha;
        fillImage.color = mixedColor;
    }

    private void ApplySavedBaseTint()
    {
        if (mixManager == null || BaseBottle == null) return;

        BaseBottle.color = mixManager.GetBaseColor(mixManager.SelectedBase);
    }

    public void NextPressed()
    {
        if (CurrentScreen != null) CurrentScreen.SetActive(false);
        if (NextScreen != null) NextScreen.SetActive(true);
    }

    public void BackPressed()
    {
        if (mixManager != null)
            mixManager.ResetFillData();
        if (CurrentScreen != null) CurrentScreen.SetActive(false);
        if (PreviousScreen != null) PreviousScreen.SetActive(true);
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

        foreach (var result in results)
        {
            if (result.gameObject == BaseBottle.gameObject)
            {
                return true;
            }
            if (result.gameObject.GetComponent<Image>() != null)
            {
                break;
            }
        }
        return false;
    }
}

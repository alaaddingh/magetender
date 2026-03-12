using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/* Copies BaseBottle appearance and position to Ingredients screen; fill settings from BaseController. */
public class IngredientController : MonoBehaviour
{
    [Header("Ingredients bottle to target")]
    public Image IngredientsBottle;

    [Header("Base screen bottle in scene")]
    public Image BaseBottle;
    [SerializeField] private BaseController baseController;

    [Header("Ingredients bottle position (per size, so it sits on counter)")]
    public Vector2 ingredientsBottlePosSmall = new Vector2(0f, -120f);
    public Vector2 ingredientsBottlePosMedium = new Vector2(0f, -160f);
    public Vector2 ingredientsBottlePosLarge = new Vector2(0f, -200f);

    [Header("UI Panels")]
    public GameObject CurrentScreen;
    public GameObject NextScreen;

    [Header("Selection Rules")]
    [Min(1)] public int MaxIngredients = 3;

    private MixManager mixManager;
    private Image fillImage;

    private void Awake()
    {
        mixManager = FindFirstObjectByType<MixManager>();
        if (baseController == null)
            baseController = FindFirstObjectByType<BaseController>();
    }

    private void OnEnable()
    {
        ApplyIngredientSelectionCap();
        ApplyFromBaseBottle();
        InitializeFillRectangle();
    }

    private void Start()
    {
        ApplyIngredientSelectionCap();
        ApplyFromBaseBottle();
        InitializeFillRectangle();
    }

    private void ApplyIngredientSelectionCap()
    {
        if (mixManager == null)
            mixManager = FindFirstObjectByType<MixManager>();
        if (mixManager == null) return;

        mixManager.SetMaxIngredients(MaxIngredients);
    }

    private void ApplyFromBaseBottle()
    {
        UIImgUtil.CopyAppearance(BaseBottle, IngredientsBottle);
        if (IngredientsBottle == null || mixManager == null) return;
        Vector2 pos;
        switch (mixManager.SelectedBottle)
        {
            case "small": pos = ingredientsBottlePosSmall; break;
            case "medium": pos = ingredientsBottlePosMedium; break;
            case "large": pos = ingredientsBottlePosLarge; break;
            default: pos = ingredientsBottlePosMedium; break;
        }
        IngredientsBottle.rectTransform.anchoredPosition = pos;
    }

    private void InitializeFillRectangle()
    {
        if (IngredientsBottle == null || mixManager == null || baseController == null) return;

        Transform fillTransform = IngredientsBottle.transform.Find("FillRectangle");

        if (fillTransform == null)
        {
            GameObject fillObj = new GameObject("FillRectangle");
            fillObj.transform.SetParent(IngredientsBottle.transform, false);

            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0.5f, 0f);
            fillRect.anchorMax = new Vector2(0.5f, 0f);
            fillRect.pivot = new Vector2(0.5f, 0f);
            fillRect.anchoredPosition = Vector2.zero;
            fillRect.sizeDelta = Vector2.zero;

            fillImage = fillObj.AddComponent<Image>();
            fillImage.sprite = FillCircleSprite.Get(baseController.fillYCutoff);
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Vertical;
            fillImage.fillOrigin = (int)Image.OriginVertical.Bottom;
            fillImage.fillAmount = 0f;
            Color initialColor = Color.white;
            initialColor.a = baseController.fillAlpha;
            fillImage.color = initialColor;
            fillImage.raycastTarget = false;
        }
        else
        {
            fillImage = fillTransform.GetComponent<Image>();
            fillImage.sprite = FillCircleSprite.Get(baseController.fillYCutoff);
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Vertical;
            fillImage.fillOrigin = (int)Image.OriginVertical.Bottom;
            RectTransform fillRect = fillImage.rectTransform;
            fillRect.anchorMin = new Vector2(0.5f, 0f);
            fillRect.anchorMax = new Vector2(0.5f, 0f);
            fillRect.pivot = new Vector2(0.5f, 0f);
        }

        UpdateFillVisual();
    }

    private void UpdateFillVisual()
    {
        if (fillImage == null || IngredientsBottle == null || mixManager == null || baseController == null) return;

        RectTransform fillRect = fillImage.rectTransform;
        RectTransform bottleRect = IngredientsBottle.rectTransform;
        float bottleWidth = bottleRect.rect.width;
        float diameter = bottleWidth * baseController.fillWidthMultiplier;
        fillRect.sizeDelta = new Vector2(diameter, diameter * baseController.fillCircleAspect);
        fillRect.anchoredPosition = new Vector2(0f, baseController.fillBottomOffsetY);
        fillImage.sprite = FillCircleSprite.Get(baseController.fillYCutoff);
        fillImage.fillAmount = mixManager.FillLevel;

        Color mixedColor = CalculateMixedColor();
        mixedColor.a = baseController.fillAlpha;
        fillImage.color = mixedColor;
    }

    private Color CalculateMixedColor()
    {
        if (mixManager.FillLevel <= 0f || mixManager.BaseAmounts.Count == 0)
        {
            return Color.white;
        }

        Color mixedColor = Color.black;
        float totalAmount = 0f;

        foreach (var kvp in mixManager.BaseAmounts)
        {
            float amount = kvp.Value;
            totalAmount += amount;
        }

        if (totalAmount <= 0f) return Color.white;

        foreach (var kvp in mixManager.BaseAmounts)
        {
            string baseKey = kvp.Key;
            float amount = kvp.Value;
            
            if (amount > 0f)
            {
                Color baseColor = GetBaseColor(baseKey);
                float weight = amount / totalAmount;
                
                mixedColor.r += baseColor.r * weight;
                mixedColor.g += baseColor.g * weight;
                mixedColor.b += baseColor.b * weight;
            }
        }

        mixedColor.a = 1f;
        return mixedColor;
    }

    private Color GetBaseColor(string baseKey)
    {
        if (mixManager == null) return Color.white;
        
        switch (baseKey.ToLower())
        {
            case "blood":
                return mixManager.BloodColor;
            case "holywater":
                return mixManager.HolyWaterColor;
            case "spirits":
                return mixManager.SpiritsColor;
            case "moonshine":
                return mixManager.MoonShineColor;
            default:
                return Color.white;
        }
    }

    public void NextPressed()
    {
		if (AudioManager.Instance != null)
			AudioManager.Instance.PlayButtonClick();
        CurrentScreen.SetActive(false);
        NextScreen.SetActive(true);
    }
}

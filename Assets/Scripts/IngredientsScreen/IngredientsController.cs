using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/*
purpose: 
    simply copy over the existing BaseBottle (size + color)
    to ingredients screen 
    
    TO-DO: Make a NextPressed() function to transition to final "Mix" screen
*/
public class IngredientController : MonoBehaviour
{
    [Header("Ingredients bottle to target")]
    public Image IngredientsBottle;

    [Header("Base screen bottle in scene")]
    public Image BaseBottle;

    [Header("Fill Settings")]
    [Range(0f, 1f)]
    public float fillWidthMultiplier = 0.18f;
    [Range(0f, 1f)]
    public float fillHeightMultiplier = 0.5f;
    [Range(0f, 1f)]
    public float fillAlpha = 0.7f;

    private MixManager mixManager;
    private Image fillImage;

    private void Awake()
    {
        mixManager = FindFirstObjectByType<MixManager>();
    }

    private void OnEnable()
    {
        ApplyFromBaseBottle();
        InitializeFillRectangle();
    }

    private void Start()
    {
        ApplyFromBaseBottle();
        InitializeFillRectangle();
    }

    private void ApplyFromBaseBottle()
    {
        UIImgUtil.CopyAppearance(BaseBottle, IngredientsBottle);
    }

    private void InitializeFillRectangle()
    {
        if (IngredientsBottle == null || mixManager == null) return;

        Transform fillTransform = IngredientsBottle.transform.Find("FillRectangle");
        
        if (fillTransform == null)
        {
            GameObject fillObj = new GameObject("FillRectangle");
            fillObj.transform.SetParent(IngredientsBottle.transform, false);

            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0.1f, 0f);
            fillRect.anchorMax = new Vector2(0.9f, 0f);
            fillRect.pivot = new Vector2(0.5f, 0f);
            fillRect.anchoredPosition = Vector2.zero;
            fillRect.sizeDelta = new Vector2(0, 0);

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

    private void UpdateFillVisual()
    {
        if (fillImage == null || IngredientsBottle == null || mixManager == null) return;

        RectTransform fillRect = fillImage.rectTransform;
        RectTransform bottleRect = IngredientsBottle.rectTransform;
        
        float bottleHeight = bottleRect.rect.height;
        float bottleWidth = bottleRect.rect.width;
        float fillHeight = bottleHeight * fillHeightMultiplier * mixManager.FillLevel;
        float fillWidth = bottleWidth * fillWidthMultiplier;
        
        fillRect.sizeDelta = new Vector2(fillWidth, fillHeight);

        Color mixedColor = CalculateMixedColor();
        mixedColor.a = fillAlpha;
        fillImage.color = mixedColor;
    }

    private Color CalculateMixedColor()
    {
        if (mixManager.FillLevel <= 0f || mixManager.BaseAmounts.Count == 0)
        {
            return Color.white;
        }

        // Weighted color blending: each base contributes based on its percentage
        Color mixedColor = Color.black;
        float totalWeight = 0f;

        foreach (var kvp in mixManager.BaseAmounts)
        {
            string baseKey = kvp.Key;
            float amount = kvp.Value;
            
            if (amount > 0f)
            {
                Color baseColor = GetBaseColor(baseKey);
                float weight = amount / mixManager.FillLevel;
                
                mixedColor.r += baseColor.r * weight;
                mixedColor.g += baseColor.g * weight;
                mixedColor.b += baseColor.b * weight;
                mixedColor.a += baseColor.a * weight;
                
                totalWeight += weight;
            }
        }

        if (totalWeight > 0f)
        {
            mixedColor.r /= totalWeight;
            mixedColor.g /= totalWeight;
            mixedColor.b /= totalWeight;
            mixedColor.a /= totalWeight;
        }

        return mixedColor;
    }

    private Color GetBaseColor(string baseKey)
    {
        switch (baseKey.ToLower())
        {
            case "blood":
                return new Color(0.7f, 0.1f, 0.1f, 1f);
            case "holywater":
                return new Color(0.7f, 0.9f, 1f, 1f);
            case "spirits":
                return new Color(0.85f, 0.95f, 0.8f, 1f);
            case "moonshine":
                return new Color(0.9f, 0.7f, 0.9f, 1f);
            default:
                return Color.white;
        }
    }
}

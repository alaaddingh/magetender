using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/* Copies BaseBottle appearance and position to Ingredients screen; fill settings from BaseController. */
public class IngredientsController : MonoBehaviour
{
    [Header("Ingredients bottle to target")]
    public Image IngredientsBottle;

    [Header("Base screen bottle in scene")]
    public Image BaseBottle;
    [SerializeField] private BaseController baseController;

    private Image fillImage;

    [Header("Ingredients bottle position (per size, so it sits on counter)")]
    public Vector2 ingredientsBottlePosSmall = new Vector2(0f, -120f);
    public Vector2 ingredientsBottlePosMedium = new Vector2(0f, -160f);
    public Vector2 ingredientsBottlePosLarge = new Vector2(0f, -200f);



    [Header("UI Panels")]
    public GameObject CurrentScreen;
    public GameObject NextScreen;

    [Header("Toppings Controller")]
    public GameObject ToppingsController;


    [Header("Selection Rules")]
    [Min(1)] public int MaxIngredients = 3;

    private MixManager mixManager;
	[SerializeField] private IngredientHoverSnapUI ingredientHoverUi;

    [Header("Ingredient tint")]
    public Color ingredientTintColor = new Color(0.9f, 0.7f, 1f, 1f);
    [Range(0f, 1f)]
    public float ingredientTintPerIngredient = 0.15f;
    [Tooltip("If present in Ingredients.json, blend toward per-ingredient colors instead of a single global tint.")]
    public bool useIngredientColorsFromJson = true;

    private readonly Dictionary<string, IngredientData> ingredientById = new Dictionary<string, IngredientData>();
    /* hide the ingredients + shelves, then show the toppings after first "Next" click */
    [SerializeField] private GameObject Ingredients;
    [SerializeField] private GameObject Shelves;
    [SerializeField] private GameObject HoverTooltip;


    [SerializeField] private GameObject Toppings;
    private bool ToppingsNext = true;

    private void Awake()
    {
        mixManager = FindFirstObjectByType<MixManager>();
        if (baseController == null)
            baseController = FindFirstObjectByType<BaseController>();
		if (ingredientHoverUi == null)
			ingredientHoverUi = FindFirstObjectByType<IngredientHoverSnapUI>();
    }

    private void OnEnable()
    {
        ApplyIngredientSelectionCap();
        ApplyFromBaseBottle();
        LoadIngredientData();
        if (IngredientsBottle != null)
            IngredientsBottle.raycastTarget = false;
        InitializeFillRectangle();
        HookIngredientEvents(true);
    }

    private void OnDisable()
    {
		if (ingredientHoverUi != null)
			ingredientHoverUi.ForceCompleteActiveIngredientAnimations();
        HookIngredientEvents(false);
    }

    private void Start()
    {
        ApplyIngredientSelectionCap();
        ApplyFromBaseBottle();
        LoadIngredientData();
        InitializeFillRectangle();
    }

    private void HookIngredientEvents(bool on)
    {
        if (mixManager == null)
            mixManager = FindFirstObjectByType<MixManager>();
        if (mixManager == null) return;

        if (on)
        {
            mixManager.OnIngredientAdded -= OnIngredientsChanged;
            mixManager.OnIngredientRemoved -= OnIngredientsChanged;
            mixManager.OnIngredientAdded += OnIngredientsChanged;
            mixManager.OnIngredientRemoved += OnIngredientsChanged;
        }
        else
        {
            mixManager.OnIngredientAdded -= OnIngredientsChanged;
            mixManager.OnIngredientRemoved -= OnIngredientsChanged;
        }
    }

    private void OnIngredientsChanged(string _)
    {
        UpdateFillVisual();
    }

    private void ApplyIngredientSelectionCap()
    {
        mixManager = FindFirstObjectByType<MixManager>();
        mixManager.SetMaxIngredients(MaxIngredients);
    }

    private void ApplyFromBaseBottle()
    {
        UIImgUtil.CopyAppearance(BaseBottle, IngredientsBottle);
        if (IngredientsBottle == null || mixManager == null) return;
        Vector2 pos = ingredientsBottlePosMedium;
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

        if (mixManager.FillLevel > 0f && mixManager.SelectedIngredients != null)
        {
            int count = mixManager.SelectedIngredients.Count;
            float t = Mathf.Clamp01(count * ingredientTintPerIngredient);
            if (t > 0f)
            {
                Color tint = ingredientTintColor;
                if (useIngredientColorsFromJson)
                {
                    bool hasTint = TryGetIngredientBlendColor(out Color blend);
                    if (hasTint)
                        tint = blend;
                }
                mixedColor = Color.Lerp(mixedColor, tint, t);
            }
        }

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

    private void LoadIngredientData()
    {
        ingredientById.Clear();

        string path = "Data/Ingredients";
        if (LanguageManager.Instance != null)
            path = LanguageManager.Instance.GetIngredientsResourcePath();
        else
        {
            string lang = PlayerPrefs.GetString("GameLanguage", LanguageManager.LangEnglish);
            if (lang == LanguageManager.LangSpanish)
                path = "Data/Ingredients_es";
            else if (lang == LanguageManager.LangArabic)
                path = "Data/Ingredients_ar";
        }

        TextAsset json = Resources.Load<TextAsset>(path);
        if (json == null) return;

        IngredientsFile file = JsonUtility.FromJson<IngredientsFile>(json.text);
        if (file == null || file.ingredients == null) return;

        foreach (var ing in file.ingredients)
        {
            if (!string.IsNullOrEmpty(ing.id))
                ingredientById[ing.id] = ing;
        }
    }

    private bool TryGetIngredientBlendColor(out Color color)
    {
        color = Color.clear;
        if (mixManager == null || mixManager.SelectedIngredients == null || mixManager.SelectedIngredients.Count == 0)
            return false;

        int count = 0;
        float r = 0f;
        float g = 0f;
        float b = 0f;

        foreach (string id in mixManager.SelectedIngredients)
        {
            if (string.IsNullOrEmpty(id)) continue;
            if (!ingredientById.TryGetValue(id, out IngredientData data) || data == null) continue;
            if (data.color == null) continue;

            Color c = data.color.ToUnityColor();
            r += c.r;
            g += c.g;
            b += c.b;
            count++;
        }

        if (count <= 0) return false;

        color = new Color(r / count, g / count, b / count, 1f);
        return true;
    }

    public void NextPressed()
    {
        if(ToppingsNext == true)
        {
            AudioManager.Instance.PlayButtonClick();
			if (ingredientHoverUi != null)
				ingredientHoverUi.ForceCompleteActiveIngredientAnimations();
            Ingredients.SetActive(false);
            Shelves.SetActive(false);
            Toppings.SetActive(true);
            HoverTooltip.SetActive(false);
            ToppingsController.SetActive(true);
            ToppingsNext = false;
            return;

        }
        CurrentScreen.SetActive(false);
        NextScreen.SetActive(true);


    }
}

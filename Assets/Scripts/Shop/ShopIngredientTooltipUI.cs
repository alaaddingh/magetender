using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using RTLTMPro;

public class ShopIngredientTooltipUI : MonoBehaviour
{
	[Header("Ingredient root")]
	[SerializeField] private Transform ingredientRoot;

	[Header("Tooltip UI")]
	public GameObject tooltipRoot;
	public TextMeshProUGUI tooltipNameText;
	public TextMeshProUGUI tooltipDescText;
	public TextMeshProUGUI tooltipEffectText;
	public Vector2 tooltipOffset = new Vector2(16f, -16f);

	[Header("Arabic (optional)")]
	[SerializeField] private TMP_FontAsset arabicFontOverride;

	private Image hoveredIngredient;
	private Dictionary<string, IngredientData> byId = new Dictionary<string, IngredientData>();
	private TMP_FontAsset tooltipNameOriginalFont;
	private TMP_FontAsset tooltipDescOriginalFont;
	private TMP_FontAsset tooltipEffectOriginalFont;

	private GameObject TooltipVisualRoot()
	{
		if (tooltipRoot == null)
			return null;
		if (tooltipRoot != gameObject)
			return tooltipRoot;
		if (transform.childCount > 0)
			return transform.GetChild(0).gameObject;
		return null;
	}

	private void Awake()
	{
		if (ingredientRoot == null)
			ingredientRoot = transform;

		LoadIngredientData();
		HideTooltip();

		tooltipNameOriginalFont = tooltipNameText != null ? tooltipNameText.font : null;
		tooltipDescOriginalFont = tooltipDescText != null ? tooltipDescText.font : null;
		tooltipEffectOriginalFont = tooltipEffectText != null ? tooltipEffectText.font : null;
	}

	private void OnEnable()
	{
		LanguageManager.OnLanguageChanged += LoadIngredientData;
	}

	private void OnDisable()
	{
		LanguageManager.OnLanguageChanged -= LoadIngredientData;
	}

	private void Update()
	{
		hoveredIngredient = GetHoveredIngredientUnderRoot();

		if (hoveredIngredient != null)
		{
			ShowTooltipFor(hoveredIngredient.gameObject.name);
			MoveTooltipToMouse();
		}
		else
			HideTooltip();
	}

	private void LoadIngredientData()
	{
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
		if (json == null)
			return;

		IngredientsFile file = JsonUtility.FromJson<IngredientsFile>(json.text);
		if (file == null || file.ingredients == null)
			return;

		byId.Clear();
		foreach (var ing in file.ingredients)
		{
			if (!string.IsNullOrEmpty(ing.id))
				byId[ing.id] = ing;
		}
	}

	private Image GetHoveredIngredientUnderRoot()
	{
		if (EventSystem.current == null || ingredientRoot == null)
			return null;

		var pointer = new PointerEventData(EventSystem.current)
		{
			position = Input.mousePosition
		};

		var results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointer, results);

		foreach (var r in results)
		{
			if (!r.gameObject.CompareTag("Ingredients"))
				continue;

			if (!r.gameObject.transform.IsChildOf(ingredientRoot))
				continue;

			var img = r.gameObject.GetComponent<Image>();
			if (img != null)
				return img;
		}

		return null;
	}

	private void ShowTooltipFor(string id)
	{
		GameObject vis = TooltipVisualRoot();
		if (vis == null)
			return;

		vis.SetActive(true);

		if (!byId.TryGetValue(id, out IngredientData data))
		{
			SetTooltipText(tooltipNameText, tooltipNameOriginalFont, id, preserveNumbers: true);
			SetTooltipText(tooltipDescText, tooltipDescOriginalFont, L.Get("ingredient_no_match"),
				preserveNumbers: true);
			SetTooltipText(tooltipEffectText, tooltipEffectOriginalFont, "", preserveNumbers: true);
			return;
		}

		SetTooltipText(tooltipNameText, tooltipNameOriginalFont, data.name, preserveNumbers: true);
		SetTooltipText(tooltipDescText, tooltipDescOriginalFont, data.description, preserveNumbers: true);

		float x = 0f;
		float y = 0f;
		if (data.effect != null)
		{
			x = data.effect.x;
			y = data.effect.y;
		}

		if (tooltipEffectText != null)
		{
			string effect =
				$"x: {x:+0.00;-0.00} ({L.Get("ingredient_axis_x")})\n" +
				$"y: {y:+0.00;-0.00} ({L.Get("ingredient_axis_y")})";
			SetTooltipText(tooltipEffectText, tooltipEffectOriginalFont, effect, preserveNumbers: true);
		}
	}

	private void HideTooltip()
	{
		GameObject vis = TooltipVisualRoot();
		if (vis != null)
			vis.SetActive(false);
	}

	private void MoveTooltipToMouse()
	{
		GameObject vis = TooltipVisualRoot();
		if (vis == null)
			return;

		RectTransform rt = vis.GetComponent<RectTransform>();
		if (rt == null)
			return;

		Vector2 pos = (Vector2)Input.mousePosition + tooltipOffset;
		float edge = 0.15f;
		float innerLeft = Screen.width * edge;
		float innerRight = Screen.width * (1f - edge);
		float innerBottom = Screen.height * edge;
		float innerTop = Screen.height * (1f - edge);
		if (pos.x < innerLeft) pos.x = innerLeft;
		if (pos.x > innerRight) pos.x = innerRight;
		if (pos.y < innerBottom) pos.y = innerBottom;
		if (pos.y > innerTop) pos.y = innerTop;
		rt.position = pos;
	}

	private static bool IsArabicLanguage()
	{
		string lang = LanguageManager.Instance != null
			? LanguageManager.Instance.CurrentLanguage
			: PlayerPrefs.GetString("GameLanguage", LanguageManager.LangEnglish);
		return lang == LanguageManager.LangArabic;
	}

	private void SetTooltipText(TextMeshProUGUI t, TMP_FontAsset originalFont, string raw, bool preserveNumbers)
	{
		if (t == null)
			return;

		bool isArabic = IsArabicLanguage();

		if (isArabic && arabicFontOverride != null)
			t.font = arabicFontOverride;
		else if (originalFont != null)
			t.font = originalFont;

		if (t is RTLTextMeshPro rtl)
		{
			rtl.Farsi = false;
			rtl.FixTags = true;
			rtl.PreserveNumbers = preserveNumbers;
			rtl.ForceFix = isArabic;
			rtl.text = raw ?? string.Empty;
			return;
		}

		if (isArabic)
		{
			t.isRightToLeftText = true;
			t.text = RtlText.FixIfArabic(raw, preserveNumbers: preserveNumbers, fixTags: true, reverseOutput: true);
		}
		else
		{
			t.isRightToLeftText = false;
			t.text = raw ?? string.Empty;
		}
	}
}

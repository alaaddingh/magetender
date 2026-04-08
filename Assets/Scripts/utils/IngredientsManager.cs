using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using RTLTMPro;

public class IngredientHoverSnapUI : MonoBehaviour
{
	[Header("target position (per bottle size)")]
	public Vector2 snapTargetPosSmall;
	public Vector2 snapTargetPosMedium;
	public Vector2 snapTargetPosLarge;

	[Header("Ingredient area (for unlock filter + shelf positions)")]
	[SerializeField] private Transform ingredientRoot;

	[Header("tooltip ui")]
	public GameObject tooltipRoot;
	public TextMeshProUGUI tooltipNameText;
	public TextMeshProUGUI tooltipDescText;
	public TextMeshProUGUI tooltipEffectText;
	public Vector2 tooltipOffset = new Vector2(16f, -16f);

	[Header("debug")]
	public bool debugLogs = true;
	public TextMeshProUGUI debugOverlayText;

	[Header("Arabic (optional)")]
	[SerializeField] private TMP_FontAsset arabicFontOverride;

	private Image hoveredIngredient;
	private MixManager mixManager;
	private Dictionary<string, Vector2> shelfPositions = new Dictionary<string, Vector2>();

	private Dictionary<string, IngredientData> byId = new Dictionary<string, IngredientData>();

	private TMP_FontAsset tooltipNameOriginalFont;
	private TMP_FontAsset tooltipDescOriginalFont;
	private TMP_FontAsset tooltipEffectOriginalFont;

	[Tooltip("Max distance from snap target to consider ingredient 'in drink' for toggle-off")]
	public float inDrinkThreshold = 15f;

	private static bool s_processedClickThisFrame;
	private float nextDebugTime = 0f;
	private const float debugInterval = 0.25f;

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
		LoadIngredientData();
		HideTooltip();
		mixManager = FindFirstObjectByType<MixManager>();
		RecordShelfPositions();

		tooltipNameOriginalFont = tooltipNameText != null ? tooltipNameText.font : null;
		tooltipDescOriginalFont = tooltipDescText != null ? tooltipDescText.font : null;
		tooltipEffectOriginalFont = tooltipEffectText != null ? tooltipEffectText.font : null;
	}

	private void OnEnable()
	{
		LanguageManager.OnLanguageChanged += LoadIngredientData;
		RecordShelfPositions();
		StartCoroutine(WaitForGameManagerUnlockHook());
	}

	private void OnDisable()
	{
		LanguageManager.OnLanguageChanged -= LoadIngredientData;
		if (GameManager.Instance != null)
			GameManager.Instance.OnIngredientUnlocksChanged -= ApplyUnlockVisibility;
	}

	private IEnumerator WaitForGameManagerUnlockHook()
	{
		int frames = 0;
		while (GameManager.Instance == null && frames < 120)
		{
			frames++;
			yield return null;
		}

		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnIngredientUnlocksChanged -= ApplyUnlockVisibility;
			GameManager.Instance.OnIngredientUnlocksChanged += ApplyUnlockVisibility;
		}

		ApplyUnlockVisibility();
	}

	private void RecordShelfPositions()
	{
		shelfPositions.Clear();
		foreach (GameObject go in EnumerateIngredientObjects())
		{
			Image img = go.GetComponent<Image>();
			if (img != null && !string.IsNullOrEmpty(go.name))
				shelfPositions[go.name] = img.rectTransform.anchoredPosition;
		}

		ApplyUnlockVisibility();
	}

	private IEnumerable<GameObject> EnumerateIngredientObjects()
	{
		if (ingredientRoot != null)
		{
			foreach (Image img in ingredientRoot.GetComponentsInChildren<Image>(true))
			{
				if (img != null && img.gameObject.CompareTag("Ingredients"))
					yield return img.gameObject;
			}
			yield break;
		}

		foreach (GameObject go in GameObject.FindGameObjectsWithTag("Ingredients"))
			yield return go;
	}

	private void ApplyUnlockVisibility()
	{
		var gm = GameManager.Instance;
		if (ingredientRoot != null)
		{
			foreach (Image img in ingredientRoot.GetComponentsInChildren<Image>(true))
			{
				if (img == null || !img.gameObject.CompareTag("Ingredients"))
					continue;

				bool on = gm != null && gm.IsIngredientUnlocked(img.gameObject.name);
				img.gameObject.SetActive(on);
			}
		}
		else
		{
			foreach (GameObject go in GameObject.FindGameObjectsWithTag("Ingredients"))
			{
				bool on = gm != null && gm.IsIngredientUnlocked(go.name);
				go.SetActive(on);
			}
		}
	}

	void LateUpdate()
	{
		s_processedClickThisFrame = false;
	}

	void Update()
	{
		hoveredIngredient = GetHoveredIngredient();

		if (hoveredIngredient != null)
		{
			string id = hoveredIngredient.gameObject.name;
			ShowTooltipFor(id);
			MoveTooltipToMouse();

			DebugTick($"hovered ingredient: {id}");
		}
		else
		{
			HideTooltip();
			DebugTick("hovered ingredient: (none)");
		}

		if (Input.GetMouseButtonDown(0) && hoveredIngredient != null && !s_processedClickThisFrame)
		{
			if (mixManager == null)
				mixManager = FindFirstObjectByType<MixManager>();
			if (mixManager == null) return;

			bool inDrink = IsInDrink(hoveredIngredient);
			if (inDrink)
			{
				bool removed = mixManager.RemoveIngredient(hoveredIngredient.name);
				if (removed)
				{
					if (AudioManager.Instance != null)
						AudioManager.Instance.PlayIngredientClick();
					hoveredIngredient.rectTransform.anchoredPosition = GetShelfPosition(hoveredIngredient.name);
				}
			}
			else
			{
				bool added = mixManager.AddIngredient(hoveredIngredient.name);
				if (added)
				{
					if (AudioManager.Instance != null)
						AudioManager.Instance.PlayIngredientClick();
					if (!shelfPositions.ContainsKey(hoveredIngredient.name))
						shelfPositions[hoveredIngredient.name] = hoveredIngredient.rectTransform.anchoredPosition;
					hoveredIngredient.rectTransform.anchoredPosition = GetSnapTarget();
				}
			}
			s_processedClickThisFrame = true;
		}
	}

	private Vector2 GetSnapTarget()
	{
		if (mixManager == null) return snapTargetPosMedium;
		switch (mixManager.SelectedBottle)
		{
			case "small": return snapTargetPosSmall;
			case "large": return snapTargetPosLarge;
			default: return snapTargetPosMedium;
		}
	}

	private bool IsInDrink(Image img)
	{
		Vector2 pos = img.rectTransform.anchoredPosition;
		return (pos - GetSnapTarget()).sqrMagnitude <= inDrinkThreshold * inDrinkThreshold;
	}

	private Vector2 GetShelfPosition(string ingredientName)
	{
		return shelfPositions.TryGetValue(ingredientName, out Vector2 pos) ? pos : GetSnapTarget();
	}

	private void DebugTick(string line)
	{
		if (!debugLogs) return;

		if (Time.unscaledTime < nextDebugTime) return;
		nextDebugTime = Time.unscaledTime + debugInterval;

		if (debugOverlayText != null)
		{
			debugOverlayText.text =
				$"eventsystem null? {(EventSystem.current == null)}\n" +
				$"{line}";
		}
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
		{
			return;
		}

		IngredientsFile file = JsonUtility.FromJson<IngredientsFile>(json.text);
		if (file == null || file.ingredients == null)
		{
			return;
		}

		byId.Clear();
		foreach (var ing in file.ingredients)
		{
			if (!string.IsNullOrEmpty(ing.id))
				byId[ing.id] = ing;
		}
	}

	private void ShowTooltipFor(string id)
	{
		if (tooltipRoot == null)
		{
			DebugTick("tooltiproot is null (not assigned in inspector)");
			return;
		}

		tooltipRoot.SetActive(true);

		if (!byId.TryGetValue(id, out IngredientData data))
		{
			SetTooltipText(tooltipNameText, tooltipNameOriginalFont, id, preserveNumbers: true);
			SetTooltipText(tooltipDescText, tooltipDescOriginalFont, L.Get("ingredient_no_match"), preserveNumbers: true);
			SetTooltipText(tooltipEffectText, tooltipEffectOriginalFont, "", preserveNumbers: true);

			DebugTick($"json lookup failed for id: {id}");
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
		if (vis == null) return;

		RectTransform rt = vis.GetComponent<RectTransform>();
		if (rt == null) return;

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

	Image GetHoveredIngredient()
	{
		if (EventSystem.current == null)
		{
			DebugTick("eventsystem.current is null");
			return null;
		}

		PointerEventData pointer = new PointerEventData(EventSystem.current);
		pointer.position = Input.mousePosition;

		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointer, results);

		foreach (RaycastResult r in results)
		{
			if (!r.gameObject.CompareTag("Ingredients"))
				continue;

			Image img = r.gameObject.GetComponent<Image>();
			if (img != null)
				return img;
		}

		return null;
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

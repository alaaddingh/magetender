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

	[Header("Bottle UI (selected ingredients parent here)")]
	[SerializeField] public Image IngredientsBottle;

	[Header("Ingredient area (for unlock filter + shelf positions)")]
	[SerializeField] private Transform ingredientRoot;

	[Header("Selected slots (assign 3 RectTransforms)")]
	[SerializeField] private RectTransform slot1;
	[SerializeField] private RectTransform slot2;
	[SerializeField] private RectTransform slot3;
	[SerializeField] private Vector3 slotScale = new Vector3(0.7f, 0.7f, 1f);

	[Header("Dissolve animation")]
	[SerializeField] private RectTransform dissolveTarget;
	[SerializeField] private float moveToDrinkSeconds = 0.12f;
	[SerializeField] private float fadeOutSeconds = 0.08f;
	[SerializeField] private float fadeInSeconds = 0.08f;
	[SerializeField] private bool allowBottleClickRemoveLast = true;

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
	private Dictionary<string, Vector3> shelfScales = new Dictionary<string, Vector3>();
	private Dictionary<string, int> ingredientToSlotIndex = new Dictionary<string, int>();
	private Dictionary<string, Coroutine> ingredientToAnim = new Dictionary<string, Coroutine>();
	private Dictionary<string, Image> ingredientImages = new Dictionary<string, Image>();
	private string[] slotOccupiedById = new string[3];

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

		RebuildSlotStateFromMix();
	}

	private void OnEnable()
	{
		LanguageManager.OnLanguageChanged += LoadIngredientData;
		RecordShelfPositions();
		StartCoroutine(WaitForGameManagerUnlockHook());
		RebuildSlotStateFromMix();
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
		shelfScales.Clear();
		foreach (GameObject go in EnumerateIngredientObjects())
		{
			Image img = go.GetComponent<Image>();
			if (img != null && !string.IsNullOrEmpty(go.name))
			{
				shelfPositions[go.name] = img.rectTransform.anchoredPosition;
				shelfScales[go.name] = img.rectTransform.localScale;
			}
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

		if (Input.GetMouseButtonDown(0) && !s_processedClickThisFrame)
		{
			if (mixManager == null)
				mixManager = FindFirstObjectByType<MixManager>();
			if (mixManager == null) return;

			if (hoveredIngredient != null)
			{
				string id = hoveredIngredient.name;
				bool inDrink = mixManager.SelectedIngredients.Contains(id);
				if (inDrink)
				{
					RemoveIngredientUi(id);
				}
				else
				{
					AddIngredientUi(id, hoveredIngredient);
				}
			}
			else if (allowBottleClickRemoveLast && IsMouseOverBottle() && mixManager.SelectedIngredients.Count > 0)
			{
				string last = mixManager.SelectedIngredients[mixManager.SelectedIngredients.Count - 1];
				RemoveIngredientUi(last);
			}

			s_processedClickThisFrame = true;
		}
	}

	private bool IsMouseOverBottle()
	{
		if (IngredientsBottle == null) return false;
		Canvas c = IngredientsBottle.canvas;
		Camera cam = c != null && c.renderMode != RenderMode.ScreenSpaceOverlay ? c.worldCamera : null;
		return RectTransformUtility.RectangleContainsScreenPoint(IngredientsBottle.rectTransform, Input.mousePosition, cam);
	}

	private void AddIngredientUi(string id, Image img)
	{
		bool added = mixManager.AddIngredient(id);
		if (!added) return;

		if (AudioManager.Instance != null)
			AudioManager.Instance.PlayIngredientClick();

		ingredientImages[id] = img;

		if (!shelfPositions.ContainsKey(id))
		{
			shelfPositions[id] = img.rectTransform.anchoredPosition;
			shelfScales[id] = img.rectTransform.localScale;
		}

		int slotIndex = ReserveSlotFor(id);
		if (slotIndex < 0)
		{
			mixManager.RemoveIngredient(id);
			return;
		}

		StartIngredientAnimationToSlot(id, img, slotIndex);
	}

	private void RemoveIngredientUi(string id)
	{
		bool removed = mixManager.RemoveIngredient(id);
		if (!removed) return;

		if (AudioManager.Instance != null)
			AudioManager.Instance.PlayIngredientClick();

		StopIngredientAnimation(id);
		ReleaseSlotFor(id);

		if (ingredientImages.TryGetValue(id, out Image img) && img != null)
		{
			img.rectTransform.SetParent(ingredientRoot, false);
			if (shelfScales.TryGetValue(id, out Vector3 s))
				img.rectTransform.localScale = s;
			img.rectTransform.anchoredPosition = GetShelfPosition(id);

			CanvasGroup cg = img.GetComponent<CanvasGroup>();
			if (cg != null) cg.alpha = 1f;
		}

		ReflowSlots();
	}

	private void StopIngredientAnimation(string id)
	{
		if (!ingredientToAnim.TryGetValue(id, out Coroutine c) || c == null)
			return;

		StopCoroutine(c);
		ingredientToAnim.Remove(id);
	}

	private void StartIngredientAnimationToSlot(string id, Image img, int slotIndex)
	{
		StopIngredientAnimation(id);
		Coroutine c = StartCoroutine(AnimateIngredientToSlot(id, img, slotIndex));
		ingredientToAnim[id] = c;
	}

	private IEnumerator AnimateIngredientToSlot(string id, Image img, int slotIndex)
	{
		if (img == null) yield break;

		RectTransform slot = GetSlot(slotIndex);
		if (slot == null) yield break;

		Canvas c = img.canvas != null ? img.canvas : (IngredientsBottle != null ? IngredientsBottle.canvas : null);
		if (c != null)
			img.rectTransform.SetParent(c.transform, true);

		CanvasGroup cg = img.GetComponent<CanvasGroup>();
		if (cg == null) cg = img.gameObject.AddComponent<CanvasGroup>();
		cg.alpha = 1f;

		Vector3 startPos = img.rectTransform.position;
		Vector3 drinkPos = GetDissolveWorldPos();

		float t = 0f;
		while (t < 1f)
		{
			t += Time.unscaledDeltaTime / Mathf.Max(0.001f, moveToDrinkSeconds);
			float u = Mathf.Clamp01(t);
			img.rectTransform.position = Vector3.Lerp(startPos, drinkPos, u);
			yield return null;
		}

		t = 0f;
		while (t < 1f)
		{
			t += Time.unscaledDeltaTime / Mathf.Max(0.001f, fadeOutSeconds);
			float u = Mathf.Clamp01(t);
			cg.alpha = 1f - u;
			yield return null;
		}

		img.rectTransform.SetParent(slot, false);
		img.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
		img.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
		img.rectTransform.pivot = new Vector2(0.5f, 0.5f);
		img.rectTransform.anchoredPosition = Vector2.zero;
		img.rectTransform.localScale = slotScale;

		t = 0f;
		while (t < 1f)
		{
			t += Time.unscaledDeltaTime / Mathf.Max(0.001f, fadeInSeconds);
			float u = Mathf.Clamp01(t);
			cg.alpha = u;
			yield return null;
		}

		ingredientToAnim.Remove(id);
	}

	private Vector3 GetDissolveWorldPos()
	{
		if (dissolveTarget != null) return dissolveTarget.position;
		if (IngredientsBottle != null) return IngredientsBottle.rectTransform.position;
		return Vector3.zero;
	}

	private int ReserveSlotFor(string id)
	{
		if (ingredientToSlotIndex.TryGetValue(id, out int existing))
			return existing;

		for (int i = 0; i < slotOccupiedById.Length; i++)
		{
			if (string.IsNullOrEmpty(slotOccupiedById[i]))
			{
				slotOccupiedById[i] = id;
				ingredientToSlotIndex[id] = i;
				return i;
			}
		}

		return -1;
	}

	private void ReleaseSlotFor(string id)
	{
		if (!ingredientToSlotIndex.TryGetValue(id, out int index))
			return;

		ingredientToSlotIndex.Remove(id);
		if (index >= 0 && index < slotOccupiedById.Length && slotOccupiedById[index] == id)
			slotOccupiedById[index] = null;
	}

	private void ReflowSlots()
	{
		if (mixManager == null) return;

		ingredientToSlotIndex.Clear();
		for (int i = 0; i < slotOccupiedById.Length; i++)
			slotOccupiedById[i] = null;

		int slotIndex = 0;
		foreach (string id in mixManager.SelectedIngredients)
		{
			if (slotIndex >= 3) break;
			slotOccupiedById[slotIndex] = id;
			ingredientToSlotIndex[id] = slotIndex;

			if (ingredientImages.TryGetValue(id, out Image img) && img != null)
			{
				RectTransform slot = GetSlot(slotIndex);
				if (slot != null)
				{
					img.rectTransform.SetParent(slot, false);
					img.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
					img.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
					img.rectTransform.pivot = new Vector2(0.5f, 0.5f);
					img.rectTransform.anchoredPosition = Vector2.zero;
					img.rectTransform.localScale = slotScale;
					CanvasGroup cg = img.GetComponent<CanvasGroup>();
					if (cg != null) cg.alpha = 1f;
				}
			}

			slotIndex++;
		}
	}

	private void RebuildSlotStateFromMix()
	{
		if (mixManager == null)
			mixManager = FindFirstObjectByType<MixManager>();

		ingredientImages.Clear();
		foreach (GameObject go in EnumerateIngredientObjects())
		{
			Image img = go.GetComponent<Image>();
			if (img != null && !string.IsNullOrEmpty(go.name))
				ingredientImages[go.name] = img;
		}

		ReflowSlots();
	}

	private RectTransform GetSlot(int index)
	{
		switch (index)
		{
			case 0: return slot1;
			case 1: return slot2;
			case 2: return slot3;
			default: return null;
		}
	}

	private Vector2 GetSnapTarget()
	{
		return snapTargetPosMedium;
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

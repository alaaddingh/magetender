using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ShopPanelController : MonoBehaviour
{
	[Header("Root")]
	[SerializeField] private Transform shopRoot;

	[Header("Optional UI")]
	[SerializeField] private TMP_Text coinsText;

	[Header("Coin spend feedback (match Day / Assess)")]
	[SerializeField] private TMP_Text coinSpendPopup;
	[SerializeField] private float coinSpendFadeDuration = 1.5f;
	[SerializeField] private float coinSpendMoveUp = 30f;

	[Header("Pricing")]
	[SerializeField] private int defaultCost = 20;

	[Serializable]
	private class IngredientCostOverride
	{
		public string ingredientId;
		public int cost = 20;
	}

	[SerializeField] private List<IngredientCostOverride> costOverrides = new List<IngredientCostOverride>();

	[Header("Cost labels (under each shop ingredient)")]
	[Tooltip("Assign Assets/Fonts/Alkhemikal Bitmap. Uses its atlas material on the price text unless you override below.")]
	[FormerlySerializedAs("costLabelFont")]
	[SerializeField] private TMP_FontAsset costLabelTypeface;
	[Tooltip("Optional. Drag the material sub-asset from Alkhemikal Bitmap if auto material is wrong.")]
	[SerializeField] private Material costLabelFontMaterial;
	[Tooltip("Text size in points for the digits (not the same as choosing the font asset above).")]
	[FormerlySerializedAs("costLabelFontSize")]
	[SerializeField] private int costLabelPointSize = 18;
	[SerializeField] private Color costLabelColor = Color.white;
	[SerializeField] private Vector2 costLabelSizeDelta = new Vector2(140f, 28f);
	[Tooltip("Moves the whole price row up (negative) or down (positive) relative to the ingredient icon.")]
	[SerializeField] private float costLabelOffsetY = -6f;
	[Tooltip("Horizontal gap in pixels between the price digits and the coin image.")]
	[SerializeField] private float costLabelLayoutSpacing = 4f;
	[SerializeField] private Vector2 costLabelCoinIconSize = new Vector2(22f, 22f);
	[Tooltip("UI Sprite (2D and UI) for the coin; shown in an Image to the right of the price.")]
	[SerializeField] private Sprite costLabelCoinSprite;

	private const string CostLabelRootName = "ShopCostLabel";
	private const string CostAmountChildName = "ShopCostAmount";
	private const string CostCoinChildName = "ShopCostCoin";

	private readonly Dictionary<string, int> costById = new Dictionary<string, int>();
	private Image hoveredIngredient;
	private float nextCoinsRefresh;
	private Coroutine spendPopupRoutine;
	private Vector2 coinSpendPopupBasePos;

	private void Awake()
	{
		if (shopRoot == null)
			shopRoot = transform;

		if (coinsText == null)
			coinsText = GetComponentInChildren<TMP_Text>(includeInactive: true);

		RebuildCosts();
		EnsureCostLabels();

		if (coinSpendPopup != null)
		{
			coinSpendPopupBasePos = coinSpendPopup.rectTransform.anchoredPosition;
			var cg = coinSpendPopup.GetComponent<CanvasGroup>();
			if (cg != null)
				cg.alpha = 0f;
			coinSpendPopup.gameObject.SetActive(false);
		}
	}

	private void OnEnable()
	{
		ResetCoinSpendPopup();
		StartCoroutine(WaitForGameManagerAndRefresh());
	}

	private IEnumerator WaitForGameManagerAndRefresh()
	{
		int frames = 0;
		while (GameManager.Instance == null && frames < 120)
		{
			frames++;
			yield return null;
		}

		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnIngredientUnlocksChanged -= RefreshVisibility;
			GameManager.Instance.OnIngredientUnlocksChanged += RefreshVisibility;
		}

		RefreshVisibility();
		RefreshCoins(force: true);
		yield return null;
		RebuildCostLabelLayouts();
	}

	private void OnDisable()
	{
		ResetCoinSpendPopup();
		if (GameManager.Instance != null)
			GameManager.Instance.OnIngredientUnlocksChanged -= RefreshVisibility;
	}

	private void Update()
	{
		RefreshCoins(force: false);

		hoveredIngredient = GetHoveredIngredient();
		if (hoveredIngredient == null)
			return;

		if (Input.GetMouseButtonDown(0))
		{
			TryBuy(hoveredIngredient.gameObject);
		}
	}

	private void RefreshCoins(bool force)
	{
		if (coinsText == null)
			return;

		if (!force && Time.unscaledTime < nextCoinsRefresh)
			return;

		nextCoinsRefresh = Time.unscaledTime + 0.15f;

		int coins = GameManager.Instance != null ? GameManager.Instance.Coins : 0;
		coinsText.text = coins.ToString();
	}

	public void ForceRefreshUI()
	{
		RefreshVisibility();
		RefreshCoins(force: true);
		StartCoroutine(RebuildCostLabelLayoutsAfterFrame());
	}

	private IEnumerator RebuildCostLabelLayoutsAfterFrame()
	{
		yield return null;
		RebuildCostLabelLayouts();
	}

	private void RebuildCosts()
	{
		costById.Clear();
		if (costOverrides == null)
			return;

		foreach (var o in costOverrides)
		{
			if (o == null || string.IsNullOrEmpty(o.ingredientId))
				continue;
			costById[o.ingredientId] = Mathf.Max(0, o.cost);
		}
	}

	private int GetCost(string ingredientId)
	{
		if (string.IsNullOrEmpty(ingredientId))
			return defaultCost;
		return costById.TryGetValue(ingredientId, out int c) ? c : defaultCost;
	}

	private void EnsureCostLabels()
	{
		foreach (GameObject go in GetIngredientObjectsUnderRoot())
		{
			if (go == null)
				continue;

			Transform existing = go.transform.Find(CostLabelRootName);
			if (existing != null && existing.Find(CostAmountChildName) == null)
				UnityEngine.Object.DestroyImmediate(existing.gameObject);
			else if (existing != null)
				continue;

			CreateCostLabelRow(go.transform);
		}
	}

	private void CreateCostLabelRow(Transform ingredientTransform)
	{
		var rootGo = new GameObject(CostLabelRootName);
		rootGo.transform.SetParent(ingredientTransform, false);

		var rootRect = rootGo.AddComponent<RectTransform>();
		rootRect.anchorMin = new Vector2(0.5f, 0f);
		rootRect.anchorMax = new Vector2(0.5f, 0f);
		rootRect.pivot = new Vector2(0.5f, 1f);
		rootRect.anchoredPosition = new Vector2(0f, costLabelOffsetY);
		rootRect.sizeDelta = costLabelSizeDelta;

		var layout = rootGo.AddComponent<HorizontalLayoutGroup>();
		layout.childAlignment = TextAnchor.MiddleCenter;
		layout.spacing = costLabelLayoutSpacing;
		layout.childControlWidth = false;
		layout.childControlHeight = false;
		layout.childForceExpandWidth = false;
		layout.childForceExpandHeight = false;

		var amountGo = new GameObject(CostAmountChildName);
		amountGo.transform.SetParent(rootGo.transform, false);
		var amountTmp = amountGo.AddComponent<TextMeshProUGUI>();
		amountTmp.raycastTarget = false;
		amountTmp.horizontalAlignment = HorizontalAlignmentOptions.Left;
		amountTmp.verticalAlignment = VerticalAlignmentOptions.Middle;
		var amountFit = amountGo.AddComponent<ContentSizeFitter>();
		amountFit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
		amountFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		var amountLe = amountGo.AddComponent<LayoutElement>();
		amountLe.minHeight = costLabelCoinIconSize.y;

		var coinGo = new GameObject(CostCoinChildName);
		coinGo.transform.SetParent(rootGo.transform, false);
		var coinLe = coinGo.AddComponent<LayoutElement>();
		coinLe.preferredWidth = costLabelCoinIconSize.x;
		coinLe.preferredHeight = costLabelCoinIconSize.y;
		var coinImg = coinGo.AddComponent<Image>();
		coinImg.raycastTarget = false;
		coinImg.preserveAspect = true;
		coinImg.color = Color.white;

		ApplyCostLabelTmpStyle(amountTmp);
	}

	private void ApplyCostLabelTmpStyle(TextMeshProUGUI tmp)
	{
		tmp.fontSize = costLabelPointSize;
		tmp.color = costLabelColor;
		tmp.spriteAsset = null;

		if (costLabelTypeface != null)
		{
			tmp.font = costLabelTypeface;
			if (costLabelFontMaterial != null)
				tmp.fontSharedMaterial = costLabelFontMaterial;
			else if (costLabelTypeface.material != null)
				tmp.fontSharedMaterial = costLabelTypeface.material;
		}
		else if (TMP_Settings.defaultFontAsset != null)
			tmp.font = TMP_Settings.defaultFontAsset;
	}

	private void RefreshCostLabels()
	{
		foreach (GameObject go in GetIngredientObjectsUnderRoot())
		{
			if (go == null || !go.activeInHierarchy)
				continue;

			Transform root = go.transform.Find(CostLabelRootName);
			if (root == null)
				continue;

			Transform amountT = root.Find(CostAmountChildName);
			if (amountT == null)
				continue;

			var amountTmp = amountT.GetComponent<TextMeshProUGUI>();
			if (amountTmp == null)
				continue;

			Transform coinT = root.Find(CostCoinChildName);
			Image coinImg = coinT != null ? coinT.GetComponent<Image>() : null;

			ApplyCostLabelTmpStyle(amountTmp);
			amountTmp.raycastTarget = false;

			var rootRect = root.GetComponent<RectTransform>();
			if (rootRect != null)
			{
				rootRect.anchorMin = new Vector2(0.5f, 0f);
				rootRect.anchorMax = new Vector2(0.5f, 0f);
				rootRect.pivot = new Vector2(0.5f, 1f);
				rootRect.anchoredPosition = new Vector2(0f, costLabelOffsetY);
				rootRect.sizeDelta = costLabelSizeDelta;
			}

			var layout = root.GetComponent<HorizontalLayoutGroup>();
			if (layout != null)
				layout.spacing = costLabelLayoutSpacing;

			if (coinImg != null)
			{
				coinImg.raycastTarget = false;
				if (costLabelCoinSprite != null)
				{
					coinImg.sprite = costLabelCoinSprite;
					coinImg.gameObject.SetActive(true);
				}
				else
					coinImg.gameObject.SetActive(false);

				var coinLe = coinImg.GetComponent<LayoutElement>();
				if (coinLe != null)
				{
					coinLe.preferredWidth = costLabelCoinIconSize.x;
					coinLe.preferredHeight = costLabelCoinIconSize.y;
				}
			}

			var amountLe = amountTmp.GetComponent<LayoutElement>();
			if (amountLe != null)
				amountLe.minHeight = costLabelCoinIconSize.y;

			int cost = GetCost(go.name);
			amountTmp.isRightToLeftText = false;
			amountTmp.text = cost.ToString();
			amountTmp.ForceMeshUpdate(true);
		}
	}

	private void RebuildCostLabelLayouts()
	{
		foreach (GameObject go in GetIngredientObjectsUnderRoot())
		{
			if (go == null || !go.activeInHierarchy)
				continue;

			Transform root = go.transform.Find(CostLabelRootName);
			if (root == null)
				continue;

			Transform amountT = root.Find(CostAmountChildName);
			var tmp = amountT != null ? amountT.GetComponent<TextMeshProUGUI>() : null;
			if (tmp != null)
				tmp.ForceMeshUpdate(true);

			var rootRect = root as RectTransform;
			if (rootRect != null)
				LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);
		}

		Canvas.ForceUpdateCanvases();
	}

	private void TryBuy(GameObject ingredientObject)
	{
		if (ingredientObject == null)
			return;

		var gm = GameManager.Instance;
		if (gm == null)
			return;

		string id = ingredientObject.name;
		if (gm.IsIngredientUnlocked(id))
			return;

		int cost = GetCost(id);
		if (cost > 0 && gm.Coins < cost)
		{
			if (AudioManager.Instance != null)
				AudioManager.Instance.PlayCantAfford();
			return;
		}

		gm.AddCoins(-cost);
		gm.TryUnlockIngredient(id);

		if (AudioManager.Instance != null)
			AudioManager.Instance.PlayRegisterChaChing();
		ShowCoinSpendPopup(cost);
		RefreshCoins(force: true);

		ingredientObject.SetActive(false);
	}

	private void ShowCoinSpendPopup(int amount)
	{
		if (coinSpendPopup == null || amount <= 0)
			return;

		if (spendPopupRoutine != null)
		{
			StopCoroutine(spendPopupRoutine);
			spendPopupRoutine = null;
		}

		coinSpendPopup.text = "-" + amount;
		coinSpendPopup.gameObject.SetActive(true);
		CanvasGroup cg = coinSpendPopup.GetComponent<CanvasGroup>();
		if (cg == null)
			cg = coinSpendPopup.gameObject.AddComponent<CanvasGroup>();
		cg.alpha = 1f;
		var rect = coinSpendPopup.rectTransform;
		Vector2 startPos = rect.anchoredPosition;
		spendPopupRoutine = StartCoroutine(FadeOutCoinSpendPopup(rect, startPos));
	}

	private void ResetCoinSpendPopup()
	{
		if (spendPopupRoutine != null)
		{
			StopCoroutine(spendPopupRoutine);
			spendPopupRoutine = null;
		}

		if (coinSpendPopup == null)
			return;

		var cg = coinSpendPopup.GetComponent<CanvasGroup>();
		if (cg != null)
			cg.alpha = 0f;

		var rect = coinSpendPopup.rectTransform;
		rect.anchoredPosition = coinSpendPopupBasePos;
		coinSpendPopup.gameObject.SetActive(false);
	}

	private IEnumerator FadeOutCoinSpendPopup(RectTransform rect, Vector2 startPos)
	{
		float elapsed = 0f;
		CanvasGroup cg = coinSpendPopup != null ? coinSpendPopup.GetComponent<CanvasGroup>() : null;
		while (elapsed < coinSpendFadeDuration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / coinSpendFadeDuration;
			if (cg != null)
				cg.alpha = 1f - t;
			rect.anchoredPosition = startPos + new Vector2(0f, coinSpendMoveUp * t);
			yield return null;
		}
		if (cg != null)
			cg.alpha = 0f;
		if (coinSpendPopup != null)
			coinSpendPopup.gameObject.SetActive(false);
		rect.anchoredPosition = startPos;
		spendPopupRoutine = null;
	}

	private void RefreshVisibility()
	{
		var gm = GameManager.Instance;

		foreach (GameObject go in GetIngredientObjectsUnderRoot())
		{
			bool unlocked = gm != null && gm.IsIngredientUnlocked(go.name);

			// Shop lists locked items; bought ones are removed from the layout.
			go.SetActive(!unlocked);
		}

		RefreshCostLabels();
		RebuildCostLabelLayouts();
	}

	private List<GameObject> GetIngredientObjectsUnderRoot()
	{
		var results = new List<GameObject>();
		if (shopRoot == null)
			return results;

		var images = shopRoot.GetComponentsInChildren<Image>(includeInactive: true);
		foreach (var img in images)
		{
			if (img != null && img.gameObject.CompareTag("Ingredients"))
				results.Add(img.gameObject);
		}
		return results;
	}

	private Image GetHoveredIngredient()
	{
		if (EventSystem.current == null)
			return null;

		var pointer = new PointerEventData(EventSystem.current)
		{
			position = Input.mousePosition
		};

		var results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointer, results);

		foreach (var r in results)
		{
			Transform t = r.gameObject.transform;
			while (t != null)
			{
				if (!t.CompareTag("Ingredients"))
				{
					t = t.parent;
					continue;
				}

				if (shopRoot != null && !t.IsChildOf(shopRoot))
					break;

				var img = t.GetComponent<Image>();
				if (img != null)
					return img;
				break;
			}
		}

		return null;
	}
}


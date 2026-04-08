using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
			if (!r.gameObject.CompareTag("Ingredients"))
				continue;

			// Ensure this ingredient is part of THIS shop panel.
			if (shopRoot != null && !r.gameObject.transform.IsChildOf(shopRoot))
				continue;

			var img = r.gameObject.GetComponent<Image>();
			if (img != null)
				return img;
		}

		return null;
	}
}


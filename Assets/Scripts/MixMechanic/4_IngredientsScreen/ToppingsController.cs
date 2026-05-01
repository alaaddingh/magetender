using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToppingsController : MonoBehaviour
{
    [Header("Data (Resources path, no extension)")]
    [SerializeField] private string toppingsResourcePath = "Data/Toppings";

    [Header("Hover Filter (use layer if it exists, else tag)")]
    [SerializeField] private string toppingsLayerName = "toppings";
    [SerializeField] private string toppingsTagName = "toppings";

    [Header("FIFO Remove (click bottle)")]
    [SerializeField] private Image toppingsBottle;

    [Header("Tooltip UI")]
    [SerializeField] private GameObject tooltipRoot;
    [SerializeField] private TextMeshProUGUI tooltipNameText;
    [SerializeField] private TextMeshProUGUI tooltipDescText;
    [SerializeField] private Vector2 tooltipOffset = new Vector2(16f, -16f);

    private readonly Dictionary<string, ToppingData> byId = new Dictionary<string, ToppingData>();
    private int toppingsLayer = -1;
    private GameObject lastHovered;
    private MixManager mixManager;
    private GameObject currentScreen;
    private GameObject nextScreen;
    private Button nextButton;

    private void Awake()
    {
        toppingsLayer = LayerMask.NameToLayer(toppingsLayerName);
        mixManager = FindFirstObjectByType<MixManager>();
        LoadToppingsData();
        HideTooltip();
    }

    private void OnEnable()
    {
        HookToppingEvents(true);
        RefreshNextButtonState();
    }

    private void OnDisable()
    {
        HookToppingEvents(false);
    }

    private void Update()
    {
        GameObject hovered = GetHoveredToppingUI();

        if (Input.GetMouseButtonDown(0) && ClickedToppingsBottle())
        {
            RemoveOldestTopping();
            return;
        }

        if (hovered == null)
        {
            lastHovered = null;
            HideTooltip();
            return;
        }

        if (hovered != lastHovered)
        {
            lastHovered = hovered;
            ShowTooltipFor(NormalizeId(hovered.name));
        }

        if (Input.GetMouseButtonDown(0))
            HandleToppingClick(hovered);

        MoveTooltipToMouse();
    }

    private void LoadToppingsData()
    {
        TextAsset json = Resources.Load<TextAsset>(toppingsResourcePath);
        ToppingsFile file = JsonUtility.FromJson<ToppingsFile>(json.text);

        byId.Clear();
        foreach (ToppingData t in file.toppings)
        {
            byId[NormalizeId(t.id)] = t;
        }
    }

    public void ConfigureNavigation(GameObject currentScreenObject, GameObject nextScreenObject, Button sharedNextButton)
    {
        currentScreen = currentScreenObject;
        nextScreen = nextScreenObject;
        nextButton = sharedNextButton;
        RefreshNextButtonState();
    }

    public void NextPressed()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        if (currentScreen != null)
            currentScreen.SetActive(false);
        if (nextScreen != null)
            nextScreen.SetActive(true);
    }

    private void HookToppingEvents(bool on)
    {
        if (mixManager == null)
            mixManager = FindFirstObjectByType<MixManager>();
        if (mixManager == null)
            return;

        if (on)
        {
            mixManager.OnToppingAdded -= OnToppingsChanged;
            mixManager.OnToppingRemoved -= OnToppingsChanged;
            mixManager.OnToppingAdded += OnToppingsChanged;
            mixManager.OnToppingRemoved += OnToppingsChanged;
        }
        else
        {
            mixManager.OnToppingAdded -= OnToppingsChanged;
            mixManager.OnToppingRemoved -= OnToppingsChanged;
        }
    }

    private void OnToppingsChanged(string _)
    {
        RefreshNextButtonState();
    }

    private void RefreshNextButtonState()
    {
        if (nextButton == null || mixManager == null)
            return;

        bool hasSelectedTopping = mixManager.SelectedToppings != null && mixManager.SelectedToppings.Count > 0;
        nextButton.gameObject.SetActive(hasSelectedTopping);
    }

    private static string NormalizeId(string raw)
    {
        return raw.Replace("(Clone)", string.Empty).Trim();
    }

    private GameObject GetHoveredToppingUI()
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);

        foreach (RaycastResult r in results)
        {
            GameObject top = FindToppingRoot(r.gameObject);
            if (top != null)
                return top;
        }

        return null;
    }

    private GameObject FindToppingRoot(GameObject hit)
    {
        Transform t = hit.transform;
        while (t != null)
        {
            if (IsToppingObject(t.gameObject))
                return t.gameObject;
            t = t.parent;
        }

        return null;
    }

    private bool IsToppingObject(GameObject go)
    {
        bool layerMatch = toppingsLayer >= 0 && go.layer == toppingsLayer;
        bool tagMatch = string.Equals(go.tag, toppingsTagName, StringComparison.OrdinalIgnoreCase);
        return layerMatch || tagMatch;
    }

    private void ShowTooltipFor(string id)
    {
        tooltipRoot.SetActive(true);

        string key = NormalizeId(id);
        ToppingData data = byId[key];

        tooltipNameText.text = data.name ?? data.id ?? string.Empty;
        tooltipDescText.text = data.description ?? string.Empty;
    }

    private void HandleToppingClick(GameObject toppingGo)
    {
        string key = NormalizeId(toppingGo.name);
        ToppingData data = byId[key];
        if (mixManager.SelectedToppings.Contains(key))
            return;

		if (mixManager.SelectedToppings.Count > 0)
		{
			string[] previouslySelected = mixManager.SelectedToppings.ToArray();
			for (int i = 0; i < previouslySelected.Length; i++)
			{
				string oldKey = NormalizeId(previouslySelected[i]);
				mixManager.RemoveTopping(oldKey);
				if (byId.TryGetValue(oldKey, out ToppingData oldData))
					SetSelectSpriteActive(oldData, false);
			}
		}

        bool added = mixManager.AddTopping(key);
        if (!added)
            return;

		if (AudioManager.Instance != null)
			AudioManager.Instance.PlayIngredientClick();

        SetSelectSpriteActive(data, true);
    }

    private bool ClickedToppingsBottle()
    {
        if (toppingsBottle == null) return false;
        Canvas c = toppingsBottle.canvas;
        Camera cam = c != null ? c.worldCamera : null;
        return RectTransformUtility.RectangleContainsScreenPoint(toppingsBottle.rectTransform, Input.mousePosition, cam);
    }

    private void RemoveOldestTopping()
    {
        if (mixManager.SelectedToppings.Count == 0) return;
        string removedKey = mixManager.SelectedToppings[mixManager.SelectedToppings.Count - 1];
        mixManager.RemoveTopping(removedKey);
        SetSelectSpriteActive(byId[removedKey], false);

		if (AudioManager.Instance != null)
			AudioManager.Instance.PlayIngredientClick();
    }

    private void SetSelectSpriteActive(ToppingData data, bool active)
    {
        Image img = FindImageByNameIncludingInactive(data.select_sprite);
        img.gameObject.SetActive(active);
    }

    private static Image FindImageByNameIncludingInactive(string goName)
    {
        foreach (Image img in Resources.FindObjectsOfTypeAll<Image>())
        {
            if (!img.gameObject.scene.IsValid() || !img.gameObject.scene.isLoaded) continue;
            if (string.Equals(img.gameObject.name, goName, StringComparison.Ordinal))
                return img;
        }

        return null;
    }

    private void HideTooltip()
    {
        tooltipRoot.SetActive(false);
    }

    private void MoveTooltipToMouse()
    {
        RectTransform rt = tooltipRoot.GetComponent<RectTransform>();
        rt.position = (Vector2)Input.mousePosition + tooltipOffset;
    }

    [Serializable]
    private class ToppingsFile
    {
        public List<ToppingData> toppings;
    }

    [Serializable]
    private class ToppingData
    {
        public string id;
        public string name;
        public string description;
        public string select_sprite;
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class IngredientHoverSnapUI : MonoBehaviour
{
    [Header("target position")]
    public Vector2 snapTargetPos;

    [Header("tooltip ui")]
    public GameObject tooltipRoot;
    public TextMeshProUGUI tooltipNameText;
    public TextMeshProUGUI tooltipDescText;
    public TextMeshProUGUI tooltipEffectText;
    public Vector2 tooltipOffset = new Vector2(16f, -16f);

    [Header("debug")]
    public bool debugLogs = true;
    public TextMeshProUGUI debugOverlayText;

    private Image hoveredIngredient;
    private MixManager mixManager;
    private Dictionary<string, Vector2> shelfPositions = new Dictionary<string, Vector2>();

    private Dictionary<string, IngredientData> byId = new Dictionary<string, IngredientData>();

    [Tooltip("Max distance from snap target to consider ingredient 'in drink' for toggle-off")]
    public float inDrinkThreshold = 15f;

    private static bool s_processedClickThisFrame;
    private float nextDebugTime = 0f;
    private const float debugInterval = 0.25f;

    private void Awake()
    {
        LoadIngredientData();
        HideTooltip();
        mixManager = FindFirstObjectByType<MixManager>();
        RecordShelfPositions();
    }

    private void RecordShelfPositions()
    {
        shelfPositions.Clear();
        GameObject[] ingredients = GameObject.FindGameObjectsWithTag("Ingredients");
        foreach (GameObject go in ingredients)
        {
            Image img = go.GetComponent<Image>();
            if (img != null && !string.IsNullOrEmpty(go.name))
                shelfPositions[go.name] = img.rectTransform.anchoredPosition;
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
                    hoveredIngredient.rectTransform.anchoredPosition = GetShelfPosition(hoveredIngredient.name);
                }
            }
            else
            {
                if (!shelfPositions.ContainsKey(hoveredIngredient.name))
                    shelfPositions[hoveredIngredient.name] = hoveredIngredient.rectTransform.anchoredPosition;
                hoveredIngredient.rectTransform.anchoredPosition = snapTargetPos;
                mixManager.AddIngredient(hoveredIngredient.name);
            }
            s_processedClickThisFrame = true;
        }
    }

    private bool IsInDrink(Image img)
    {
        Vector2 pos = img.rectTransform.anchoredPosition;
        return (pos - snapTargetPos).sqrMagnitude <= inDrinkThreshold * inDrinkThreshold;
    }

    private Vector2 GetShelfPosition(string ingredientName)
    {
        return shelfPositions.TryGetValue(ingredientName, out Vector2 pos) ? pos : snapTargetPos;
    }

    private void DebugTick(string line)
    {
        if (!debugLogs) return;

        if (Time.unscaledTime < nextDebugTime) return;
        nextDebugTime = Time.unscaledTime + debugInterval;

        string msg = line;

        if (debugOverlayText != null)
        {
            debugOverlayText.text =
                $"eventsystem null? {(EventSystem.current == null)}\n" +
                $"{line}";
        }
    }

    private void LoadIngredientData()
    {
        TextAsset json = Resources.Load<TextAsset>("Data/Ingredients");
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
            if (tooltipNameText != null) tooltipNameText.text = id;
            if (tooltipDescText != null) tooltipDescText.text = "(no json match)";
            if (tooltipEffectText != null) tooltipEffectText.text = "";

            DebugTick($"json lookup failed for id: {id}");
            return;
        }

        if (tooltipNameText != null) tooltipNameText.text = data.name;
        if (tooltipDescText != null) tooltipDescText.text = data.description;

        float x = 0f;
        float y = 0f;
        if (data.effect != null)
        {
            x = data.effect.x;
            y = data.effect.y;
        }

        if (tooltipEffectText != null)
        {
            tooltipEffectText.text =
                $"x: {x:+0.00;-0.00} (grounded↔dissociative)\n" +
                $"y: {y:+0.00;-0.00} (calm↕elevated)";
        }
    }

    private void HideTooltip()
    {
        if (tooltipRoot != null)
            tooltipRoot.SetActive(false);
    }

    private void MoveTooltipToMouse()
    {
        if (tooltipRoot == null) return;

        RectTransform rt = tooltipRoot.GetComponent<RectTransform>();
        if (rt != null)
            rt.position = (Vector2)Input.mousePosition + tooltipOffset;
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
}
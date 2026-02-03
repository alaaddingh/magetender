/* rly simple for now, when user clicks on UI image tagged as "Ingredients", relocates image to new XY and adds Ingredients
to mixmanager's storage */
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class IngredientHoverSnapUI : MonoBehaviour
{
    /* relocate ingredients after click */
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
    public TextMeshProUGUI debugOverlayText; //optional: put a tmp text on canvas and drag it here

    private Image hoveredIngredient;

    private Dictionary<string, IngredientData> byId = new Dictionary<string, IngredientData>();

    //debug throttling
    private float nextDebugTime = 0f;
    private const float debugInterval = 0.25f;

    private void Awake()
    {
        LoadIngredientData();
        HideTooltip();

        if (debugLogs)
        {
            Debug.Log("ingredienthoversnapui awake");
            Debug.Log("eventsystem current null? " + (EventSystem.current == null));
            Debug.Log("tooltiproot assigned? " + (tooltipRoot != null));
            Debug.Log("tooltip name text assigned? " + (tooltipNameText != null));
            Debug.Log("tooltip desc text assigned? " + (tooltipDescText != null));
            Debug.Log("tooltip effect text assigned? " + (tooltipEffectText != null));
        }
    }

    void Update()
    {
        hoveredIngredient = GetHoveredIngredient();

        //hover tooltip
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

        if (Input.GetMouseButtonDown(0) && hoveredIngredient != null)
        {
            hoveredIngredient.rectTransform.anchoredPosition = snapTargetPos;

            /* add ingredient to mixmanager */ 
            var mixManager = FindFirstObjectByType<MixManager>();
            if (mixManager != null)
                mixManager.AddIngredient(hoveredIngredient.name);
        }
    }

    private void DebugTick(string line)
    {
        if (!debugLogs) return;

        //throttle log spam
        if (Time.unscaledTime < nextDebugTime) return;
        nextDebugTime = Time.unscaledTime + debugInterval;

        string msg = line;

        //overlay (optional)
        if (debugOverlayText != null)
        {
            debugOverlayText.text =
                $"eventsystem null? {(EventSystem.current == null)}\n" +
                $"{line}";
        }

        //console
        Debug.Log(msg);
    }

    private void LoadIngredientData()
    {
        TextAsset json = Resources.Load<TextAsset>("Data/Ingredients");
        if (json == null)
        {
            Debug.LogError("could not find Ingredients.json in Assets/Resources/Data");
            return;
        }

        IngredientsFile file = JsonUtility.FromJson<IngredientsFile>(json.text);
        if (file == null || file.ingredients == null)
        {
            Debug.LogError("failed to parse Ingredients.json");
            return;
        }

        byId.Clear();
        foreach (var ing in file.ingredients)
        {
            if (!string.IsNullOrEmpty(ing.id))
                byId[ing.id] = ing;
        }

        if (debugLogs)
            Debug.Log("ingredients loaded: " + byId.Count);
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
            //if lookup fails, still show something so we know hover worked
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

        //debug what we're actually hitting (top-most ui element wins)
        if (debugLogs && Time.unscaledTime >= nextDebugTime)
        {
            Debug.Log("raycast hits: " + results.Count);
            if (results.Count > 0)
                Debug.Log("top hit: " + results[0].gameObject.name + " tag=" + results[0].gameObject.tag);
        }

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
using UnityEngine;

public class MoodGraphPositioning : MonoBehaviour
{
    public MoodGraphUI MoodGraph;

    public GameObject OrderScreen;
    public GameObject ToppingsScreen;
    public GameObject BaseScreen;
    public GameObject IngredientsScreen;
    public GameObject AssessScreen;

    [System.Serializable]
    public class PanelLayout
    {
        [Header("Anchor Presets")]
        public Vector2 AnchorMin = new Vector2(0.5f, 0.5f);
        public Vector2 AnchorMax = new Vector2(0.5f, 0.5f);
        public Vector2 Pivot = new Vector2(0.5f, 0.5f);

        [Header("Offset")]
        public Vector2 AnchoredPosition;

        [Header("Scale")]
        public float Scale = 1f;
    }

    [Header("Order Screen")]
    public PanelLayout OrderLayout;

    [Header("Toppings Screen")]
    public PanelLayout ToppingsLayout;

    [Header("Base Screen")]
    public PanelLayout BaseLayout;

    [Header("Ingredients Screen")]
    public PanelLayout IngredientsLayout;

    [Header("Assess Screen")]
    public PanelLayout AssessLayout;

    private void Start()
    {
        Hook(OrderScreen, "Order");
        Hook(ToppingsScreen, "Toppings");
        Hook(BaseScreen, "Base");
        Hook(IngredientsScreen, "Ingredients");
        Hook(AssessScreen, "Assess");

        PlaceActive();
    }

    public void PanelOn(string id)
    {
        if (MoodGraph == null) return;

        RectTransform rt = MoodGraph.transform as RectTransform;

        switch (id)
        {
            case "Order":
                ApplyLayout(rt, OrderLayout);
                break;

            case "Toppings":
                ApplyLayout(rt, ToppingsLayout);
                break;

            case "Base":
                ApplyLayout(rt, BaseLayout);
                break;

            case "Ingredients":
                ApplyLayout(rt, IngredientsLayout);
                break;

            case "Assess":
                ApplyLayout(rt, AssessLayout);
                break;
        }
    }

    private void Hook(GameObject panel, string id)
    {
        if (panel == null) return;

        PanelOnRelay relay =
            panel.GetComponent<PanelOnRelay>() ??
            panel.AddComponent<PanelOnRelay>();

        relay.Target = this;
        relay.Id = id;
    }

    private void PlaceActive()
    {
        if (OrderScreen != null && OrderScreen.activeInHierarchy)
        {
            PanelOn("Order");
            return;
        }

        if (ToppingsScreen != null && ToppingsScreen.activeInHierarchy)
        {
            PanelOn("Toppings");
            return;
        }

        if (BaseScreen != null && BaseScreen.activeInHierarchy)
        {
            PanelOn("Base");
            return;
        }

        if (IngredientsScreen != null && IngredientsScreen.activeInHierarchy)
        {
            PanelOn("Ingredients");
            return;
        }

        if (AssessScreen != null && AssessScreen.activeInHierarchy)
        {
            PanelOn("Assess");
        }
    }

    private void ApplyLayout(RectTransform rt, PanelLayout layout)
    {
        if (layout == null) return;

        rt.anchorMin = layout.AnchorMin;
        rt.anchorMax = layout.AnchorMax;
        rt.pivot = layout.Pivot;

        rt.anchoredPosition = layout.AnchoredPosition;
        rt.localScale = Vector3.one * layout.Scale;
    }
}

public class PanelOnRelay : MonoBehaviour
{
    public MoodGraphPositioning Target;
    public string Id;

    private void OnEnable()
    {
        if (Target != null)
            Target.PanelOn(Id);
    }
}
using UnityEngine;

public class MoodGraphPositioning : MonoBehaviour
{
    public MoodGraphUI MoodGraph;

    public GameObject OrderScreen;
    public GameObject PickGlassScreen;
    public GameObject BaseScreen;
    public GameObject IngredientsScreen;
    public GameObject AssessScreen;

    [Header("Order Screen pos.")]
    public float OrderX;
    public float OrderY;
    public float OrderScale = 1f;

    [Header("Pick Glass Screen pos.")]
    public float PickGlassX;
    public float PickGlassY;
    public float PickGlassScale = 1f;

    [Header("Base Screen pos.")]
    public float BaseX;
    public float BaseY;
    public float BaseScale = 1f;

    [Header("Ingredients Screen pos.")]
    public float IngredientsX;
    public float IngredientsY;
    public float IngredientsScale = 1f;

    [Header("Assess Screen pos.")]
    public float AssessX;
    public float AssessY;
    public float AssessScale = 1f;

    private void Start()
    {
        Hook(OrderScreen, "Order");
        Hook(PickGlassScreen, "PickGlass");
        Hook(BaseScreen, "Base");
        Hook(IngredientsScreen, "Ingredients");
        Hook(AssessScreen, "Assess");

        PlaceActive();
    }

    public void PanelOn(string id)
    {
        if (MoodGraph == null) return;
        var rt = MoodGraph.transform as RectTransform;

        if (id == "Order")
        {
            Place(rt, OrderX, OrderY, OrderScale);
            return;
        }

        if (id == "PickGlass")
        {
            Place(rt, PickGlassX, PickGlassY, PickGlassScale);
            return;
        }

        if (id == "Base")
        {
            Place(rt, BaseX, BaseY, BaseScale);
            return;
        }

        if (id == "Ingredients")
        {
            Place(rt, IngredientsX, IngredientsY, IngredientsScale);
            return;
        }

        if (id == "Assess")
            Place(rt, AssessX, AssessY, AssessScale);
    }

    private void Hook(GameObject panel, string id)
    {
        if (panel == null) return;
        var r = panel.GetComponent<PanelOnRelay>() ?? panel.AddComponent<PanelOnRelay>();
        r.Target = this;
        r.Id = id;
    }


    private void PlaceActive()
    {
        if (OrderScreen != null && OrderScreen.activeInHierarchy) 
            { PanelOn("Order"); return; }
        if (PickGlassScreen != null && PickGlassScreen.activeInHierarchy) 
            { PanelOn("PickGlass"); return; }
        if (BaseScreen != null && BaseScreen.activeInHierarchy) 
            { PanelOn("Base"); return; }
        if (IngredientsScreen != null && IngredientsScreen.activeInHierarchy) 
            { PanelOn("Ingredients"); return; }
        if (AssessScreen != null && AssessScreen.activeInHierarchy) 
           { PanelOn("Assess"); }
    }

    private void Place(RectTransform rt, float x, float y, float s)
    {
        rt.anchoredPosition = new Vector2(x, y);
        rt.localScale = Vector3.one * s;
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

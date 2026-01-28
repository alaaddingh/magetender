using UnityEngine;
using UnityEngine.UI;

/* purpose of script: retrieves selected glass from MixManager and displays it on Ingredients */

public class IngredientController : MonoBehaviour
{
    [Header("Target UI Image")]
    public Image GlassBottle;   /* this is the one shown to the player */

    [Header("Source UI Images (already in scene)")]
    public Image smallBottleUI;
    public Image mediumBottleUI;
    public Image largeBottleUI;

    [Header("Manager")]
    [SerializeField] private MixManager mixManager;


    void Start()
    {
        ApplyBottle();
    }

    void ApplyBottle()
    {
        switch (mixManager.SelectedBottle)
        {
            case "small":
                CopyImage(smallBottleUI);
                break;

            case "medium":
                CopyImage(mediumBottleUI);
                break;

            case "large":
                CopyImage(largeBottleUI);
                break;

            default:
                break;
        }
    }

    void CopyImage(Image source)
    {

        GlassBottle.sprite = source.sprite;
        GlassBottle.color = source.color;
        GlassBottle.material = source.material;
        GlassBottle.type = source.type;
        GlassBottle.preserveAspect = source.preserveAspect;

        /* match rect transform so it looks identical */
        RectTransform srcRT = source.rectTransform;
        RectTransform dstRT = GlassBottle.rectTransform;

        dstRT.anchorMin = srcRT.anchorMin;
        dstRT.anchorMax = srcRT.anchorMax;
        dstRT.pivot     = srcRT.pivot;
        dstRT.sizeDelta = srcRT.sizeDelta;
        dstRT.localScale = srcRT.localScale;
    }
}

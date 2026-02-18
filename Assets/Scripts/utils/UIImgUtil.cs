using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;


/* util for copying visual appearance + scale of Image */
/* update: since we have two seperate assets for glass bottle (sealed/empty)
this utility does not copy over src.sprite anymore */
public static class UIImgUtil
{
    public static void CopyAppearance(Image src, Image dst)
    {
        Debug.Log("image copied over!");
        dst.sprite = src.sprite;
        dst.color = src.color;
        dst.material = src.material;
        dst.type = src.type;
        dst.preserveAspect = src.preserveAspect;

        dst.rectTransform.localScale = src.rectTransform.localScale;

    }
}

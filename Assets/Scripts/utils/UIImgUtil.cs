using UnityEngine.UI;

/* util for copying visual appearance + scale of Image */
public static class UIImgUtil
{
    public static void CopyAppearance(Image src, Image dst)
    {

        dst.sprite = src.sprite;
        dst.color = src.color;
        dst.material = src.material;
        dst.type = src.type;
        dst.preserveAspect = src.preserveAspect;

        dst.rectTransform.localScale = src.rectTransform.localScale;

    }
}

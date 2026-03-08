using UnityEngine;
using System.Collections.Generic;

public static class FillCircleSprite
{
    private static Dictionary<float, Sprite> s_cache = new Dictionary<float, Sprite>();

    public static Sprite Get(float yCutoffNormalized = 0.5f)
    {
        float key = Mathf.Round(yCutoffNormalized * 100f) / 100f;
        if (s_cache.TryGetValue(key, out Sprite cached)) return cached;
        const int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0f, 0f, 0f, 0f);
        Color white = Color.white;
        float cx = (size - 1) * 0.5f;
        float r = cx - 1f;
        float yCutoff = (size - 1) * Mathf.Clamp01(yCutoffNormalized);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx;
                float dy = y - (size - 1) * 0.5f;
                bool inCircle = dx * dx + dy * dy <= r * r;
                bool belowCutoff = y <= yCutoff;
                tex.SetPixel(x, y, (inCircle && belowCutoff) ? white : clear);
            }
        tex.Apply();
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0f));
        s_cache[key] = sprite;
        return sprite;
    }
}

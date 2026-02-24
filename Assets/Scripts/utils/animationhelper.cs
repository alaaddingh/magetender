using UnityEngine;


/* helper file that:
1) bobs character if in any state but angry
2) Makes character jitter if angry

is imported into MonsterSpriteManager.cs
*/

public static class AnimationHelper
{
    public static Vector2 GetBobOffset(float time, float amplitude, float frequency)
    {
        float wave = Mathf.Sin(time * frequency * Mathf.PI * 2f);
        return new Vector2(0f, wave * amplitude);
    }

    public static Vector2 GetJitterOffset(float time, float amplitude, float frequency)
    {
        float x = (Mathf.PerlinNoise(time * frequency, 0f) - 0.5f) * 2f * amplitude;
        float y = (Mathf.PerlinNoise(0f, (time * frequency) + 73.129f) - 0.5f) * 2f * amplitude;
        return new Vector2(x, y);
    }
}

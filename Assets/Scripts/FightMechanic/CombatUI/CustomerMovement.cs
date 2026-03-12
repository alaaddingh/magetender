using UnityEngine;

// Makes customer sprite shake constantly in anger during combat
// Uses same jitter animation as MonsterSpriteManager for consistency
public class CustomerMovement : MonoBehaviour
{
    [Header("Shake Settings")]
    public float jitterAmplitude = 3f; // How far to shake
    public float jitterFrequency = 15f; // How fast to shake
    
    private RectTransform rectTransform;
    private Vector2 basePosition;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (rectTransform != null)
        {
            basePosition = rectTransform.anchoredPosition;
        }
    }
    
    void Update()
    {
        if (rectTransform == null) return;
        
        // Use same jitter method as MonsterSpriteManager
        Vector2 offset = AnimationHelper.GetJitterOffset(Time.time, jitterAmplitude, jitterFrequency);
        
        rectTransform.anchoredPosition = basePosition + offset;
    }
    
    // Call this to pause/resume shaking
    public void SetEnabled(bool enabled)
    {
        this.enabled = enabled;
        
        // Reset to base position when disabled
        if (!enabled && rectTransform != null)
        {
            rectTransform.anchoredPosition = basePosition;
        }
    }
    
    // Update base position (useful after other animations move the sprite)
    public void UpdateBasePosition()
    {
        if (rectTransform != null)
        {
            basePosition = rectTransform.anchoredPosition;
        }
    }
}
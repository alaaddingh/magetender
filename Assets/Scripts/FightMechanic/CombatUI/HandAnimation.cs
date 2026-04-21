using UnityEngine;
using System.Collections;

// Idle floating animation for player hands
// Creates smooth circular/elliptical motion
public class HandAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public float radiusX = 20f; // Horizontal movement distance
    public float radiusY = 15f; // Vertical movement distance
    public float speed = 1f; // Speed of rotation
    public float startAngle = 0f; // Starting position in circle (0-360)
    
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private float currentAngle;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (rectTransform != null)
        {
            // Store original position as center point
            originalPosition = rectTransform.anchoredPosition;
            
            // Set starting angle (converted to radians)
            currentAngle = startAngle * Mathf.Deg2Rad;
        }
    }
    
    void Update()
    {
        if (rectTransform == null) return;
        
        // Increment angle over time (clockwise = negative)
        currentAngle -= speed * Time.deltaTime;
        
        // Calculate offset from original position using circular motion
        float offsetX = Mathf.Cos(currentAngle) * radiusX;
        float offsetY = Mathf.Sin(currentAngle) * radiusY;
        
        // Apply to position
        rectTransform.anchoredPosition = originalPosition + new Vector2(offsetX, offsetY);
    }
    
    // Call this to reset to original position (used during attack animations)
    public void ResetToOriginal()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }
    
    // Call this to pause/resume animation
    public void SetEnabled(bool enabled)
    {
        this.enabled = enabled;
    }
}
using UnityEngine;
using UnityEngine.UI;

// Provides visual feedback for which bank is active
// Pulsating scale and glow effects
public class BankVisualFeedback : MonoBehaviour
{
	[Header("References")]
	public RectTransform bankTransform;
	public Image glowImage;  // Optional - can leave empty
	
	[Header("Pulse Settings")]
	public bool enablePulse = true;
	public float pulseSpeed = 2f;
	public float minScale = 0.95f;
	public float maxScale = 1.05f;
	
	[Header("Glow Settings")]
	public Color activeGlowColor = new Color(1f, 1f, 0.3f, 1f);
	public Color inactiveGlowColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
	public float glowPulseIntensity = 0.3f;
	
	private bool isActive = false;
	private Vector3 originalScale;
	
	void Start()
	{
		if (bankTransform != null)
		{
			originalScale = bankTransform.localScale;
		}
	}
	
	void Update()
	{
		if (isActive && enablePulse)
		{
			PulseEffect();
			
			// Only pulse glow if it exists
			if (glowImage != null)
			{
				GlowPulseEffect();
			}
		}
	}
	
	// Set whether this bank is active
	public void SetActive(bool active)
	{
		isActive = active;
		
		// Update glow color (optional)
		if (glowImage != null)
		{
			glowImage.color = active ? activeGlowColor : inactiveGlowColor;
		}
		
		// Reset scale if inactive
		if (!active && bankTransform != null)
		{
			bankTransform.localScale = originalScale;
		}
	}
	
	// Pulsating scale effect
	void PulseEffect()
	{
		if (bankTransform == null) return;
		
		// PingPong creates a smooth back-and-forth value
		float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
		
		// Smooth the transition with easing
		float smoothT = Mathf.SmoothStep(0f, 1f, t);
		
		// Lerp between min and max scale
		float scale = Mathf.Lerp(minScale, maxScale, smoothT);
		bankTransform.localScale = originalScale * scale;
	}
	
	// Pulsating glow effect (optional)
	void GlowPulseEffect()
	{
		if (glowImage == null) return;
		
		// Use PingPong for smooth back-and-forth
		float t = Mathf.PingPong(Time.time * pulseSpeed * 0.75f, 1f);  // Slightly slower than scale
		float smoothT = Mathf.SmoothStep(0f, 1f, t);
		
		Color baseColor = activeGlowColor;
		Color brightColor = activeGlowColor * (1f + glowPulseIntensity);
		glowImage.color = Color.Lerp(baseColor, brightColor, smoothT);
	}
}
using UnityEngine;
using UnityEngine.UI;

// Manages health bar fill (no text, just visual bar)
public class HealthBarUI : MonoBehaviour
{
	[Header("UI References")]
	public Image fillImage;  // The colored bar that drains
	
	[Header("Settings")]
	public Color fullHealthColor = new Color(0.3f, 1f, 0.3f);  // Green
	public Color midHealthColor = new Color(1f, 1f, 0.3f);     // Yellow
	public Color lowHealthColor = new Color(1f, 0.3f, 0.3f);   // Red
	public float lowHealthThreshold = 0.3f;  // 30%
	public float midHealthThreshold = 0.6f;  // 60%
	
	private int maxHealth;
	
	public void Initialize(int max)
	{
		maxHealth = max;
		UpdateHealth(max);
	}
	
	public void UpdateHealth(int current)
	{
		float fillPercent = (float)current / maxHealth;
		
		// Update fill bar
		if (fillImage != null)
		{
			fillImage.fillAmount = Mathf.Clamp01(fillPercent);
			
			// Change color based on health
			if (fillPercent <= lowHealthThreshold)
			{
				fillImage.color = lowHealthColor;
			}
			else if (fillPercent <= midHealthThreshold)
			{
				fillImage.color = midHealthColor;
			}
			else
			{
				fillImage.color = fullHealthColor;
			}
		}
	}
}
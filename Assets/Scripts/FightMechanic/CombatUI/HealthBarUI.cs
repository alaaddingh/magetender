using UnityEngine;
using UnityEngine.UI;

// Manages health bar fill using width 
public class HealthBarUI : MonoBehaviour
{
	[Header("UI References")]
	public RectTransform fillRect;  // The colored bar RectTransform
	public Image fillImage;         // The colored bar Image (for color changes)
	
	[Header("Settings")]
	public Color fullHealthColor = new Color(0.3f, 1f, 0.3f);  // Green
	public Color midHealthColor = new Color(1f, 1f, 0.3f);     // Yellow
	public Color lowHealthColor = new Color(1f, 0.3f, 0.3f);   // Red
	public float lowHealthThreshold = 0.3f;  
	public float midHealthThreshold = 0.6f; 
	
	private int maxHealth;
	private float maxWidth;
	
	void Start()
	{
		// Store the max width (full health width)
		if (fillRect != null)
		{
			maxWidth = fillRect.rect.width;
		}
	}
	
	public void Initialize(int max)
	{
		maxHealth = max;
		
		// Store max width if not already set
		if (fillRect != null && maxWidth == 0)
		{
			maxWidth = fillRect.rect.width;
		}
		
		UpdateHealth(max);
	}
	
	public void UpdateHealth(int current)
	{
		float fillPercent = (float)current / maxHealth;
		
		// Update bar width
		if (fillRect != null)
		{
			// Change the Right anchor position to shrink the bar
			fillRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, maxWidth * fillPercent);
			
			// Change color based on health
			if (fillImage != null)
			{
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
}
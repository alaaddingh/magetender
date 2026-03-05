using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Creates spotlight effect - darkens everything except target element
public class TutorialSpotlight : MonoBehaviour
{
	[Header("Overlay Settings")]
	public Image darkOverlay;  // Full screen dark overlay
	public Color overlayColor = new Color(0, 0, 0, 0.8f);  // 80% black
	
	[Header("Animation")]
	public float fadeInDuration = 0.3f;
	public float fadeOutDuration = 0.3f;
	
	private GameObject currentSpotlightTarget;
	private CanvasGroup targetCanvasGroup;
	
	void Start()
	{
		// Start with overlay hidden
		if (darkOverlay != null)
		{
			darkOverlay.enabled = false;
		}
	}
	
	// Show spotlight on specific element (darkens everything else)
	public void ShowSpotlightOn(GameObject target)
	{
		currentSpotlightTarget = target;
		
		// Enable overlay
		if (darkOverlay != null)
		{
			darkOverlay.enabled = true;
			StartCoroutine(FadeOverlay(0f, overlayColor.a));
		}
		
		// Ensure target is on top and fully visible
		if (target != null)
		{
			// Bring target to front
			target.transform.SetAsLastSibling();
			
			// Make sure it's fully opaque
			targetCanvasGroup = target.GetComponent<CanvasGroup>();
			if (targetCanvasGroup == null)
			{
				targetCanvasGroup = target.AddComponent<CanvasGroup>();
			}
			targetCanvasGroup.alpha = 1f;
			targetCanvasGroup.blocksRaycasts = true;
		}
	}
	
	// Hide spotlight (remove dark overlay)
	public void HideSpotlight()
	{
		if (darkOverlay != null)
		{
			StartCoroutine(FadeOverlay(overlayColor.a, 0f, () => 
			{
				darkOverlay.enabled = false;
			}));
		}
		
		currentSpotlightTarget = null;
	}
	
	// Fade overlay alpha
	IEnumerator FadeOverlay(float from, float to, System.Action onComplete = null)
	{
		float elapsed = 0f;
		float duration = from < to ? fadeInDuration : fadeOutDuration;
		
		while (elapsed < duration)
		{
			float t = elapsed / duration;
			Color color = darkOverlay.color;
			color.a = Mathf.Lerp(from, to, t);
			darkOverlay.color = color;
			
			elapsed += Time.deltaTime;
			yield return null;
		}
		
		// Ensure final value
		Color finalColor = darkOverlay.color;
		finalColor.a = to;
		darkOverlay.color = finalColor;
		
		onComplete?.Invoke();
	}
}
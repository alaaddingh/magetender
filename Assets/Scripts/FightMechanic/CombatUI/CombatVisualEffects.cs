using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Handles visual effects for combat (healing, attacks, etc.)
public class CombatVisualEffects : MonoBehaviour
{
    [Header("Healing Effect")]
    public Sprite healingCenterSprite;
    public Sprite healingPlusSprite;
    public float healingDuration = 1f;
    public int healingPlusCount = 8;
    public float healingSpreadRadius = 100f;
    public float healingCenterSize = 80f;
    public float healingPlusSize = 30f;
    public float healingPlusSizeVariation = 10f;
    
    [Header("References")]
    public Canvas mainCanvas;
    public RectTransform leftHand;
    
    public void PlayHealEffect()
    {
        Debug.Log("PlayHealEffect called");

        if (leftHand != null && mainCanvas != null)
        {
            Debug.Log("Starting heal effect coroutine");
            StartCoroutine(HealEffectCoroutine());
        }
        else
        {
            Debug.LogWarning("Cannot play heal effect - missing references");
        }
    }
    
    IEnumerator HealEffectCoroutine()
    {
        // Create center swirl
        GameObject centerObj = new GameObject("HealingCenter");
        centerObj.transform.SetParent(mainCanvas.transform, false);
        
        Image centerImage = centerObj.AddComponent<Image>();
        centerImage.sprite = healingCenterSprite;
        centerImage.raycastTarget = false;
        
        RectTransform centerRect = centerObj.GetComponent<RectTransform>();
        centerRect.sizeDelta = new Vector2(healingCenterSize, healingCenterSize);
        centerRect.position = leftHand.position;

        // Set to render behind hand
        int handIndex = leftHand.GetSiblingIndex();
        centerRect.SetSiblingIndex(handIndex);

        // Create plus symbols
        GameObject[] pluses = new GameObject[healingPlusCount];
        RectTransform[] plusRects = new RectTransform[healingPlusCount];
        Vector2[] plusDirections = new Vector2[healingPlusCount];
        
        for (int i = 0; i < healingPlusCount; i++)
        {
            GameObject plusObj = new GameObject($"HealingPlus_{i}");
            plusObj.transform.SetParent(mainCanvas.transform, false);
            
            Image plusImage = plusObj.AddComponent<Image>();
            plusImage.sprite = healingPlusSprite;
            plusImage.raycastTarget = false;

            RectTransform plusRect = plusObj.GetComponent<RectTransform>();

            // Randomize size slightly
            float randomSize = healingPlusSize + Random.Range(-healingPlusSizeVariation, healingPlusSizeVariation);
            plusRect.sizeDelta = new Vector2(randomSize, randomSize);

            plusRect.position = leftHand.position;
            plusRect.SetSiblingIndex(handIndex);
            plusRect.sizeDelta = new Vector2(healingPlusSize, healingPlusSize);
            plusRect.position = leftHand.position;
            
            // Calculate spread direction
            float angle = (360f / healingPlusCount) * i * Mathf.Deg2Rad;
            plusDirections[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            
            pluses[i] = plusObj;
            plusRects[i] = plusRect;
        }
        
        // Animate
        float elapsed = 0f;
        Vector3 startPos = leftHand.position;
        
        while (elapsed < healingDuration)
        {
            float t = elapsed / healingDuration;
            
            // Rotate center counterclockwise
            centerRect.Rotate(Vector3.forward, 300f * Time.deltaTime);
            
            // Fade out near end
            if (t > 0.7f)
            {
                Color color = centerImage.color;
                color.a = Mathf.Lerp(1f, 0f, (t - 0.7f) / 0.3f);
                centerImage.color = color;
            }
            
            // Spread pluses outward
            for (int i = 0; i < healingPlusCount; i++)
            {
                if (plusRects[i] != null)
                {
                    Vector2 offset = plusDirections[i] * healingSpreadRadius * t;
                    plusRects[i].position = startPos + new Vector3(offset.x, offset.y, 0);
                    
                    if (pluses[i] != null)
                    {
                        Image img = pluses[i].GetComponent<Image>();
                        Color color = img.color;
                        color.a = Mathf.Lerp(1f, 0f, t);
                        img.color = color;
                    }
                }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Clean up
        Destroy(centerObj);
        foreach (GameObject plus in pluses)
        {
            if (plus != null) Destroy(plus);
        }
    }
}
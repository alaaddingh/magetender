using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeScript : MonoBehaviour
{
    [SerializeField] public CanvasGroup canvasGroup;
    [SerializeField] public float FadeDuration = 5.0f;
    [SerializeField] public bool fadeInOnStart = false;
    [SerializeField] public bool disableGameObjectWhenDone = false;


    private void Start()
    {
        if (fadeInOnStart)
        {
            FadeIn();
        }
        else
        {
            FadeOut();
        }
    }

    public void FadeIn()
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        StartCoroutine(FadeCanvas(canvasGroup, canvasGroup.alpha, 0, FadeDuration));
    }
     public void FadeOut()
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        StartCoroutine(FadeCanvas(canvasGroup, canvasGroup.alpha, 1, FadeDuration));
    }

    private IEnumerator FadeCanvas(CanvasGroup cg, float start, float end, float duration)
    {
        float elapsed = 0.0f;
        while(elapsed < FadeDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield  return null;
        }
        cg.alpha = end;

        /* let clicks pass through after fade-in */
        if (end <= 0.001f)
        {
            cg.blocksRaycasts = false;
            cg.interactable = false;
            if (disableGameObjectWhenDone)
                gameObject.SetActive(false);
        }
    }

    
}

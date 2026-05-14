using UnityEngine;

public class IDClick : MonoBehaviour
{
    [SerializeField] private RectTransform IdImage;
    [SerializeField] private InkyDialogueController inkyDialogueController;

    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.05f;

    private Vector3 startScale;
    private float openTime;

    void OnEnable()
    {
        openTime = Time.time;

      
            startScale = IdImage.localScale;
    }

    void Update()
    {
    
            float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            IdImage.localScale = startScale * scale;

        // click anywhere to close
        if (Time.time > openTime + 0.15f &&
            Input.GetMouseButtonDown(0))
        {
            if (inkyDialogueController != null)
            {
                inkyDialogueController.SetBoolVariable("show_license", false);
            }

            gameObject.SetActive(false);
        }
    }
}
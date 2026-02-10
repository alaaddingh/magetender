/*
Purpose of file:
 Handles hover over glasses on "Pick Glass" screen
 When glass is pressed, relocates glasss
 sets "Next" button to active
 Informs "MixManager" of selected glass (stores the data)
*/
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class BottleHoverSnapUI : MonoBehaviour
{
    public Image smallBottle;
    public Image mediumBottle;
    public Image largeBottle;

    [Header("New bottle position")]
    public Vector2 snapTargetPos;

    public GameObject nextButton;

    [Header("FX")]
    public float hoverScale = 1.12f;
    public float pulseAmount = 0.03f;
    public float pulseSpeed = 6f;
    public float scaleLerpSpeed = 12f;

    public Color highlightColor = new Color(1f, 0.95f, 0.6f, 1f);
    public float colorLerpSpeed = 14f;

    private Image hovered;

    private Vector3 smallBaseScale, medBaseScale, largeBaseScale;
    private Color smallBaseColor, medBaseColor, largeBaseColor;


     [Header("Manager")]
    [SerializeField] private MixManager mixManager;


    [Header("Ui Panels (to toggle/hide)")] 
    public GameObject CurrentScreen;
    public GameObject NextScreen;

    void Awake()
    {
        smallBaseScale = smallBottle.rectTransform.localScale;
        medBaseScale   = mediumBottle.rectTransform.localScale;
        largeBaseScale = largeBottle.rectTransform.localScale;

        smallBaseColor = smallBottle.color;
        medBaseColor   = mediumBottle.color;
        largeBaseColor = largeBottle.color;
        nextButton.SetActive(false);

    }

    void Update()
    {
        hovered = GetHoveredBottle();

        if (Input.GetMouseButtonDown(0) && hovered != null)
        {
            hovered.rectTransform.anchoredPosition = snapTargetPos;
            nextButton.SetActive(true);


            /* Store string for bottle type in mix manager */
             mixManager.SetBottle(GetBottleKey(hovered));
        }

        UpdateBottleFX(smallBottle,  smallBaseScale, smallBaseColor, hovered == smallBottle);
        UpdateBottleFX(mediumBottle, medBaseScale,   medBaseColor,   hovered == mediumBottle);
        UpdateBottleFX(largeBottle,  largeBaseScale, largeBaseColor, hovered == largeBottle);
    }

    /* helper function to get string for mix manager */
    private string GetBottleKey(Image img)
    {
        if (img == smallBottle) return "small";
        if (img == mediumBottle) return "medium";
        if (img == largeBottle) return "large";
        return "";
    }

    Image GetHoveredBottle()
    {
        var pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);

        foreach (var r in results)
        {
            var img = r.gameObject.GetComponent<Image>();
            if (img == smallBottle || img == mediumBottle || img == largeBottle)
                return img;
        }

        return null;
    }

    void UpdateBottleFX(Image img, Vector3 baseScale, Color baseColor, bool isHovered)
    {
        Vector3 targetScale = baseScale;

        if (isHovered)
        {
            float pulse = Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmount;
            float scale = hoverScale + pulse;
            targetScale = baseScale * scale;
        }

        /* smoothly move towards scale */
        img.rectTransform.localScale = Vector3.Lerp(
            img.rectTransform.localScale,
            targetScale,
            scaleLerpSpeed * Time.unscaledDeltaTime
        );

        Color targetColor = isHovered ? highlightColor : baseColor;

        img.color = Color.Lerp(
            img.color,
            targetColor,
            colorLerpSpeed * Time.unscaledDeltaTime
        );
    }

    public void NextPressed()
    {
        CurrentScreen.SetActive(false);
        NextScreen.SetActive(true);
    }
}


/* rly simple for now, when user clicks on UI image tagged as "Ingredients", relocates image to new XY and adds Ingredients
to mixmanager's storage */
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class IngredientHoverSnapUI : MonoBehaviour
{
    /* relocate ingredients after click */
    [Header("target position")]
    public Vector2 snapTargetPos;

    private Image hoveredIngredient;

    void Update()
    {
        hoveredIngredient = GetHoveredIngredient();

        if (Input.GetMouseButtonDown(0) && hoveredIngredient != null)
        {
            hoveredIngredient.rectTransform.anchoredPosition = snapTargetPos;

            /* add ingredient to mixmanager */ 
             var mixManager = FindFirstObjectByType<MixManager>();
             mixManager.AddIngredient(hoveredIngredient.name);
        
    }
    }

    Image GetHoveredIngredient()
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);

        foreach (RaycastResult r in results)
        {
            if (!r.gameObject.CompareTag("Ingredients"))
                continue;

            Image img = r.gameObject.GetComponent<Image>();
            if (img != null)
                return img;
        }

        return null;
    }
}

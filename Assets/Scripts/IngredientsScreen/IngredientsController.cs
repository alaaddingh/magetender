using UnityEngine;
using UnityEngine.UI;

/*
purpose: 
    simply copy over the existing BaseBottle (size + color)
    to ingredients screen 
    
    TO-DO: Make a NextPressed() function to transition to final "Mix" screen
*/
public class IngredientController : MonoBehaviour
{
    [Header("Ingredients bottle to target")]
    public Image IngredientsBottle;

    [Header("Base screen bottle in scene")]
    public Image BaseBottle; 

    private void OnEnable()
    {
        ApplyFromBaseBottle();
    }

    private void Start()
    {
        ApplyFromBaseBottle();
    }

    private void ApplyFromBaseBottle()
    {
        UIImgUtil.CopyAppearance(BaseBottle, IngredientsBottle);
    }
}

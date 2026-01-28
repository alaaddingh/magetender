
/** storage of user selections, exists in scene under MixManager gameobject */
using UnityEngine;
using System.Collections.Generic;
 public class MixManager : MonoBehaviour 
 { [Header("Bottle selection")] 
 public string SelectedBottle = ""; 
 public List<string> SelectedIngredients = new List<string>();

 public void SetBottle(string bottleKey) {
     SelectedBottle = bottleKey; 
     } 

public void AddIngredient(string IngredientKey)
{
    SelectedIngredients.Add(IngredientKey);
}
 }
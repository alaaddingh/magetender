
/** storage of user selections, exists in scene under MixManager gameobject */
using UnityEngine;
using System.Collections.Generic;
 public class MixManager : MonoBehaviour 
 { [Header("Bottle selection")] 
 public string SelectedBottle = ""; 

  public string SelectedBase = "";

 public List<string> SelectedIngredients = new List<string>();

 [Header("Fill Data")]
 public float FillLevel = 0f;
 public Dictionary<string, float> BaseAmounts = new Dictionary<string, float>();

 public void SetBottle(string bottleKey) {
     SelectedBottle = bottleKey; 
     } 

public void SetBase(string baseKey) {
    SelectedBase = baseKey;
     }

public void AddIngredient(string IngredientKey)
{
    SelectedIngredients.Add(IngredientKey);
}

public void SetFillData(float fillLevel, Dictionary<string, float> baseAmounts)
{
    FillLevel = fillLevel;
    BaseAmounts.Clear();
    foreach (var kvp in baseAmounts)
    {
        BaseAmounts[kvp.Key] = kvp.Value;
    }
}
 }
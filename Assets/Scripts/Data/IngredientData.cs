using System;
using System.Collections.Generic;

[Serializable]
public class IngredientsFile
{
    public List<IngredientData> ingredients;
}

[Serializable]
public class IngredientData
{
    public string id;
    public string name;
    public string description;
    public string type;
    public IngredientEffect effect;
}

[Serializable]
public class IngredientEffect
{
    public float x;
    public float y;
}

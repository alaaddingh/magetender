using System;
using System.Collections.Generic;
using UnityEngine;

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
    public IngredientColor color;
}

[Serializable]
public class IngredientEffect
{
    public float x;
    public float y;
}

[Serializable]
public class IngredientColor
{
    public float r = 1f;
    public float g = 1f;
    public float b = 1f;
    public float a = 1f;

    public Color ToUnityColor()
    {
        return new Color(r, g, b, a);
    }
}

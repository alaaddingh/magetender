using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MonstersFile
{
    public List<MonsterData> monsters;
}

[Serializable]
public class MonsterData
{
    public string id;
    public string name;
    public string toppingsPreference;
    public MonsterSprites sprites;
    public MonsterPosition position;
    public float scale = 1f;
    public string dialogueId;
    public string inkDialogueId;
    public string inkDialogueId_es;
    public string inkDialogueId_ar;
    public string assessDialogueId;
    public string assessDialogueId_es;
    public string assessDialogueId_ar;
    public bool branching;

    public string GetInkDialogueIdForCurrentLanguage()
    {
        return ResolveInkPathForLanguage(inkDialogueId, inkDialogueId_es, inkDialogueId_ar);
    }

    public string GetAssessInkDialogueIdForCurrentLanguage()
    {
        return ResolveInkPathForLanguage(assessDialogueId, assessDialogueId_es, assessDialogueId_ar);
    }

    private static string ResolveInkPathForLanguage(string englishPath, string spanishPath, string arabicPath)
    {
        string language = LanguageManager.Instance != null
            ? LanguageManager.Instance.CurrentLanguage
            : PlayerPrefs.GetString("GameLanguage", LanguageManager.LangEnglish);

        if (language == LanguageManager.LangSpanish && !string.IsNullOrWhiteSpace(spanishPath))
            return spanishPath;
        if (language == LanguageManager.LangArabic && !string.IsNullOrWhiteSpace(arabicPath))
            return arabicPath;
        return englishPath ?? string.Empty;
    }
}

[Serializable]
public class MonsterSprites
{
    public string neutral;
    public string happy;
    public string angry;
    public string fight;
    public string hit;
}

[Serializable]
public class MonsterPosition
{
    public float pos_x;
    public float pos_y;
}

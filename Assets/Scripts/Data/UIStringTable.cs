using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StringEntry
{
    public string key;
    public string value;
}

[Serializable]
public class UIStringTable
{
    public List<StringEntry> entries;
}

public static class L
{
    private static Dictionary<string, string> _table;
    private static string _loadedForLanguage;

    private static Dictionary<string, string> GetEnglishFallback()
    {
        return new Dictionary<string, string>
        {
            { "mood_tooltip_label", "current mood" },
            { "score_display_format", "Mood: ({0:F2}, {1:F2})" },
            { "coins_suffix", " coins" },
            { "day_prefix", "Day " },
            { "tutorial_default_customer", "This customer" },
            { "tutorial_body", "{0} is looking for a fight!\n\nPress <color=yellow>WASD</color> to complete key sequences\nPress <color=yellow>SPACE</color> to switch between banks\n\n<color=blue>DEFEND</color> - Complete before time runs out to avoid taking damage\n<color=red>ATTACK</color> - Complete to deal damage\n\nPress <color=yellow>SPACE</color> to begin..." },
            { "ingredient_no_match", "(no json match)" },
            { "ingredient_axis_x", "grounded↔dissociative" },
            { "ingredient_axis_y", "calm↕elevated" },
            { "health_separator", " / " }
        };
    }

    private static void EnsureLoaded()
    {
        string lang = LanguageManager.Instance != null ? LanguageManager.Instance.CurrentLanguage : LanguageManager.LangEnglish;
        if (_table != null && _loadedForLanguage == lang) return;

        _loadedForLanguage = lang;
        _table = new Dictionary<string, string>(GetEnglishFallback());

        string path = LanguageManager.Instance != null ? LanguageManager.Instance.GetUIStringsResourcePath() : "Data/UIStrings_en";
        TextAsset asset = Resources.Load<TextAsset>(path);
        if (asset != null)
        {
            var data = JsonUtility.FromJson<UIStringTable>(asset.text);
            if (data?.entries != null)
            {
                foreach (var e in data.entries)
                    if (!string.IsNullOrEmpty(e.key))
                        _table[e.key] = e.value ?? "";
            }
        }
    }

    public static string Get(string key)
    {
        EnsureLoaded();
        return _table != null && _table.TryGetValue(key, out var s) ? s : key;
    }

    public static string Get(string key, params object[] args)
    {
        string format = Get(key);
        if (args == null || args.Length == 0) return format;
        try { return string.Format(format, args); }
        catch { return format; }
    }
}

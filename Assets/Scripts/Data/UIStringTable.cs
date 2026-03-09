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

    private static void EnsureLoaded()
    {
        string lang = LanguageManager.Instance != null
            ? LanguageManager.Instance.CurrentLanguage
            : PlayerPrefs.GetString("GameLanguage", LanguageManager.LangEnglish);
        if (_table != null && _loadedForLanguage == lang) return;

        _loadedForLanguage = lang;
        _table = new Dictionary<string, string>();

        string path = LanguageManager.Instance != null
            ? LanguageManager.Instance.GetUIStringsResourcePath()
            : (lang == LanguageManager.LangSpanish
                ? "Data/UIStrings_es"
                : (lang == LanguageManager.LangArabic ? "Data/UIStrings_ar" : "Data/UIStrings_en"));

        TextAsset asset = Resources.Load<TextAsset>(path);
        Debug.Log($"[UIStrings] Loading for lang='{lang}' from '{path}'");
        if (asset == null)
        {
            Debug.LogWarning($"[UIStrings] TextAsset not found at '{path}'");
            return;
        }

        var data = JsonUtility.FromJson<UIStringTable>(asset.text);
        if (data?.entries == null)
        {
            Debug.LogWarning("[UIStrings] Parsed JSON but entries list is null.");
            return;
        }

        foreach (var e in data.entries)
        {
            if (!string.IsNullOrEmpty(e.key))
                _table[e.key] = e.value ?? "";
        }

        Debug.Log($"[UIStrings] Loaded {_table.Count} entries.");
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

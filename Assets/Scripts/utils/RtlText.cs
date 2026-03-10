/* central utility to: 
1. Apply RTL in place of norml TMP component when in arabic
2. how to shape the arabic using RTLS plugin (fixRTL)
*/
using RTLTMPro;
using UnityEngine;

public static class RtlText
{
    private static readonly FastStringBuilder s_buffer = new FastStringBuilder(RTLSupport.DefaultBufferSize);

    public static bool IsArabic()
    {
        string lang = LanguageManager.Instance != null
            ? LanguageManager.Instance.CurrentLanguage
            : PlayerPrefs.GetString("GameLanguage", LanguageManager.LangEnglish);
        return lang == LanguageManager.LangArabic;
    }

    public static string FixIfArabic(string input, bool preserveNumbers = true, bool fixTags = true, bool reverseOutput = false)
    {
        if (!IsArabic())
            return input ?? string.Empty;
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        s_buffer.Clear();
        RTLSupport.FixRTL(input, s_buffer, farsi: false, fixTextTags: fixTags, preserveNumbers: preserveNumbers);
        if (reverseOutput)
            s_buffer.Reverse();
        return s_buffer.ToString();
    }
}

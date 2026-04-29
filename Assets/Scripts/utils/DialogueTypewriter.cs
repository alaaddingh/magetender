using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueTypewriter : MonoBehaviour
{
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private float charsPerSecond = 35f;
    [SerializeField] private float punctuationPauseMultiplier = 4f;

    private Coroutine typingRoutine;
    private string currentLine = string.Empty;

    public bool IsTyping { get; private set; }

    private void Awake()
    {
        if (targetText == null)
            targetText = GetComponent<TMP_Text>();
    }

    public void SetTargetText(TMP_Text newTarget)
    {
        if (newTarget == null)
            return;
        targetText = newTarget;
    }

    public void TypeLine(string line)
    {
        if (targetText == null)
            targetText = GetComponent<TMP_Text>();
        if (targetText == null)
            return;

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        currentLine = line ?? string.Empty;
        typingRoutine = StartCoroutine(TypeRoutine(currentLine));
    }

    public void SkipTyping()
    {
        if (targetText == null)
            targetText = GetComponent<TMP_Text>();
        if (targetText == null)
            return;

        if (!IsTyping)
            return;

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        targetText.text = currentLine;
        targetText.ForceMeshUpdate();
        targetText.maxVisibleCharacters = int.MaxValue;
        IsTyping = false;
    }

    public void SetInstant(string text)
    {
        if (targetText == null)
            targetText = GetComponent<TMP_Text>();
        if (targetText == null)
            return;

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        currentLine = text ?? string.Empty;
        targetText.text = currentLine;
        targetText.ForceMeshUpdate();
        targetText.maxVisibleCharacters = int.MaxValue;
        IsTyping = false;
    }

    private IEnumerator TypeRoutine(string line)
    {
        IsTyping = true;
        targetText.text = string.Empty;

        // Arabic shaping requires the full string; per-character typing causes disconnected glyphs / wrong order.
        if (IsArabicLanguage())
        {
            targetText.text = line ?? string.Empty;
            targetText.ForceMeshUpdate();
            targetText.maxVisibleCharacters = int.MaxValue;
            IsTyping = false;
            yield break;
        }

        targetText.text = line ?? string.Empty;
        targetText.ForceMeshUpdate();
        targetText.maxVisibleCharacters = 0;

        float delay = charsPerSecond > 0f ? (1f / charsPerSecond) : 0f;
        int visibleCharacters = targetText.textInfo.characterCount;

        for (int i = 0; i < visibleCharacters; i++)
        {
            targetText.maxVisibleCharacters = i + 1;

            if (delay <= 0f)
                continue;

            TMP_CharacterInfo characterInfo = targetText.textInfo.characterInfo[i];
            char c = characterInfo.character;

            if (c == '.' || c == ',' || c == '!' || c == '?')
                yield return new WaitForSeconds(delay * punctuationPauseMultiplier);
            else
                yield return new WaitForSeconds(delay);
        }

        IsTyping = false;
    }

    private static bool IsArabicLanguage()
    {
        string lang = LanguageManager.Instance != null
            ? LanguageManager.Instance.CurrentLanguage
            : PlayerPrefs.GetString("GameLanguage", LanguageManager.LangEnglish);
        return lang == LanguageManager.LangArabic;
    }
}

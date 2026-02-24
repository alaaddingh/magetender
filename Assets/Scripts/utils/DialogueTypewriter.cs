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

    public void TypeLine(string line)
    {
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
            return;

        if (!IsTyping)
            return;

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        targetText.text = currentLine;
        IsTyping = false;
    }

    public void SetInstant(string text)
    {
        if (targetText == null)
            return;

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        currentLine = text ?? string.Empty;
        targetText.text = currentLine;
        IsTyping = false;
    }

    private IEnumerator TypeRoutine(string line)
    {
        IsTyping = true;
        targetText.text = string.Empty;

        float delay = charsPerSecond > 0f ? (1f / charsPerSecond) : 0f;

        foreach (char c in line)
        {
            targetText.text += c;

            if (delay <= 0f)
                continue;

            if (c == '.' || c == ',' || c == '!' || c == '?')
                yield return new WaitForSeconds(delay * punctuationPauseMultiplier);
            else
                yield return new WaitForSeconds(delay);
        }

        IsTyping = false;
    }
}

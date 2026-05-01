using System.Collections.Generic;
using Ink.Runtime;
using TMPro;
using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;

/*
This file is the UI side of Ink dialogue.
It asks InkyDialogueController for the current line / current choices,
then shows that information on screen.

It handles:
1) Showing dialogue text.
2) Showing speaker name
3) Rendering choice buttons.
4) Forwarding button clicks back to InkyDialogueController.
*/
public class InkDialoguePresenter : MonoBehaviour
{
	[Header("Dependencies")]
	[SerializeField] private InkyDialogueController inkyDialogueController;

	[Header("Text UI")]
	[SerializeField] private TMP_Text dialogueText;
	[SerializeField] private TMP_Text serveDialogueText;
	[SerializeField] private TMP_Text speakerNameText;
	[SerializeField] private string speakerName = "";
	[SerializeField] private DialogueTypewriter dialogueTypewriter;
	[SerializeField] private DialogueTypewriter serveTypewriter;

	private TMP_Text activeDialogueText;
	private TMP_FontAsset activeDialogueOriginalFont;
	private DialogueTypewriter activeTypewriter;

	[Header("Choice UI")]
	[SerializeField] private GameObject choicesRoot;
	[SerializeField] private Button choiceButtonPrefab;

	[Header("Arabic (optional)")]
	[SerializeField] private TMP_FontAsset arabicFontOverride;

	private readonly List<Button> spawnedChoiceButtons = new List<Button>();
	private TMP_FontAsset dialogueOriginalFont;
	private TMP_FontAsset speakerOriginalFont;

	public bool IsTyping
	{
		get
		{
			return activeTypewriter != null && activeTypewriter.enabled && activeTypewriter.IsTyping;
		}
	}

	private void Awake()
	{
		if (dialogueText == null)
			dialogueText = GetComponent<TMP_Text>();

		activeDialogueText = dialogueText;
		activeDialogueOriginalFont = activeDialogueText != null ? activeDialogueText.font : null;

		if (dialogueTypewriter == null && dialogueText != null)
			dialogueTypewriter = dialogueText.GetComponent<DialogueTypewriter>();
		if (serveTypewriter == null && serveDialogueText != null)
			serveTypewriter = serveDialogueText.GetComponent<DialogueTypewriter>();

		activeTypewriter = dialogueTypewriter;
		if (activeTypewriter != null && activeDialogueText != null)
			activeTypewriter.SetTargetText(activeDialogueText);

		dialogueOriginalFont = dialogueText != null ? dialogueText.font : null;
		speakerOriginalFont = speakerNameText != null ? speakerNameText.font : null;
	}

	public void SetDialogueTextTarget(TMP_Text target)
	{
		if (target == null)
			return;

		activeDialogueText = target;
		activeDialogueOriginalFont = activeDialogueText != null ? activeDialogueText.font : null;
		activeTypewriter = activeDialogueText == serveDialogueText ? serveTypewriter : dialogueTypewriter;

		if (activeTypewriter != null)
			activeTypewriter.SetTargetText(activeDialogueText);
	}

	public void SetUseServeDialogueText(bool useServeText)
	{
		if (useServeText && serveDialogueText != null)
		{
			SetDialogueTextTarget(serveDialogueText);
			return;
		}

		SetDialogueTextTarget(dialogueText);
	}

	private void Start()
	{
		RefreshSpeakerName();
		ClearChoices();
	}

	public void StartKnotAndPresent(string knotName)
	{
		if (inkyDialogueController == null)
			return;

		inkyDialogueController.StartKnot(knotName);
		AdvanceAndPresent();
	}

	public void AdvanceAndPresent()
	{
		if (inkyDialogueController == null)
			return;

		if (activeTypewriter != null && activeTypewriter.enabled && activeTypewriter.IsTyping)
		{
			activeTypewriter.SkipTyping();
			return;
		}

		if (!inkyDialogueController.HasChoices)
			inkyDialogueController.ContinueStory();

		RefreshView();
	}

	public void RefreshView()
	{
		if (inkyDialogueController == null)
		{
			SetDialogueText(string.Empty);
			ClearChoices();
			return;
		}

		RefreshSpeakerName();
		SetDialogueText(inkyDialogueController.CurrentText);
		RenderChoices(inkyDialogueController.CurrentChoices);
	}

	public void ClearView()
	{
		SetDialogueText(string.Empty);
		ClearChoices();
	}

	public void SetSpeakerName(string newSpeakerName)
	{
		speakerName = newSpeakerName ?? string.Empty;
		RefreshSpeakerName();
	}

	private void RenderChoices(IReadOnlyList<Choice> choices)
	{
		ClearChoices();

		if (choicesRoot == null || choiceButtonPrefab == null || choices == null || choices.Count == 0)
		{
			if (choicesRoot != null)
				choicesRoot.SetActive(false);
			return;
		}

		choicesRoot.SetActive(true);

		for (int i = 0; i < choices.Count; i++)
		{
			Choice choice = choices[i];
			Button button = Instantiate(choiceButtonPrefab, choicesRoot.transform);
			spawnedChoiceButtons.Add(button);

			int choiceIndex = i;
			button.onClick.AddListener(delegate
			{
				OnChoiceSelected(choiceIndex);
			});

			TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>(includeInactive: true);
			if (buttonText != null)
			{
				string sanitizedChoice = SanitizeInkLine(choice.text);
				ApplyText(buttonText, sanitizedChoice, preserveNumbers: true, buttonText.font);
			}
		}
	}

	private void OnChoiceSelected(int choiceIndex)
	{
		if (inkyDialogueController == null)
			return;

		bool applied = inkyDialogueController.ChooseChoice(choiceIndex);
		if (!applied)
			return;

		inkyDialogueController.ContinueStory();
		RefreshView();
	}

	private void ClearChoices()
	{
		for (int i = 0; i < spawnedChoiceButtons.Count; i++)
		{
			if (spawnedChoiceButtons[i] != null)
				Destroy(spawnedChoiceButtons[i].gameObject);
		}

		spawnedChoiceButtons.Clear();

		if (choicesRoot != null)
			choicesRoot.SetActive(false);
	}

	private void RefreshSpeakerName()
	{
		if (speakerNameText == null)
			return;

		ApplyText(speakerNameText, speakerName, preserveNumbers: true, speakerOriginalFont);
	}

	private void SetDialogueText(string rawText)
	{
		if (activeDialogueText == null)
			return;

		string sanitizedText = SanitizeInkLine(rawText ?? string.Empty);
		bool isArabic = RtlText.IsArabic();
		activeDialogueText.font = isArabic && arabicFontOverride != null ? arabicFontOverride : activeDialogueOriginalFont;

		if (activeTypewriter != null)
		{
			activeTypewriter.enabled = !isArabic;
			if (activeTypewriter.enabled)
			{
				activeTypewriter.TypeLine(sanitizedText);
				return;
			}
		}

		ApplyText(activeDialogueText, sanitizedText, preserveNumbers: true, activeDialogueOriginalFont);
	}

	private void ApplyText(TMP_Text target, string raw, bool preserveNumbers, TMP_FontAsset originalFont)
	{
		if (target == null)
			return;

		raw = SanitizeInkLine(raw);

		bool isArabic = RtlText.IsArabic();

		if (target == dialogueText)
			target.font = isArabic && arabicFontOverride != null ? arabicFontOverride : dialogueOriginalFont;
		else if (target == speakerNameText)
			target.font = isArabic && arabicFontOverride != null ? arabicFontOverride : speakerOriginalFont;
		else if (originalFont != null)
			target.font = isArabic && arabicFontOverride != null ? arabicFontOverride : originalFont;

		if (target is RTLTextMeshPro rtl)
		{
			rtl.Farsi = false;
			rtl.FixTags = true;
			rtl.PreserveNumbers = preserveNumbers;
			rtl.ForceFix = isArabic;
			rtl.text = raw ?? string.Empty;
			return;
		}

		if (isArabic)
		{
			target.isRightToLeftText = true;
			target.text = RtlText.FixIfArabic(raw, preserveNumbers: preserveNumbers, fixTags: true, reverseOutput: true);
		}
		else
		{
			target.isRightToLeftText = false;
			target.text = raw ?? string.Empty;
		}
	}

	private string SanitizeInkLine(string line)
	{
		if (string.IsNullOrWhiteSpace(line))
			return string.Empty;

		string trimmedLine = line.Trim();
		if (trimmedLine.Length >= 2 && trimmedLine.StartsWith("\"") && trimmedLine.EndsWith("\""))
			return trimmedLine.Substring(1, trimmedLine.Length - 2).Trim();

		return trimmedLine;
	}
}

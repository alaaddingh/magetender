using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Ink.Runtime;
using UnityEngine;

/*
Responsibilities:
1) O
2) display a specific knotprovided.
3) Expose plain dialogue state back to the caller:
   - current text
   - whether the story can continue
   - current choices
   - whether the story is finished
4) Apply player choices to the active Ink story.
*/
public class InkyDialogueController : MonoBehaviour
{
	[SerializeField] private TextAsset inkJsonAsset;

	private Story story;
	private string lastChosenPlayerResponse;
	private string lastChosenPlayerResponseNormalized;

	public string CurrentText { get; private set; } = string.Empty;

	public void SetInkJsonAsset(TextAsset newInkJsonAsset)
	{
		inkJsonAsset = newInkJsonAsset;
		ResetStory();
	}

	public bool HasActiveStory
	{
		get
		{
			return story != null;
		}
	}

	public bool CanContinue
	{
		get
		{
			return story != null && story.canContinue;
		}
	}

	public bool HasChoices
	{
		get
		{
			return story != null && story.currentChoices.Count > 0;
		}
	}

	public bool IsFinished
	{
		get
		{
			return story == null || (!story.canContinue && story.currentChoices.Count == 0);
		}
	}

	public IReadOnlyList<Choice> CurrentChoices
	{
		get
		{
			return story != null ? story.currentChoices : null;
		}
	}

	public void StartKnot(string knotName)
	{
		story = new Story(inkJsonAsset.text);
		lastChosenPlayerResponse = null;

		if (!TryChoosePath(knotName))
		{
			Debug.LogWarning($"[InkyDialogueController] Could not find Ink path '{knotName}', starting at root instead.", this);
		}

		CurrentText = string.Empty;
	}

	private bool TryChoosePath(string knotName)
	{
		if (TryChoosePathString(knotName))
			return true;

		string resolvedPath = ResolveNestedInkPath(knotName);
		return !string.IsNullOrWhiteSpace(resolvedPath) && TryChoosePathString(resolvedPath);
	}

	private bool TryChoosePathString(string path)
	{
		try
		{
			story.ChoosePathString(path);
			return true;
		}
		catch (StoryException)
		{
			return false;
		}
	}

	private string ResolveNestedInkPath(string knotName)
	{
		if (story == null || story.mainContentContainer == null)
			return null;

		if (story.mainContentContainer.namedContent != null && story.mainContentContainer.namedContent.ContainsKey(knotName))
			return knotName;

		foreach (var topLevel in story.mainContentContainer.namedContent)
		{
			if (topLevel.Value is Container container && container.namedContent != null && container.namedContent.ContainsKey(knotName))
				return $"{topLevel.Key}.{knotName}";
		}

		return null;
	}

	public string ContinueStory()
	{
		if (story == null)
		{
			CurrentText = string.Empty;
			return CurrentText;
		}

		CurrentText = ReadNextNonEmptyLine();
		return CurrentText;
	}

	public bool ChooseChoice(int choiceIndex)
	{
		if (story == null)
			return false;

		if (choiceIndex < 0 || choiceIndex >= story.currentChoices.Count)
			return false;

		lastChosenPlayerResponse = SanitizeInkLine(story.currentChoices[choiceIndex].text);
		lastChosenPlayerResponseNormalized = NormalizeInkLine(lastChosenPlayerResponse);
		story.ChooseChoiceIndex(choiceIndex);
		GameAnalytics.TryRecordAlienIdRefusedFromInkChoice(lastChosenPlayerResponseNormalized);
		CurrentText = string.Empty;
		return true;
	}

	public void ResetStory()
	{
		story = null;
		lastChosenPlayerResponse = null;
		lastChosenPlayerResponseNormalized = null;
		CurrentText = string.Empty;
	}

	public Dictionary<string, object> GetVariablesSnapshot()
	{
		var snapshot = new Dictionary<string, object>();
		if (story == null)
			return snapshot;

		foreach (string variableName in story.variablesState)
		{
			snapshot[variableName] = story.variablesState[variableName];
		}

		return snapshot;
	}

	public bool TryGetBoolVariable(string variableName, out bool value)
	{
		value = false;
		if (story == null || string.IsNullOrWhiteSpace(variableName))
			return false;

		object rawValue = story.variablesState[variableName];
		if (rawValue is bool boolValue)
		{
			value = boolValue;
			return true;
		}

		return false;
	}

	private string ReadNextNonEmptyLine()
	{
		while (story != null && story.canContinue)
		{
			string line = story.Continue();
			if (string.IsNullOrWhiteSpace(line))
				continue;

			string sanitizedLine = SanitizeInkLine(line);
			if (IsPlayerResponseEcho(sanitizedLine))
				continue;

			lastChosenPlayerResponse = null;
			lastChosenPlayerResponseNormalized = null;
			return sanitizedLine;
		}

		return string.Empty;
	}

	private bool IsPlayerResponseEcho(string line)
	{
		if (string.IsNullOrEmpty(line) || string.IsNullOrEmpty(lastChosenPlayerResponseNormalized))
			return false;

		string normalizedLine = NormalizeInkLine(line);
		if (string.IsNullOrEmpty(normalizedLine))
			return false;

		bool isEcho =
			string.Equals(normalizedLine, lastChosenPlayerResponseNormalized, StringComparison.OrdinalIgnoreCase) ||
			normalizedLine.Contains(lastChosenPlayerResponseNormalized) ||
			lastChosenPlayerResponseNormalized.Contains(normalizedLine);

		if (!isEcho)
			return false;

		lastChosenPlayerResponse = null;
		lastChosenPlayerResponseNormalized = null;
		return true;
	}

	private string SanitizeInkLine(string line)
	{
		if (string.IsNullOrWhiteSpace(line))
			return string.Empty;

		string trimmedLine = line.Trim();
		if (trimmedLine.Length >= 2 && trimmedLine.StartsWith("\"") && trimmedLine.EndsWith("\""))
			trimmedLine = trimmedLine.Substring(1, trimmedLine.Length - 2).Trim();

		return trimmedLine;
	}

	private string NormalizeInkLine(string line)
	{
		if (string.IsNullOrWhiteSpace(line))
			return string.Empty;

		string normalized = line.Trim().ToLowerInvariant();
		normalized = Regex.Replace(normalized, "\\s+", " ");
		normalized = Regex.Replace(normalized, "[\"'’“”]", string.Empty);
		return normalized.Trim();
	}
}

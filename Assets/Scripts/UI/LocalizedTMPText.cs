using TMPro;
using RTLTMPro;
using UnityEngine;

public class LocalizedTMPText : MonoBehaviour
{
	public enum Style
	{
		None,
		Button,
		MoodGraphHint,
		MoodGraphTitleOrDescription
	}

	[SerializeField] private string key;
	[SerializeField] private Style style = Style.None;

	private TMP_Text tmpText;
	private readonly FastStringBuilder rtlOutput = new FastStringBuilder(RTLSupport.DefaultBufferSize);

	private void Awake()
	{
		tmpText = GetComponent<TMP_Text>();
		Refresh();
	}

	private void OnEnable()
	{
		LanguageManager.OnLanguageChanged += Refresh;
		Refresh();
	}

	private void OnDisable()
	{
		LanguageManager.OnLanguageChanged -= Refresh;
	}

	public void SetKey(string newKey)
	{
		key = newKey;
		Refresh();
	}

	public void SetKeyAndStyle(string newKey, Style newStyle)
	{
		key = newKey;
		style = newStyle;
		Refresh();
	}

	public void Refresh()
	{
		if (tmpText == null)
			tmpText = GetComponent<TMP_Text>();
		if (tmpText == null)
			return;
		if (string.IsNullOrEmpty(key))
			return;

		string localized = L.Get(key);
		bool useRtlTmpComponent = tmpText is RTLTextMeshPro;
		if (IsArabicLanguage() && !useRtlTmpComponent)
			localized = FixArabic(localized);

		tmpText.text = localized;
		ApplyStyle();
	}

	private static bool IsArabicLanguage()
	{
		string lang = LanguageManager.Instance != null
			? LanguageManager.Instance.CurrentLanguage
			: PlayerPrefs.GetString("GameLanguage", LanguageManager.LangEnglish);
		return lang == LanguageManager.LangArabic;
	}

	private string FixArabic(string input)
	{
		if (string.IsNullOrEmpty(input))
			return input;

		rtlOutput.Clear();
		RTLSupport.FixRTL(input, rtlOutput, farsi: false, fixTextTags: true, preserveNumbers: false);
		return rtlOutput.ToString();
	}

	private void ApplyStyle()
	{
		if (tmpText == null || style == Style.None)
			return;

		float baseSize = tmpText.fontSize;

		switch (style)
		{
			case Style.Button:
				tmpText.enableAutoSizing = true;
				tmpText.fontSizeMax = baseSize;
				tmpText.fontSizeMin = baseSize * 0.6f;
				tmpText.enableWordWrapping = false;
				break;
			case Style.MoodGraphHint:
				tmpText.enableAutoSizing = true;
				tmpText.fontSizeMax = baseSize;
				tmpText.fontSizeMin = Mathf.Max(14f, baseSize * 0.75f);
				tmpText.enableWordWrapping = true;
				break;
			case Style.MoodGraphTitleOrDescription:
				tmpText.enableAutoSizing = true;
				tmpText.fontSizeMax = baseSize * 0.85f;
				tmpText.fontSizeMin = 10f;
				tmpText.enableWordWrapping = true;
				break;
		}
	}
}

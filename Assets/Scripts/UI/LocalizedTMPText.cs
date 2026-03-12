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

	[Header("Arabic (optional)")]
	[SerializeField] private TMP_FontAsset arabicFontOverride;

	private TMP_Text tmpText;
	private TMP_FontAsset originalFont;
	private HorizontalAlignmentOptions originalHorizontalAlignment;
	private VerticalAlignmentOptions originalVerticalAlignment;
	private bool originalAutoSizing;
	private float originalFontSizeMin;
	private float originalFontSizeMax;
	private bool originalWordWrapping;
	private TextOverflowModes originalOverflowMode;
	private readonly FastStringBuilder rtlOutput = new FastStringBuilder(RTLSupport.DefaultBufferSize);

	private void Awake()
	{
		tmpText = GetComponent<TMP_Text>();
		if (tmpText != null)
		{
			originalFont = tmpText.font;
			originalHorizontalAlignment = tmpText.horizontalAlignment;
			originalVerticalAlignment = tmpText.verticalAlignment;
			originalAutoSizing = tmpText.enableAutoSizing;
			originalFontSizeMin = tmpText.fontSizeMin;
			originalFontSizeMax = tmpText.fontSizeMax;
			originalWordWrapping = tmpText.enableWordWrapping;
			originalOverflowMode = tmpText.overflowMode;
		}
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
		bool isArabic = IsArabicLanguage();

		// Keep fonts consistent (and revert cleanly) when switching languages.
		if (isArabic && arabicFontOverride != null)
			tmpText.font = arabicFontOverride;
		else if (originalFont != null)
			tmpText.font = originalFont;

		if (tmpText is RTLTextMeshPro rtl)
		{
			rtl.Farsi = false;
			rtl.FixTags = true;
			rtl.PreserveNumbers = true;
			rtl.ForceFix = isArabic;
			rtl.text = localized;
		}
		else
		{
			if (isArabic)
			{
				tmpText.isRightToLeftText = true;
				localized = FixArabic(localized);
			}
			else
			{
				tmpText.isRightToLeftText = false;
			}
			tmpText.text = localized;
		}

		ApplyStyle();

		if (IsKegLabelKey(key))
		{
			tmpText.horizontalAlignment = originalHorizontalAlignment;
			tmpText.verticalAlignment = VerticalAlignmentOptions.Middle;

			tmpText.enableAutoSizing = true;
			tmpText.fontSizeMax = tmpText.fontSize;
			tmpText.fontSizeMin = tmpText.fontSize * 0.6f;
			tmpText.enableWordWrapping = false;
			tmpText.overflowMode = TextOverflowModes.Overflow;
		}
		else if (isArabic && style == Style.Button)
		{
			tmpText.horizontalAlignment = originalHorizontalAlignment;
			tmpText.verticalAlignment = VerticalAlignmentOptions.Middle;
		}
		else
		{
			tmpText.horizontalAlignment = originalHorizontalAlignment;
			tmpText.verticalAlignment = originalVerticalAlignment;
			tmpText.enableAutoSizing = originalAutoSizing;
			tmpText.fontSizeMin = originalFontSizeMin;
			tmpText.fontSizeMax = originalFontSizeMax;
			tmpText.enableWordWrapping = originalWordWrapping;
			tmpText.overflowMode = originalOverflowMode;
		}
	}

	private static bool IsArabicLanguage()
	{
		string lang = LanguageManager.Instance != null
			? LanguageManager.Instance.CurrentLanguage
			: PlayerPrefs.GetString("GameLanguage", LanguageManager.LangEnglish);
		return lang == LanguageManager.LangArabic;
	}

	private static bool IsKegLabelKey(string k)
	{
		return k == "blood_label" || k == "holywater_label" || k == "moonshine_label" || k == "spirits_label";
	}

	private string FixArabic(string input)
	{
		if (string.IsNullOrEmpty(input))
			return input;

		rtlOutput.Clear();
		RTLSupport.FixRTL(input, rtlOutput, farsi: false, fixTextTags: true, preserveNumbers: true);
		rtlOutput.Reverse();
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

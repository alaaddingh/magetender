using UnityEngine;

/* Holds current language and persists it. Used to load the right Dialogue JSON (and later other localized data). */
public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance { get; private set; }

    public const string LangEnglish = "en";
    public const string LangSpanish = "es";
    // public const string LangArabic = "ar"; // for Arabic

    private const string PlayerPrefsKey = "GameLanguage";

    [Header("Default language if none saved")]
    [SerializeField] private string defaultLanguage = LangEnglish;

    private string _currentLanguage;

    // Current language code (e.g. "en", "es"). Persisted to PlayerPrefs when set
    public string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage == value) return;
            _currentLanguage = value ?? defaultLanguage;
            PlayerPrefs.SetString(PlayerPrefsKey, _currentLanguage);
            PlayerPrefs.Save();
        }
    }

    public static void SetLanguage(string code)
    {
        if (string.IsNullOrEmpty(code))
            return;

        if (Instance != null)
        {
            Instance.CurrentLanguage = code;
        }
        else
        {
            PlayerPrefs.SetString(PlayerPrefsKey, code);
            PlayerPrefs.Save();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _currentLanguage = PlayerPrefs.GetString(PlayerPrefsKey, defaultLanguage);
        if (string.IsNullOrEmpty(_currentLanguage))
            _currentLanguage = defaultLanguage;
    }

    // Resource path for the dialogue JSON for the current language (e.g. "Data/Dialogue", "Data/Dialogue_es").
    public string GetDialogueResourcePath()
    {
        Debug.Log("GetDialogueResourcePath: " + _currentLanguage);
        if (_currentLanguage == LangSpanish)
            return "Data/Dialogue_es";
        // if (_currentLanguage == LangArabic) return "Data/Dialogue_ar";
        return "Data/Dialogue";
    }

    // Resource path for UI strings JSON (e.g. "Data/UIStrings_en", "Data/UIStrings_es").
    public string GetUIStringsResourcePath()
    {
        if (_currentLanguage == LangEnglish) return "Data/UIStrings_en";
        if (_currentLanguage == LangSpanish) return "Data/UIStrings_es";
        return "Data/UIStrings_en";
    }

	// Resource path for ingredients JSON (e.g. "Data/Ingredients", "Data/Ingredients_es").
	public string GetIngredientsResourcePath()
	{
		if (_currentLanguage == LangEnglish) return "Data/Ingredients";
		if (_currentLanguage == LangSpanish) return "Data/Ingredients_es";
		return "Data/Ingredients";
	}
}

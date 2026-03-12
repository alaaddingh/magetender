using UnityEngine;
using TMPro;

public class TitleOptions : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsPanel;

    [SerializeField] private TMP_Dropdown languageDropdown;

    private void Start()
    {
        if (languageDropdown == null)
            return;

        string lang = LanguageManager.Instance != null
            ? LanguageManager.Instance.CurrentLanguage
            : PlayerPrefs.GetString("GameLanguage", LanguageManager.LangEnglish);

        int index = 0;
        if (lang == LanguageManager.LangSpanish)
            index = 1;
        else if (lang == LanguageManager.LangArabic)
            index = 2;

        languageDropdown.SetValueWithoutNotify(index);
        languageDropdown.RefreshShownValue();
    }

    public void ShowOptions()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void ShowMainMenu()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void OnLanguageDropdownChanged(int index)
    {
        if (index == 0)
            LanguageManager.SetLanguage(LanguageManager.LangEnglish);
        else if (index == 1)
            LanguageManager.SetLanguage(LanguageManager.LangSpanish);
        else if (index == 2)
            LanguageManager.SetLanguage(LanguageManager.LangArabic);
    }
}

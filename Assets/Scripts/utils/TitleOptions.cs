using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TitleOptions : MonoBehaviour
{
    private const string ControlsPrefKey = "QTEControlScheme"; // 0 = Arrow keys, 1 = WASD
    private const string SwitchKeyPrefKey = "QTESwitchKey"; // 0 = Space, 1 = Shift

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsPanel;

    [SerializeField] private TMP_Dropdown languageDropdown;

    [Header("Controls")]
    [SerializeField] private TMP_Text controlSchemeLabel;
    [SerializeField] private TMP_Text switchKeyLabel;

    [Header("Audio")]
    [SerializeField] private Slider masterVolumeSlider;

    private void Start()
    {
        if (languageDropdown != null)
        {
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

        UpdateSequenceLabel();
        UpdateBankLabel();

        if (masterVolumeSlider != null && AudioManager.Instance != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.GetMasterVolume());
        }
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

        UpdateSequenceLabel();
        UpdateBankLabel();
    }

    // Movement keys (Arrow <-> WASD)
    public void CycleSequenceScheme()
    {
        int scheme = PlayerPrefs.GetInt(ControlsPrefKey, 0);

        scheme = (scheme + 1) % 2;

        PlayerPrefs.SetInt(ControlsPrefKey, scheme);
        PlayerPrefs.Save();

        UpdateSequenceLabel();
    }

    void UpdateSequenceLabel()
    {
        int scheme = PlayerPrefs.GetInt(ControlsPrefKey, 0);

        if (controlSchemeLabel != null)
        {
            if (scheme == 0)
                controlSchemeLabel.text = "< " + L.Get("controls_arrows") + " >";
            else
                controlSchemeLabel.text = "< " + L.Get("controls_wasd") + " >";
        }
    }

    public void CycleBankKey()
    {
        int key = PlayerPrefs.GetInt(SwitchKeyPrefKey, 0);

        key = (key + 1) % 2;

        PlayerPrefs.SetInt(SwitchKeyPrefKey, key);
        PlayerPrefs.Save();

        UpdateBankLabel();
    }

    void UpdateBankLabel()
    {
        int key = PlayerPrefs.GetInt(SwitchKeyPrefKey, 0);

        if (switchKeyLabel != null)
        {
            if (key == 0)
                switchKeyLabel.text = "< " + L.Get("controls_space") + " >";
            else
                switchKeyLabel.text = "< " + L.Get("controls_shift") + " >";
        }
    }

    public void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(value);
        }
    }
}

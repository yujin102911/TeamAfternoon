using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LocalizationFontManager : MonoBehaviour
{
    [System.Serializable]
    public class LocaleFontPair
    {
        public string localeCode;   // "ko-KR", "en", "ja"
        public TMP_FontAsset font;
    }

    [SerializeField]
    private LocaleFontPair[] localeFonts;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void Start()
    {
        ApplyFont(LocalizationSettings.SelectedLocale);
    }

    private void OnLocaleChanged(Locale locale)
    {
        ApplyFont(locale);
    }

    private void ApplyFont(Locale locale)
    {
        string code = locale.Identifier.Code;

        foreach (var pair in localeFonts)
        {
            if (pair.localeCode == code)
            {
                TMP_Settings.defaultFontAsset = pair.font;
                RefreshAllTMP();
                return;
            }
        }

        Debug.LogWarning($"No font assigned for locale: {code}");
    }

    private void RefreshAllTMP()
    {
        foreach (var text in FindObjectsOfType<TextMeshProUGUI>(true))
        {
            text.font = TMP_Settings.defaultFontAsset;
            text.ForceMeshUpdate();
        }
    }
}

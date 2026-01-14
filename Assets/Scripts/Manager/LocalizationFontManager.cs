using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        ApplyFont(LocalizationSettings.SelectedLocale);
        Debug.Log($"[Scene] {SceneManager.GetActiveScene().name}");
    }

    private void OnLocaleChanged(Locale locale)
    {
        ApplyFont(locale);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyFont(LocalizationSettings.SelectedLocale);
        Debug.Log($"[Scene Loaded] {scene.name}");
    }

    private void ApplyFont(Locale locale)
    {
        if (locale == null) return;

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
        foreach (var text in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            text.font = TMP_Settings.defaultFontAsset;
            text.ForceMeshUpdate();
        }
    }
}

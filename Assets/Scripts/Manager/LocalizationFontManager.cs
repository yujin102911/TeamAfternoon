using System.Collections;
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

    private string _lastLocaleCode;
    private TMP_FontAsset _lastFont;

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
        // 씬 로드 직후는 UI가 아직 다 안 붙었을 수 있어서 1프레임 뒤에 적용
        StartCoroutine(ApplyAfterOneFrame());
    }

    private IEnumerator ApplyAfterOneFrame()
    {
        yield return null;
        ApplyFont(LocalizationSettings.SelectedLocale);
    }

    private void ApplyFont(Locale locale)
    {
        if (locale == null) return;

        string code = locale.Identifier.Code;

        // ✅ 같은 로케일/같은 폰트면 아무 것도 안 함 (씬 로드/이벤트 중복 호출 최소화)
        //if (_lastLocaleCode == code && _lastFont == TMP_Settings.defaultFontAsset)
        //    return;

        TMP_FontAsset targetFont = null;

        foreach (var pair in localeFonts)
        {
            if (pair != null && pair.localeCode == code)
            {
                targetFont = pair.font;
                break;
            }
        }

        if (targetFont == null)
        {
            Debug.LogWarning($"No font assigned for locale: {code}");
            return;
        }

        TMP_Settings.defaultFontAsset = targetFont;
        _lastLocaleCode = code;
        _lastFont = targetFont;

        RefreshSceneTMP();
    }

    // ✅ 전체 씬 전수조사(FindObjectsByType + Inactive Include) 제거
    // ✅ 현재 씬의 Root -> 자식으로만, "활성 오브젝트만" 갱신
    private void RefreshSceneTMP()
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.isLoaded) return;

        var roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            // includeInactive: false (비활성 포함하면 스파이크 커짐)
            var texts = roots[i].GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
            for (int t = 0; t < texts.Length; t++)
            {
                var text = texts[t];
                if (text == null) continue;

                // 이미 같은 폰트면 스킵
                if (text.font == TMP_Settings.defaultFontAsset) continue;

                text.font = TMP_Settings.defaultFontAsset;

                // ForceMeshUpdate()는 무거워서 제거
                // 필요 최소 갱신만 요청
                text.havePropertiesChanged = true;
                text.SetAllDirty();
            }
        }
    }
}

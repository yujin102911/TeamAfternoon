using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LocalizedImageSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class LocaleSpritePair
    {
        public string localeCode;   // 예: "en", "ko-KR", "ja", "zh-Hans"
        public Sprite sprite;
    }

    [SerializeField] private Image targetImage;
    [SerializeField] private LocaleSpritePair[] localeSprites;
    [SerializeField] private Sprite fallbackSprite; // 없으면 null 가능

    private void Awake()
    {
        if (!targetImage) targetImage = GetComponent<Image>();
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void Start()
    {
        Apply(LocalizationSettings.SelectedLocale);
    }

    private void OnLocaleChanged(Locale locale)
    {
        Apply(locale);
    }

    private void Apply(Locale locale)
    {
        if (locale == null || targetImage == null) return;

        string code = locale.Identifier.Code;

        foreach (var pair in localeSprites)
        {
            if (pair != null && pair.localeCode == code && pair.sprite != null)
            {
                targetImage.sprite = pair.sprite;
                return;
            }
        }

        // localeCode가 "ko"만 들어오는데 "ko-KR"로 넣어둔 경우 대비(선택)
        string shortCode = code.Split('-')[0];
        foreach (var pair in localeSprites)
        {
            if (pair != null && pair.localeCode == shortCode && pair.sprite != null)
            {
                targetImage.sprite = pair.sprite;
                return;
            }
        }

        if (fallbackSprite != null)
            targetImage.sprite = fallbackSprite;
    }
}

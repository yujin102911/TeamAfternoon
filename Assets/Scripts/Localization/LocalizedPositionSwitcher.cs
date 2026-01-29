using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LocalizedPositionSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class LocalePositionPair
    {
        public string localeCode;
        public Vector2 anchoredPosition;
    }
    [SerializeField] private RectTransform targetRectTransform;
    [SerializeField] private LocalePositionPair[] localePositions;

    [SerializeField] private Vector2 fallbackPosition;
    [SerializeField] private bool useFallback = true;

    private void Awake()
    {
        if (!targetRectTransform) targetRectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale)
    {
        Apply(locale);
    }

    private void Apply(Locale locale)
    {
        if (locale == null || targetRectTransform == null) return;

        string code = locale.Identifier.Code;

        foreach (var pair in localePositions)
        {
            if (pair != null && pair.localeCode == code)
            {
                targetRectTransform.anchoredPosition = pair.anchoredPosition;
                return;
            }
        }
        string shortCode = code.Split('-')[0];
        foreach (var pair in localePositions)
        {
            if (pair != null && pair.localeCode == shortCode)
            {
                targetRectTransform.anchoredPosition = pair.anchoredPosition;
                return;
            }
        }
        if (useFallback)
        {
            targetRectTransform.anchoredPosition = fallbackPosition;
        }
    }

    [ContextMenu("Set Current Position As Fallback")]
    private void SetCurrentPosAsFallback()
    {
        if (targetRectTransform == null) targetRectTransform = GetComponent<RectTransform>();
        fallbackPosition = targetRectTransform.anchoredPosition;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        Debug.Log($"Current Position {fallbackPosition} saved to Fallback.");
    }

}

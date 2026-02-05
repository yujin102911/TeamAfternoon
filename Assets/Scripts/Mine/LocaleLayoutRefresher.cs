using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class LocaleLayoutRefresher : MonoBehaviour
{
    [SerializeField] private RectTransform layoutRoot; // Difficulty_con 같은 루트
    [SerializeField] private TMP_Text[] tmps;          // Easy/Normal/Hard TMP들

    private Coroutine refreshCoroutine;

    private void Awake()
    {
        LocalizationSettings.SelectedLocaleChanged += Refresh;
    }

    private void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= Refresh;
    }

    private void OnEnable()
    {
        Refresh(LocalizationSettings.SelectedLocale);
    }

    public void Refresh(Locale locale)
    {
        if (this.isActiveAndEnabled)
        {
            if (refreshCoroutine != null)
                StopCoroutine(refreshCoroutine);
            refreshCoroutine = StartCoroutine(CoRefresh());
        }
            
    }

    private IEnumerator CoRefresh()
    {
        // 로컬라이즈 문자열이 적용되는 타이밍 확보
        yield return null;

        // TMP preferred size 확정
        if (tmps != null)
        {
            foreach (var t in tmps)
                if (t) t.ForceMeshUpdate();
        }

        // 해당 루트만 레이아웃 즉시 재계산
        if (layoutRoot)
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
        
        // 상위 레이아웃이 또 물고 있으면 부모도 한번
        var parent = layoutRoot.parent as RectTransform;
        if (parent)
            LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
    }
}

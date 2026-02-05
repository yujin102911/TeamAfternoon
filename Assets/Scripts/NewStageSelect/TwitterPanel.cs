using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Localization.Settings;

public class TwitterPanel : MonoBehaviour
{
    [Header("연결된 데이터")]
    [SerializeField] private TwitData twitDataSO;

    [Header("프리팹 설정")]
    [SerializeField] private GameObject twitPrefab;
    [SerializeField] private GameObject commentPrefab;

    [Header("생성 위치")]
    [SerializeField] private Transform contentTransform;

    private void OnEnable()
    {
        RefreshFeed();
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(UnityEngine.Localization.Locale locale)
    {
        RefreshFeed();
    }

    public void RefreshFeed()
    {
        twitDataSO = ServiceLocator.Instance.CurrentTwitData;
        if (twitDataSO == null)
        {
            Debug.LogError("CurrentTwitData가 ServiceLocator에 없음");
            return;
        }
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }
        var visibleTwits = twitDataSO.TwitDatas
            .Where(t => t.IsVisible)
            .OrderByDescending(t => t.TwitID)
            .ToList();
        foreach (var twitItem in visibleTwits)
        {
            CreateTwitItem(twitItem);
        }
        Canvas.ForceUpdateCanvases();
    }

    private void CreateTwitItem(TwitItem data)
    {
        GameObject twitObj = Instantiate(twitPrefab, contentTransform);
        TwitUI twitUI = twitObj.GetComponent<TwitUI>();

        if (twitUI != null)
        {
            twitUI.SetData(data, commentPrefab);
        }
    }

}

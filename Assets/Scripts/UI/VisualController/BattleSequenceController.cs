using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

public class BattleSequenceController : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private GameObject _storyLine;      // 스토리라인 텍스트
    [SerializeField] private GameObject _timelinePanel;  // 타임라인 패널

    [Header("연출 설정")]
    [SerializeField] private float _storyLineDuration = 1.5f;
    [SerializeField] private float _timelinePanelDuration = 1.5f;
    [SerializeField] private float _timelineUpDuration = 1f;
    [SerializeField] private float targetHeight = 620f;

    private RectTransform _storyLineRect;
    private float targetWidth;
    private RectTransform _timelineRect;

    private void Awake()
    {
        if (_storyLine != null)
        {
            _storyLineRect = _storyLine.GetComponent<RectTransform>();
            targetWidth = _storyLineRect.sizeDelta.x;
        }
        if (_timelinePanel != null)
        {
            _timelineRect = _timelinePanel.GetComponent<RectTransform>();
        }
    }
    // 연출 재생 후 끝나면 onComplete
    public void PlayerTurnStartSequence(Action onComplete)
    {
        StartCoroutine(CoSequence(onComplete));
    }

    public void TimelineEndSequence()
    {
    }

    private IEnumerator CoSequence(Action onComplete)
    {
        _storyLine.SetActive(false);
        yield return StartCoroutine(ScrollUp());

        yield return StartCoroutine(AnimateExpand());

        yield return StartCoroutine(ScrollDown());

        onComplete?.Invoke();
    }

    private IEnumerator AnimateExpand()
    {
        _storyLineRect.sizeDelta = new Vector2(0, _storyLineRect.sizeDelta.y);
        _storyLine.SetActive(true);
        float timer = 0f;

        while (timer < _storyLineDuration)
        {
            timer += Time.deltaTime;

            float currentWidth = Mathf.Lerp(0f, targetWidth, timer / _storyLineDuration);
            _storyLineRect.sizeDelta = new Vector2(currentWidth, _storyLineRect.sizeDelta.y);

            yield return null;
        }
        _storyLineRect.sizeDelta = new Vector2(targetWidth, _storyLineRect.sizeDelta.y);
    }

    // 두루마기 내려오는 연출
    private IEnumerator ScrollDown()
    {
        float timer = 0f;
        _timelineRect.sizeDelta = new Vector2(_timelineRect.sizeDelta.x, 0);

        while (timer < _timelinePanelDuration)
        {
            timer += Time.deltaTime;

            float currentHeight = Mathf.Lerp(0f, targetHeight, timer/ _timelinePanelDuration);
            _timelineRect.sizeDelta = new Vector2(_timelineRect.sizeDelta.x, currentHeight);

            yield return null;
        }
        _timelineRect.sizeDelta = new Vector2(_timelineRect.sizeDelta.x, targetHeight);

    }

    // 두루마기 올라가는 연출
    private IEnumerator ScrollUp()
    {
        float timer = 0f;
        while (timer < _timelineUpDuration)
        {
            timer += Time.deltaTime;
            float currentHeight = Mathf.Lerp(targetHeight, 0f, timer/ _timelineUpDuration);
            _timelineRect.sizeDelta = new Vector2(_timelineRect.sizeDelta.x, currentHeight);

            yield return null;
        }
        _timelineRect.sizeDelta = new Vector2(_timelineRect.sizeDelta.x, 0f);

    }

}

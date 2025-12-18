using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

public class BattleSequenceController : MonoBehaviour
{
    [SerializeField]
    private RectTransform[] _enemyCardUIs;
    [SerializeField] private float _cardYOffset = 200f;
    [SerializeField] private float _cardDuration = 1f;

    [Header("UI 연결")]
    [SerializeField] private GameObject _storyLine;      // 스토리라인 텍스트
    [SerializeField] private GameObject _timelinePanel;  // 타임라인 패널
    [SerializeField] private GameObject _sectorSelectText; // 섹터 선택하세요 텍스트

    [Header("책 UI 연결")]
    [SerializeField]
    private RectTransform HandPanel;

    [Header("연출 설정")]
    [SerializeField] private float _storyLineDuration = 1.5f;
    [SerializeField] private float _timelinePanelDuration = 1.5f;
    [SerializeField] private float _timelineUpDuration = 1f;
    [SerializeField] private float targetHeight = 620f;

    [Header("책 연출 설정")]
    [SerializeField] private float _handPanelUpY = 200f;
    [SerializeField] private float _handPanelDownY = 0f;
    [SerializeField] private float _handPanelDuration = 1f;

    [Header("대상 카메라")]
    [SerializeField] private Camera targetCamera;

    [Header("Move Y")]
    [SerializeField] private float moveYOffset = 0.4f;
    [SerializeField] private float moveDuration = 0.3f;

    [Header("Zoom (Orthographic)")]
    [SerializeField] private float zoomInSizeOffset = -0.4f;
    [SerializeField] private float zoomDuration = 0.3f;

    [Header("Ease")]
    [SerializeField]
    private AnimationCurve easeCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Coroutine cameraCoroutine;

    // 🔹 기준값 캐싱
    private Vector3 basePosition;
    private float baseOrthoSize;


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

        if (targetCamera == null)
            targetCamera = Camera.main;

        basePosition = targetCamera.transform.position;
        baseOrthoSize = targetCamera.orthographicSize;
    }

    /// <summary>
    /// true  → 줌인 연출
    /// false → 줌아웃 (원위치 복귀)
    /// </summary>
    public void PlayCameraEffect(bool isZoomIn)
    {
        if (cameraCoroutine != null)
            StopCoroutine(cameraCoroutine);

        cameraCoroutine = StartCoroutine(CameraEffectCoroutine(isZoomIn));
    }

    // 연출 재생 후 끝나면 onComplete
    public void PlayerTurnStartSequence(Action onComplete)
    {
        StartCoroutine(CoSequence(onComplete));
    }

    private void TurnOnSectorSelectText()
    {
        if (_sectorSelectText != null && !GameManager.Instance.IsSectorSelected)
            _sectorSelectText.SetActive(true);
    }

    public void TurnOffSectorSelectText()
    {
        if (_sectorSelectText != null)
            _sectorSelectText.SetActive(false);
    }

    private IEnumerator CoSequence(Action onComplete)
    {
        _storyLine.SetActive(false);

        //yield return StartCoroutine(Move_enemyCardUIs(false));

        yield return StartCoroutine(ScrollUp());

        yield return StartCoroutine(AnimateExpand());

        yield return StartCoroutine(ScrollDown());

        PlayCameraEffect(false);
        yield return StartCoroutine(Move_HandPanel(true));

        if (BattleUIManager.Instance != null)
            BattleUIManager.Instance.Show_startBtn();

        TurnOnSectorSelectText();

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
        if (_timelineRect.sizeDelta.y == 0) yield break;

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

    public IEnumerator Move_HandPanel(bool isUp)
    {
        Vector2 startPos = HandPanel.anchoredPosition;

        float targetY = isUp ? _handPanelUpY : _handPanelDownY;
        Vector2 endPos = new Vector2(startPos.x, targetY);

        float elapsed = 0f;

        while (elapsed < _handPanelDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _handPanelDuration;

            // Ease Out
            t = 1f - Mathf.Pow(1f - t, 3f);

            HandPanel.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        HandPanel.anchoredPosition = endPos;
    }

    private IEnumerator CameraEffectCoroutine(bool isZoomIn)
    {
        Transform camTransform = targetCamera.transform;

        Vector3 startPos = camTransform.position;
        float startSize = targetCamera.orthographicSize;

        Vector3 targetPos = isZoomIn
            ? basePosition + Vector3.up * moveYOffset
            : basePosition;

        float targetSize = isZoomIn
            ? baseOrthoSize + zoomInSizeOffset
            : baseOrthoSize;

        float elapsed = 0f;
        float duration = Mathf.Max(moveDuration, zoomDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = easeCurve.Evaluate(t);

            camTransform.position =
                Vector3.Lerp(startPos, targetPos, easedT);

            targetCamera.orthographicSize =
                Mathf.Lerp(startSize, targetSize, easedT);

            yield return null;
        }

        camTransform.position = targetPos;
        targetCamera.orthographicSize = targetSize;

        cameraCoroutine = null;
    }

    public IEnumerator Move_enemyCardUIs(bool isUp)
    {
        int count = _enemyCardUIs.Length;

        Vector2[] startPositions = new Vector2[count];
        Vector2[] endPositions = new Vector2[count];

        

        // 시작 / 종료 위치 캐싱
        for (int i = 0; i < count; i++)
        {
            

            RectTransform rt = _enemyCardUIs[i];
            startPositions[i] = rt.anchoredPosition;

            float targetY = isUp ? startPositions[i].y + _cardYOffset : startPositions[i].y - _cardYOffset;

            endPositions[i] = new Vector2(startPositions[i].x, targetY);

            Debug.Log($"{i}번 {targetY}");
        }

        float elapsed = 0f;

        while (elapsed < _cardDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _cardDuration;

            // Ease Out
            t = 1f - Mathf.Pow(1f - t, 3f);

            for (int i = 0; i < count; i++)
            {
                _enemyCardUIs[i].anchoredPosition =
                    Vector2.Lerp(startPositions[i], endPositions[i], t);
            }

            yield return null;
        }

        // 최종 위치 보정
        for (int i = 0; i < count; i++)
        {
            _enemyCardUIs[i].anchoredPosition = endPositions[i];
        }
    }

}

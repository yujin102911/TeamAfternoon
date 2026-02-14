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

    [Header("타임 라인UI")]
    [SerializeField] private TimelineUI _timelineUI;

    [Header("게임 시작 가림막 페이드 아웃")]
    [SerializeField]
    private GameStartPanel_VController _gamestartPanel;

    [Header("타임 라인 가림막")]
    [SerializeField] GameObject[] _coverEffects;
    private GameObject _prevCover = null;
    private GameObject _currentCover = null;
    private GameObject _nextCover = null;

    [Header("연출 코드")]
    [SerializeField]
    private TimeLineResetController _resetController;
    [SerializeField]
    private SilinderResetController _silinderResetController;

    [Header("타임라인 슬라이더")]
    [SerializeField]
    private StepSlider _timelineSlider;

    [Header("타임라인 슬라이더")]
    [SerializeField]
    private Execution_Slider _roundSlider;

    [Header("배경 전환 연출")]
    [SerializeField] private RectTransform bgCurrent;
    [SerializeField] private RectTransform bgNext;
    [SerializeField] private float _bgSwitchDuration = 1.0f;
    
    private bool _isPlaying;

    [Header("좌우 적 이미지 연출")]
    [SerializeField] private RectTransform leftImage;
    [SerializeField] private RectTransform rightImage;

    [SerializeField] private float moveDistance = 500f;
    [SerializeField] private float duration = 0.5f;

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

    private void Start()
    {
        _roundSlider.Initialize(GameManager.Instance.CurrentStageData.LimitRound);
    }

    //슬라이더 연출
    public void PlaySlider()
    {
        _timelineSlider.Play(TimelineManager.Instance.Tick_interval * 16);
        _roundSlider.Play(TimelineManager.Instance.Tick_interval * 16);
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

    // 게임 시작 시 연출
    public void GameStartSequence()
    {
        if (BattleUIManager.Instance != null)
            BattleUIManager.Instance.Show_startBtn();


        StartCoroutine(SetCover());
        _gamestartPanel.FadeOut();
        //_timelineUI.SetActive_Slots(true);
    }

    // 연출 재생 후 끝나면 onComplete
    public void PlayerTurnStartSequence(Action onComplete)
    {
        //SwitchCover();

        _timelineUI.SetActive_Slots(false);
        //_nextCover.SetActive(false);
        //_prevCover.SetActive(true);

        if (Debug_Text.Instance != null)
            Debug_Text.Instance.Add_Message($"_prevCover: {_prevCover.gameObject.name}");

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
        GameManager.Instance.IsSequencePlaying = true;
        BattleUIManager.Instance.RefreshStartButtonState();

        

        yield return StartCoroutine(ScrollLeft());

        if (Debug_Text.Instance != null)
            Debug_Text.Instance.Add_Message($"_currentCover: {_currentCover.gameObject.name}");

        //_currentCover.SetActive(false);
        _timelineUI.SetActive_Slots(true);

        GameManager.Instance.IsSequencePlaying = false;
        BattleUIManager.Instance.RefreshStartButtonState();

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

    // 타임라인 원래대로 돌아오는 연출
    private IEnumerator ScrollLeft()
    {

        

        //Coroutine beltReset = _resetController.Play();
        //Coroutine sliderReturn = _timelineSlider.Return(2.0f);


        //yield return beltReset;
        //yield return sliderReturn;

        bool beltDone = false;
        bool sliderDone = false;

        StartCoroutine(RunAndFlag(_resetController.Play_IEnumerator(), () => beltDone = true));
        StartCoroutine(RunAndFlag(_timelineSlider.Return_IEnumerator(2.0f), () => sliderDone = true));

        yield return new WaitUntil(() => beltDone && sliderDone);

        //yield return StartCoroutine(_currentCover.Play_Effect(Direction.Down, 0.5f, false));
        
    }

    private IEnumerator RunAndFlag(IEnumerator routine, Action onDone)
    {
        yield return StartCoroutine(routine);
        onDone?.Invoke();
    }

    // 두루마기 올라가는 연출
    private IEnumerator ScrollUp()
    {
        if (_timelinePanel == null) yield break;
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

    // 적 입장
    public IEnumerator Slide_Enemy(bool is_in)
    {
        float halfWidth = Screen.width * 0.5f;

        // 화면 안 기준 위치 (도착 지점)
        Vector2 insideLeftPos = new Vector2(-moveDistance * 0.5f, 0);
        Vector2 insideRightPos = new Vector2(moveDistance * 0.5f, 0);

        // 화면 밖 위치
        Vector2 outsideLeftPos = new Vector2(-halfWidth - moveDistance, 0);
        Vector2 outsideRightPos = new Vector2(halfWidth + moveDistance, 0);

        Vector2 leftStart, leftEnd;
        Vector2 rightStart, rightEnd;

        if (is_in)
        {
            // 들어오기
            leftStart = outsideLeftPos;
            rightStart = outsideRightPos;

            leftEnd = insideLeftPos;
            rightEnd = insideRightPos;
        }
        else
        {
            // 나가기
            leftStart = insideLeftPos;
            rightStart = insideRightPos;

            leftEnd = outsideLeftPos;
            rightEnd = outsideRightPos;

            // 회전 정지
            if (leftImage != null)
                leftImage.GetComponent<UIRotate>().OnExit();
            if (rightImage != null)
                rightImage.GetComponent<UIRotate>().OnExit();
        }

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);

            float eased = Mathf.SmoothStep(0f, 1f, t);

            if (leftImage != null)
            {
                leftImage.anchoredPosition = Vector2.Lerp(leftStart, leftEnd, eased);
            }

            if (rightImage != null)
            {
                rightImage.anchoredPosition = Vector2.Lerp(rightStart, rightEnd, eased);
            }

            

            yield return null;
        }

        if(leftImage != null)
            leftImage.anchoredPosition = leftEnd;
        if (rightImage != null)
            rightImage.anchoredPosition = rightEnd;

        if (is_in) 
        {
            // 회전 시작
            if (leftImage != null)
                leftImage.GetComponent<UIRotate>().OnArrived();
            if (rightImage != null)
                rightImage.GetComponent<UIRotate>().OnArrived();
        }
    }

    public void ChangeBackground(Sprite nextSprite)
    {
        if (_isPlaying)
            return;

        
        StartCoroutine(ScrollCoroutine(nextSprite));
    }

    public IEnumerator ScrollCoroutine(Sprite nextSprite)
    {
        if (nextSprite == null)
            yield break;

        _isPlaying = true;

        bgNext.GetComponent<Image>().sprite = nextSprite;

        float height = bgCurrent.rect.height;

        Vector2 curStart = Vector2.zero;
        Vector2 curEnd = Vector2.up * height;

        Vector2 nextStart = Vector2.down * height;
        Vector2 nextEnd = Vector2.zero;

        bgCurrent.anchoredPosition = curStart;
        bgNext.anchoredPosition = nextStart;

        float time = 0f;

        while (time < _bgSwitchDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / _bgSwitchDuration);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            bgCurrent.anchoredPosition =
                Vector2.Lerp(curStart, curEnd, eased);
            bgNext.anchoredPosition =
                Vector2.Lerp(nextStart, nextEnd, eased);

            yield return null;
        }

        bgCurrent.anchoredPosition = curEnd;
        bgNext.anchoredPosition = nextEnd;

        // 역할 교체
        (bgCurrent, bgNext) = (bgNext, bgCurrent);

        _isPlaying = false;
    }

    public void UpdateEnemyImages(EnemyData enemyData, float curePercentage)
    {
        if (enemyData == null) return;

        UpdateSingleImage(leftImage, enemyData.LeftEnemySprites, curePercentage);
        UpdateSingleImage(rightImage, enemyData.RightEnemySprites, curePercentage);
    }

    private void UpdateSingleImage(RectTransform targetRect, System.Collections.Generic.List<Sprite> sprites, float percent)
    {
        if (targetRect == null) return;
        Image img = targetRect.GetComponent<Image>();
        if (img == null) return;  

        if (sprites == null || sprites.Count == 0)
        {
            img.sprite = null;
            Color c = img.color;
            c.a = 0;
            img.color = c;
            return;
        } 
        Color color = img.color;
        color.a = 1f;
        img.color = color;
        img.sprite = GetSpriteByPercentage(sprites, percent);
    }
    private Sprite GetSpriteByPercentage(System.Collections.Generic.List<Sprite> sprites, float percent)
    {
        if (sprites == null || sprites.Count == 0) return null;
        if (sprites.Count == 1)  return sprites[0];
        int maxIndex = sprites.Count - 1;
        int index = Mathf.FloorToInt(percent * maxIndex);
        index = Mathf.Clamp(index, 0, maxIndex);

        return sprites[index];
    }

    public void StopSlider()
    {
        if (_timelineSlider != null)
            _timelineSlider.Stop();
        if (_roundSlider != null)
            _roundSlider.Stop();
    }

    // 커버 초기화
    IEnumerator SetCover()
    {
        _prevCover = _coverEffects[0];
        _currentCover = _coverEffects[1];
        _nextCover = _coverEffects[2];

        //열림
        //yield return StartCoroutine(_currentCover.Play_Effect(Direction.Down, 0.5f, false));
        _currentCover.SetActive(false);
        yield return null;
        _timelineUI.SetActive_Slots(true);
    }

    private void SwitchCover()
    {
        GameObject temp = _prevCover;
        _prevCover = _currentCover;
        _currentCover = _nextCover;
        _nextCover = temp;
    }
}

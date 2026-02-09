using Sirenix.OdinInspector;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public enum EndCondition
{
    Victory,     // 승리
    Dead,        // 사망
    RoundOver,   // 라운드 초과
}

[System.Serializable]
public class VictoryUIConfig
{
    [LabelText("난이도")] public Difficulty difficulty;
    [LabelText("스테이지 번호")] public int stageNumber;

    [Header("이미지 설정")]
    [LabelText("결과 스프라이트")] public Sprite resultSprite;

    [Header("오브젝트 설정")]
    [LabelText("활성화 텍스트 오브젝트")] public GameObject textBoxObject;

    [Header("텍스트 컴포넌트 연결")]
    [LabelText("제목 텍스트(CopyText)")] public TextMeshProUGUI titleText;
    [LabelText("서브타이틀(Day/Week)")] public TextMeshProUGUI subTitleText;

    [Header("텍스트 설정")]
    [LabelText("플레이스홀더 문구")] public LocalizedString placeholderLocalizedKey;
}

public class BattleResultUIController : MonoBehaviour
{
    [Header("패널 연결")]
    [SerializeField] private RenderingPanel _renderingPanel;
    [SerializeField] private GameObject _victoryPanel;
    [SerializeField] private GameObject _deadDefeatPanel;
    [SerializeField] private GameObject _roundOverDefeatPanel;

    [Header("에러 프리징 연출 설정")]
    [SerializeField] private GameObject _freezePanel;
    [SerializeField] private CanvasGroup _freezeCanvasGroup;
    [SerializeField] private float _freezeDuration = 2.0f;
    [SerializeField] private float _freezeFadeDuration = 0.5f;

    [Header("버튼 연결")]
    [SerializeField] private Button _victoryHomeButton;
    [SerializeField] private Button _deadDefeatHomeButton;
    [SerializeField] private Button _roundOverDefeatHomeButton;
    //[SerializeField] private Button _retryButton;

    [Header("텍스트 연결")]
    [SerializeField] private TextMeshProUGUI _victoryStageText;
    //[SerializeField] private TextMeshProUGUI _defeatStageText;

    [Header("연출 설정")]
    [SerializeField] private float _delayBeforeResult = 1.5f;

    [Header("Day 연출 설정")]
    [SerializeField] private CanvasGroup _fadeCanvasGroup;
    [SerializeField] private TextMeshProUGUI _dayCountText;
    [SerializeField] private float _countUpDuration = 1.0f;
    [SerializeField] private float _waitInBlack = 0.5f;

    [Header("씬 설정")]
    [SerializeField] private string _easyMainSceneName = "MainScene";
    [SerializeField] private string _hardMainSceneName = "hardMainScene";
    [SerializeField] private string _battleSceneName = "BattleScene";
    [SerializeField] private string _easyEndSceneName = "EasyOutro";
    [SerializeField] private string _hardEndSceneName = "HardOutro";

    [SerializeField]
    [TableList(AlwaysExpanded = true)]
    private List<VictoryUIConfig> _victoryUIConfigs = new List<VictoryUIConfig>();

    [Header("승리 UI 오브젝트 연결")]
    [SerializeField] private Image _victoryResultImage;
    [SerializeField] private TMP_InputField _mainInputField;
    [SerializeField] private TextMeshProUGUI _countText;

    [Header("글자수 제한 설정")]
    [SerializeField] private int _maxCharacterLimit = 20;

    [Header("현지화 설정")]
    [SerializeField] private string _tableName = "UI Table";
    [SerializeField] private string _easySubTitleFormatKey = "UI_VICTORY_EASY_SUBTITLE";
    [SerializeField] private string _hardSubTitleFormatKey = "UI_VICTORY_HARD_SUBTITLE";

    private TextMeshProUGUI _currentActiveCopyText;
    private LocalizedString _currentPlaceholderLocalized;
    private TextMeshProUGUI _currentActiveSubTitleText; 

    private void Awake()
    {
        if (_victoryHomeButton != null)
        {
            _victoryHomeButton.onClick.AddListener(GoToTitleVictory);
        }
        if (_deadDefeatHomeButton != null)
        {
            _deadDefeatHomeButton.onClick.AddListener(GoToTitle);
        }
        if (_roundOverDefeatHomeButton != null)
        {
            _roundOverDefeatHomeButton.onClick.AddListener(GoToTitle);
        }

        if (_mainInputField != null)
        {
            _mainInputField.characterLimit = _maxCharacterLimit;
            _mainInputField.onValueChanged.AddListener(HandleInputChanged);
        }
    }
    private void OnEnable()
    {
        
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBattleEnded += HandleBattleEnded;
            Debug.Log("구독 성공했어용");
        }

        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnBattleEnded -= HandleBattleEnded;

        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale)
    {
        if (_victoryPanel != null && _victoryPanel.activeSelf)
        {
            RefreshLocalizedTexts();
        }
    }

    private void RefreshLocalizedTexts()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentStageData == null) return;
        int currentStageNum = GameManager.Instance.CurrentStageData.StageNumber;

        if (_currentActiveSubTitleText != null)
        {
            string key = (ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Easy)
                         ? _easySubTitleFormatKey : _hardSubTitleFormatKey;

            _currentActiveSubTitleText.text = LocalizationSettings.StringDatabase.GetLocalizedString(
                _tableName, key, arguments: new object[] { currentStageNum });
        }

        if (_currentPlaceholderLocalized != null && !_currentPlaceholderLocalized.IsEmpty)
        {
            SetPlaceholderText(_currentPlaceholderLocalized.GetLocalizedString());
        }
    }

    private void HandleBattleEnded(EndCondition victory)
    {
        StartCoroutine(ProcessResultSequence(victory));
    }

    private IEnumerator ProcessResultSequence(EndCondition victory)
    {
        yield return new WaitForSeconds(_delayBeforeResult);

        //배틀 타임스케일 초기화
        Time.timeScale = 1f;
        Debug.Log("[BattleResultUIController] 전투 종료 - 배틀 타임스케일 초기화");

        UpdateStageInfo();

        if (victory == EndCondition.Victory)  // 승리시
        {
            if (_renderingPanel != null)
            {
                _renderingPanel.StartRendering(() =>
                {
                    ShowResultPanel(victory);
                });
            }
            else
            {
                ShowResultPanel(victory);
            }
        }
        else  // 어떻게든 패배 시
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.StopBGM();
            }
            if (ServiceLocator.Instance != null && ServiceLocator.Instance.Cursor != null)
            {
                ServiceLocator.Instance.Cursor.StartAnimation("Loading");
            }
            if (_freezePanel != null)
            {
                _freezePanel.SetActive(true);
                if (_freezeCanvasGroup != null)
                {
                    _freezeCanvasGroup.alpha = 0f;
                    float elapsed = 0f;
                    while (elapsed < _freezeDuration)
                    {
                        elapsed += Time.deltaTime;
                        _freezeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / _freezeFadeDuration);
                        yield return null;
                    }
                    _freezeCanvasGroup.alpha = 1f;
                }
            }
            
            yield return new WaitForSeconds(_freezeDuration);
            if (ServiceLocator.Instance != null && ServiceLocator.Instance.Cursor != null)
            {
                ServiceLocator.Instance.Cursor.StopAnimation();
            }
            ShowResultPanel(victory);
        }
    }
    private void UpdateStageInfo()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentStageData != null)
        {
            int stageNum = GameManager.Instance.CurrentStageData.StageNumber;
            if (_victoryStageText != null)
                _victoryStageText.text = $"[ Clear_Run_Stage_{stageNum:D2}.mp4 ]";
        }
    }
    private void ShowResultPanel(EndCondition victory)
    {
        if (victory == EndCondition.Victory)
        {
            SetupVictoryUI();
            if (_victoryPanel != null) _victoryPanel.SetActive(true);

            if (SoundManager.Instance != null)
                SoundManager.Instance.Play(SoundID.UI_Clear);
        }
        else if (victory == EndCondition.Dead)
        {
            if (_deadDefeatPanel != null) _deadDefeatPanel.SetActive(true);

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.Play(SoundID.UI_Error);
            }

        }
        else if (victory == EndCondition.RoundOver)
        {
            if (_roundOverDefeatPanel != null) _roundOverDefeatPanel.SetActive(true);

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.StopBGM();
                SoundManager.Instance.Play(SoundID.UI_Error);
            }
        }
    }
    public void GoToTitle()
    {
        GameManager.SelectedStageID = 0;
        if (ServiceLocator.Instance != null && ServiceLocator.Instance.Scene != null)
        {
            if (ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Easy)
            {
                ServiceLocator.Instance.Scene.Load(_easyMainSceneName);
            }
            else if (ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Hard)
            {
                ServiceLocator.Instance.Scene.Load(_hardMainSceneName);
            }
        }
    }

    public void GoToTitleVictory()
    {
        _victoryHomeButton.interactable = false;

        SaveInputToTwitData();
        TwitSaveService.Save(ServiceLocator.Instance.CurrentTwitData);
        ServiceLocator.Instance.SaveNowUserData();
        UnlockStageAchivement();

        StartCoroutine(ClearSequenceAndLoadAsync());
    }

    private void SaveInputToTwitData()
    {
        if (ServiceLocator.Instance == null || ServiceLocator.Instance.CurrentTwitData == null) return;
        if (GameManager.Instance == null || GameManager.Instance.CurrentStageData == null) return;

        int currentStageNum = GameManager.Instance.CurrentStageData.StageNumber;
        Difficulty currentDifficulty = ServiceLocator.Instance.CurrentUser.Difficulty;

        int nextDayNum = currentStageNum + 1;

        string finalTitle = _mainInputField.text;

        if (string.IsNullOrWhiteSpace(finalTitle))
        {
            var placeholderComponent = _mainInputField.placeholder as TextMeshProUGUI;
            if (placeholderComponent != null)
            {
                finalTitle = placeholderComponent.text;
            }
        }

        var targets = ServiceLocator.Instance.CurrentTwitData.TwitDatas
        .Where(t => t.UploadDay == nextDayNum && t.Difficulty == currentDifficulty)
        .ToList();

        if (targets.Count > 0)
        {
            foreach (var twit in targets)
            {
                twit.videoTitle = finalTitle;
                twit.IsVisible = true;
                if (twit.TwitID == 100)
                {
                    twit.IsLiked = true;
                }
            }

            Debug.Log($"[BattleResult] 클리어 스테이지: {currentStageNum}, 트윗 업로드 날짜: {nextDayNum}");
            Debug.Log($"[BattleResult] '{finalTitle}' 제목으로 {targets.Count}개 트윗 활성화 완료");
        }
    }

    public void RetryStage()
    {
        if (ServiceLocator.Instance != null && ServiceLocator.Instance.Scene != null)
        {
            ServiceLocator.Instance.Scene.Load(_battleSceneName);
        }
    }

    private void SetupVictoryUI()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentStageData == null) return;

        Difficulty currentDiff = ServiceLocator.Instance.CurrentUser.Difficulty;
        int currentStage = GameManager.Instance.CurrentStageData.StageNumber;

        _currentActiveCopyText = null;
        _currentActiveSubTitleText = null;

        foreach (var config in _victoryUIConfigs)
        {
            if (config.textBoxObject != null)
                config.textBoxObject.SetActive(false);
        }

        var targetConfig = _victoryUIConfigs.Find(x => x.difficulty == currentDiff && x.stageNumber == currentStage);

        if (targetConfig != null)
        {
            if (_victoryResultImage != null && targetConfig.resultSprite != null)
            {
                _victoryResultImage.sprite = targetConfig.resultSprite;
            }
            if (targetConfig.textBoxObject != null)
            {
                targetConfig.textBoxObject.SetActive(true);
                _currentActiveCopyText = targetConfig.titleText;
                _currentActiveSubTitleText = targetConfig.subTitleText;
            }

            _currentPlaceholderLocalized = targetConfig.placeholderLocalizedKey;
            RefreshLocalizedTexts();

            if (_mainInputField != null)
            {
                _mainInputField.text = "";
                HandleInputChanged("");
            }
            Debug.Log($"[BattleResult] {currentDiff} 난이도 {currentStage} 스테이지 UI 설정 완료");
        }
        else
        {
            Debug.LogWarning($"[BattleResult] 해당 스테이지({currentStage})에 대한 UI 설정 데이터가 없습니다.");
        }
    }

    private void HandleInputChanged(string input)
    {
        if (_currentActiveCopyText != null)
        {
            if (string.IsNullOrWhiteSpace(input) && _mainInputField != null && _mainInputField.placeholder != null)
            {
                var placeholderComponent = _mainInputField.placeholder.GetComponent<TextMeshProUGUI>();
                if (placeholderComponent != null)
                {
                    _currentActiveCopyText.text = placeholderComponent.text;
                }
            }
            else
            {
                _currentActiveCopyText.text = input;
            }
        }

        if (_countText != null)
        {
            _countText.text = $"{input.Length} / {_maxCharacterLimit}";
        }
    }

    public void SetPlaceholderText(string newText)
    {
        if (_mainInputField != null && _mainInputField.placeholder != null)
        {
            var placeholderText = _mainInputField.placeholder.GetComponent<TextMeshProUGUI>();
            if (placeholderText != null)
            {
                placeholderText.text = newText;
            }
        }
    }

    #region Coroutine

    private IEnumerator ClearSequenceAndLoadAsync()
    {
        int currentDay = 0;
        int nextDay = 1;
        int totalStages = 0;
        string targetSceneName = "";

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBGM();
        }

        if (ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Easy)
        {
            targetSceneName = _easyMainSceneName;
        }
        else if (ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Hard)
        {
            targetSceneName = _hardMainSceneName;
        }

        if (GameManager.Instance != null && GameManager.Instance.CurrentStageData != null)
        {
            currentDay = GameManager.Instance.CurrentStageData.StageNumber;
            nextDay = currentDay + 1;
            if (ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Easy)
            {
                _dayCountText.text = $"Day {currentDay:D2}";
            }
            else if (ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Hard)
            {
                _dayCountText.text = $"Week {currentDay:D2}";
            }
        }
        if (ServiceLocator.Instance.CurrentRepository != null)
        {
            totalStages = ServiceLocator.Instance.CurrentRepository.stageDatas.Count;
        }
        if (currentDay >= totalStages)
        {
            if (ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Easy)
            {
                TwitSaveService.SetVisibleForEasyContinue(ServiceLocator.Instance.CurrentTwitData);
                targetSceneName = _easyEndSceneName;
            }
            else if (ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Hard)
            {
                TwitSaveService.SetVisibleForHardContinue(ServiceLocator.Instance.CurrentTwitData);
                targetSceneName = _hardEndSceneName;
            }
            Debug.Log("마지막 스테이지 클리어. 엔딩씬으로 넘어갑니다.");
        }
        AsyncOperation asyncLoad = null;
        if (ServiceLocator.Instance != null && ServiceLocator.Instance.Scene != null)
        {
            asyncLoad = ServiceLocator.Instance.Scene.LoadAsync(targetSceneName);
            asyncLoad.allowSceneActivation = false; // 로딩이 끝나도 바로 씬을 바꾸지 않음
        }
        if (_fadeCanvasGroup != null)
        {
            _fadeCanvasGroup.gameObject.SetActive(true);
            float elapsed = 0f;
            while (elapsed < _countUpDuration)
            {
                elapsed += Time.deltaTime;
                _fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / _countUpDuration);
                yield return null;
            }
        }

        float e = 0f;
        while (e < _countUpDuration)
        {
            e += Time.deltaTime;
            int displayDay = (int)Mathf.Lerp(currentDay, nextDay, e/_countUpDuration);
            if (_dayCountText != null)
            {
                if (ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Easy)
                {
                    _dayCountText.text = $"Day {displayDay:D2}";
                }
                else if (ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Hard)
                {
                    _dayCountText.text = $"Week {displayDay:D2}";
                }
            }

            yield return null;
        }
        if (_dayCountText != null)
        {
            if (ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Easy)
            {
                _dayCountText.text = $"Day {nextDay:D2}";
            }
            else if (ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Hard)
            {
                _dayCountText.text = $"Week {nextDay:D2}";
            }
        }
        
        yield return new WaitForSeconds(_waitInBlack);
        while (asyncLoad != null && asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);

        if (ServiceLocator.Instance != null && ServiceLocator.Instance.CurrentUser != null)
        {
            SaveService.Save(ServiceLocator.Instance.CurrentUser);
            Debug.Log("[BattleResultUIController] 연출 종료 및 씬 전환 전 최종 저장 완료");
        }

        if (asyncLoad != null)
            asyncLoad.allowSceneActivation = true;

    }

    #endregion

    #region Achivement Methods
    private void UnlockStageAchivement()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentStageData != null)
        {
            int stageNum = GameManager.Instance.CurrentStageData.StageNumber;
            string achievementKey = "";
            if (ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Easy)
            {
                achievementKey = $"NEW_ACHIEVEMENT_{stageNum}_0";
            }
            else
            {
                achievementKey = $"NEW_ACHIEVEMENT_{stageNum}_1";
            }

            SteamAchievementManager.Unlock(achievementKey);
        } 
    }

    #endregion

    [Button("승리 연출 테스트", ButtonSizes.Medium)]
    private void TestVictory() => HandleBattleEnded(EndCondition.Victory);

    [Button("죽음 패배 연출 테스트", ButtonSizes.Medium)]
    private void TestDeadDefeat() => HandleBattleEnded(EndCondition.Dead);

    [Button("라운드 오버 패배 연출 테스트", ButtonSizes.Medium)]
    private void TestRoundOverDefeat() => HandleBattleEnded(EndCondition.RoundOver);

}

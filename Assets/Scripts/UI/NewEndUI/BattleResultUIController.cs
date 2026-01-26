using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using System.Collections;
using Steamworks;

public enum EndCondition
{
    Victory,     // 승리
    Dead,        // 사망
    RoundOver,   // 라운드 초과
}

public class BattleResultUIController : MonoBehaviour
{
    [Header("패널 연결")]
    [SerializeField] private RenderingPanel _renderingPanel;
    [SerializeField] private GameObject _victoryPanel;
    [SerializeField] private GameObject _deadDefeatPanel;
    [SerializeField] private GameObject _roundOverDefeatPanel;

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
        if ( _roundOverDefeatHomeButton != null)
        {
            _roundOverDefeatHomeButton.onClick.AddListener(GoToTitle);
        }
    }
    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnBattleEnded += HandleBattleEnded;
    }


    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnBattleEnded -= HandleBattleEnded;
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
        else if (victory == EndCondition.Dead)
        {
            // 패배 시엔 렌더링 패널 없이 바로 띵~
            ShowResultPanel(victory);
        }
        else if (victory == EndCondition.RoundOver)
        {
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
            if (_victoryPanel != null) _victoryPanel.SetActive(true);
        }
        else if (victory == EndCondition.Dead)
        {
            if (_deadDefeatPanel != null) _deadDefeatPanel.SetActive(true);
        }
        else if( victory == EndCondition.RoundOver)
        {
            if (_roundOverDefeatPanel != null) _roundOverDefeatPanel.SetActive(true);
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
            else if(ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Hard)
            {
                ServiceLocator.Instance.Scene.Load(_hardMainSceneName);
            }
        }
    }

    public void GoToTitleVictory()
    {
        _victoryHomeButton.interactable = false;

        UnlockStageAchivement();

        StartCoroutine(ClearSequenceAndLoadAsync());
    }

    public void RetryStage()
    {
        if (ServiceLocator.Instance != null && ServiceLocator.Instance.Scene != null)
        {
            ServiceLocator.Instance.Scene.Load(_battleSceneName);
        }
    }

    #region Coroutine

    private IEnumerator ClearSequenceAndLoadAsync()
    {
        int currentDay = 0;
        int nextDay = 1;
        int totalStages = 0;
        string targetSceneName = "";

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
                targetSceneName = _easyEndSceneName;
            }
            else if (ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Hard)
            {
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

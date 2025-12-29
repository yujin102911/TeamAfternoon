using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using System.Collections;

public class BattleResultUIController : MonoBehaviour
{
    [Header("패널 연결")]
    [SerializeField] private RenderingPanel _renderingPanel;
    [SerializeField] private GameObject _victoryPanel;
    [SerializeField] private GameObject _defeatPanel;

    [Header("버튼 연결")]
    [SerializeField] private Button _victoryHomeButton;
    [SerializeField] private Button _defeatHomeButton;
    [SerializeField] private Button _retryButton;

    [Header("텍스트 연결")]
    [SerializeField] private TextMeshProUGUI _victoryStageText;
    [SerializeField] private TextMeshProUGUI _defeatStageText;


    [Header("연출 설정")]
    [SerializeField] private float _delayBeforeResult = 1.5f;

    [Header("씬 설정")]
    [SerializeField] private string _titleSceneName = "TitleScene";
    [SerializeField] private string _battleSceneName = "BattleScene";

    private void Awake()
    {
        if (_victoryHomeButton != null)
        {
            _victoryHomeButton.onClick.AddListener(GoToTitle);
        }
        if (_defeatHomeButton != null)
        {
            _defeatHomeButton.onClick.AddListener(GoToTitle);
        }
        if (_retryButton != null)
        {
            _retryButton.onClick.AddListener(RetryStage);
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
    private void HandleBattleEnded(bool isVictory, int round, int hitCount, int attackCount, int leftHP)
    {
        StartCoroutine(ProcessResultSequence(isVictory));
    }
    private IEnumerator ProcessResultSequence(bool isVictory)
    {
        // 1. 설정한 시간만큼 대기 (게임 상황을 잠시 보여줌)
        yield return new WaitForSeconds(_delayBeforeResult);

        // 2. 스테이지 정보 갱신
        UpdateStageInfo();

        // 3. 결과에 따른 연출 분기
        if (isVictory)
        {
            if (_renderingPanel != null)
            {
                // RenderingPanel 내부에서도 시간 설정(_totalDuration)에 따라 블록이 차오름
                _renderingPanel.StartRendering(() =>
                {
                    ShowResultPanel(true);
                });
            }
            else
            {
                ShowResultPanel(true);
            }
        }
        else
        {
            // 패배 시에도 바로 패널이 뜨는 대신 대기 후 출력
            ShowResultPanel(false);
        }
    }
    private void UpdateStageInfo()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentStageData != null)
        {
            int stageNum = GameManager.Instance.CurrentStageData.StageNumber;
            string stageName = GameManager.Instance.CurrentStageData.StageName;
            if (_victoryStageText != null)
                _victoryStageText.text = $"[ Clear_Run_Stage_{stageNum:D2}.mp4 ]";
            if (_defeatStageText != null)
                _defeatStageText.text = $"[ Fail_Run_Stage_{stageNum:D2}.mp4 ]";
        }
    }
    private void ShowResultPanel(bool isVictory)
    {
        if (isVictory)
        {
            if (_victoryPanel != null) _victoryPanel.SetActive(true);
        }
        else
        {
            if (_defeatPanel != null) _defeatPanel.SetActive(true);
        }
    }
    public void GoToTitle()
    {
        GameManager.SelectedStageID = 0;
        if (ServiceLocator.Instance != null && ServiceLocator.Instance.Scene != null)
        {
            ServiceLocator.Instance.Scene.Load(_titleSceneName);
        }
    }
    public void RetryStage()
    {
        if (ServiceLocator.Instance != null && ServiceLocator.Instance.Scene != null)
        {
            ServiceLocator.Instance.Scene.Load(_battleSceneName);
        }
    }

    [Button("승리 연출 테스트", ButtonSizes.Medium)]
    private void TestVictory() => HandleBattleEnded(true, 1, 0, 0, 20);

    [Button("패배 연출 테스트", ButtonSizes.Medium)]
    private void TestDefeat() => HandleBattleEnded(false, 1, 0, 0, 0);

}

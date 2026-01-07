using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using System.Collections;

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

    [Header("씬 설정")]
    [SerializeField] private string _titleSceneName = "TitleScene";
    [SerializeField] private string _battleSceneName = "BattleScene";

    private void Awake()
    {
        if (_victoryHomeButton != null)
        {
            _victoryHomeButton.onClick.AddListener(GoToTitle);
        }
        if (_deadDefeatHomeButton != null)
        {
             _deadDefeatHomeButton.onClick.AddListener(GoToTitle);
        }
        if ( _roundOverDefeatHomeButton != null)
        {
            _roundOverDefeatHomeButton.onClick.AddListener(GoToTitle);
        }
        //if (_retryButton != null)
        //{
        //    _retryButton.onClick.AddListener(RetryStage);
        //}
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
            string stageName = GameManager.Instance.CurrentStageData.StageName;
            if (_victoryStageText != null)
                _victoryStageText.text = $"[ Clear_Run_Stage_{stageNum:D2}.mp4 ]";
            //if (_defeatStageText != null)
            //    _defeatStageText.text = $"[ Fail_Run_Stage_{stageNum:D2}.mp4 ]";
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
    private void TestVictory() => HandleBattleEnded(EndCondition.Victory);

    [Button("죽음 패배 연출 테스트", ButtonSizes.Medium)]
    private void TestDeadDefeat() => HandleBattleEnded(EndCondition.Dead);

    [Button("라운드 오버 패배 연출 테스트", ButtonSizes.Medium)]
    private void TestRoundOverDefeat() => HandleBattleEnded(EndCondition.RoundOver);

}

using UnityEngine.UI;
using UnityEngine;

public class ContractPanel : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private Toggle _yesToggle;
    [SerializeField] private Toggle _noToggle;
    [SerializeField] private Button _finishButton;

    [Header("씬 이름 설정")]
    [SerializeField] private string _tutorialSceneName = "TutorialScene";
    [SerializeField] private string _mainSceneName = "MainScene";

    [Header("아니오일때의 Popup")]
    [SerializeField] private GameObject _realPopup;
    [SerializeField] private Button _passButton;
    [SerializeField] private Button _goTutorialButton;

    private void Awake()
    {
        _finishButton.onClick.AddListener(OnFinishButtonClick);

        // 팝업 경고 버튼 이벤트 연결
        _passButton.onClick.AddListener(GoWithoutTutorial);
        _goTutorialButton.onClick.AddListener(GoTutorial);
    }
    public void OnFinishButtonClick()
    {
        if (_yesToggle.isOn)
        {
            Debug.Log("튜토리얼 설치 선택");
            GoTutorial();
        }
        else if (_noToggle.isOn)
        {
            Debug.Log("설치하지 않음 선택");
            GoWithoutTutorial();
        }
    }

    /// <summary>
    /// 어떤 방식이든 튜토리얼을 한다고 선택했을 때 실행되는 로직
    /// </summary>
    private void GoTutorial()
    {
        Debug.Log("튜토리얼을 시작합니다.");
        // 튜토리얼 시작 로직 (저장 X)
    }

    /// <summary>
    /// 튜토리얼을 하지 않는다고 선택했을 때 실행되는 로직
    /// </summary>
    private void GoWithoutTutorial()
    {
        Debug.Log("튜토리얼을 진행하지 않고 게임을 시작합니다.");
        ServiceLocator.Instance.SaveNowUserData(); // 새로운 게임 데이터 생성
        // 추가로 UserData의 Tutorial클리어 처리 여부도 여기서 결정
    }

}

using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ContractPanel : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private Toggle _yesToggle;
    [SerializeField] private Toggle _noToggle;
    [SerializeField] private Button _finishButton;
    [SerializeField] private IntroPanel _introPanel;

    [Header("씬 이름 설정")]
    [SerializeField] private string _mainSceneName = "MainScene";

    [Header("아니오일때의 Popup")]
    [SerializeField] private GameObject _realPopup;
    [SerializeField] private Button _passButton;
    [SerializeField] private Button _goTutorialButton;
    [SerializeField] private List<int> _addBlocks;

    [Header("Nail 설치마법사 Panel")]
    [SerializeField] private GameObject _nailPanel;

    private bool _isSceneLoading = false;

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
            _realPopup.SetActive(true);
        }
    }

    public void GoTutorial()
    {
        StartCoroutine(GoTutorialRoutine());
    }

    /// <summary>
    /// 어떤 방식이든 튜토리얼을 한다고 선택했을 때 실행되는 로직
    /// </summary>
    private IEnumerator GoTutorialRoutine()
    {
        Debug.Log("튜토리얼을 시작합니다.");
        ServiceLocator.Instance.Cursor.StartAnimation("Loading");
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
        }

        yield return new WaitForSeconds(2f);

        ServiceLocator.Instance.Cursor.StopAnimation();
        _nailPanel.SetActive(true);
        gameObject.SetActive(false);

    }

    /// <summary>
    /// 튜토리얼을 하지 않는다고 선택했을 때 실행되는 로직
    /// </summary>
    private void GoWithoutTutorial()
    {
        Debug.Log("튜토리얼을 진행하지 않고 게임을 시작합니다.");
        if (ServiceLocator.Instance.CurrentUser != null && _addBlocks != null)
        {
            foreach (int block in _addBlocks)
            {
                ServiceLocator.Instance.CurrentUser.AddUnlockedBlock(block);
                ServiceLocator.Instance.SetTutorialClear(true);
            }
        }
        ServiceLocator.Instance.SaveNowUserData(); // 새로운 게임 데이터 저장
        StartCoroutine(End());

        //ServiceLocator.Instance.Scene.Load(_mainSceneName);
        // 추가로 UserData의 Tutorial클리어 처리 여부도 여기서 결정 + block 추가도 여기서 ~.~
    }

    private IEnumerator End()
    {
        if (_isSceneLoading) yield break;

        _isSceneLoading = true;

        AsyncOperation asyncLoad = null;
        if (ServiceLocator.Instance != null && ServiceLocator.Instance.Scene != null)
        {
            asyncLoad = ServiceLocator.Instance.Scene.LoadAsync(_mainSceneName);
            asyncLoad.allowSceneActivation = false;
        }
        yield return new WaitForSeconds(0.5f);
        _introPanel.StartDayChange(0, 1);
        yield return _introPanel.FadeOutRoutine();

        if (asyncLoad != null)
            asyncLoad.allowSceneActivation = true;
    }

}

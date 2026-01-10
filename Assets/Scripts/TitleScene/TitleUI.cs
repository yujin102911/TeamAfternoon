using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using TMPro;

public class TitleUI : MonoBehaviour
{
    [Header("메인 패널들")]
    [SerializeField] private GameObject _mainMenuPanel;       // 메인 버튼들이 있는 패널
    [SerializeField] private GameObject _newGameConfigPanel;  // 난이도 드롭다운이 있는 패널
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _continuePanel;

    [Header("메인메뉴 버튼")]
    [SerializeField] private Button _newGameButton;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitButton;

    [Header("새 게임 패널 UI")]
    [SerializeField] private TMP_Dropdown _difficultyDropdown;
    [SerializeField] private Button _startGameButton;         // 난이도 선택 후 진짜 시작하는 버튼
    [SerializeField] private Button _backToMainFromNewGame;   // 메인으로 돌아가기

    [Header("이어하기 패널 UI")]
    [SerializeField] private Button _realContinueButton;
    [SerializeField] private Button _backToMainFromContinue; // 메인으로 돌아가기
    [SerializeField] private TextMeshProUGUI _lastStageInfoText;
    [SerializeField] private TextMeshProUGUI _saveDateText;

    [Header("확인 패널 UI")]
    [SerializeField] private GameObject _confirmPopup;
    [SerializeField] private Button _confirmYesButton;
    [SerializeField] private Button _confirmNoButton;

    [Header("씬 설정")]
    [SerializeField] private string _nextSceneName = "DesktopScene";


    private void Awake()
    {
        // 메인 메뉴 버튼 이벤트
        _newGameButton.onClick.AddListener(ShowNewGameConfig);
        _continueButton.onClick.AddListener(ShowContinueConfig);
        _settingsButton.onClick.AddListener(() => _settingsPanel.SetActive(true));
        _quitButton.onClick.AddListener(OnClickQuit);

        // 새 게임 설정 UI 이벤트
        _startGameButton.onClick.AddListener(OnClickStartWithDifficulty);
        _backToMainFromNewGame.onClick.AddListener(ShowMainMenu);

        // 이어하기 UI 이벤트
        _backToMainFromContinue.onClick.AddListener(ShowMainMenu);
        _realContinueButton.onClick.AddListener(OnClickContinue);

        // 확인 팝업 이벤트
        _confirmYesButton.onClick.AddListener(StartNewGameFinal);
        _confirmNoButton.onClick.AddListener(() => _confirmPopup.SetActive(false));
    }

    private void Start()
    {
        ShowMainMenu();

        // 이어하기 버튼 활성화 상태 체크
        bool hasSave = SaveService.HasSaveData();
        _continueButton.interactable = hasSave;
        if (hasSave)
        {
            _continueButton.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        }
        else
        {
            _continueButton.GetComponent<Image>().color = new Color32(217, 217, 217, 255);
        }

        // 타이틀 브금
        if (SoundManager.Instance != null)
            SoundManager.Instance.Play(SoundID.BGM_Title);
    }

    #region Main Button Logic

    private void ShowMainMenu()
    {
        _mainMenuPanel.SetActive(true);
        _newGameConfigPanel.SetActive(false);
        _settingsPanel.SetActive(false);
        _confirmPopup.SetActive(false);
        _continuePanel.SetActive(false);
    }

    private void ShowNewGameConfig()
    {
        _mainMenuPanel.SetActive(false);
        _newGameConfigPanel.SetActive(true);
    }

    private void ShowContinueConfig()
    {
        _mainMenuPanel.SetActive(false);
        _continuePanel.SetActive(true) ;

        UpdateContinuePreview();
    }

    private void OnClickContinue()
    {
        Debug.Log("[TitleUI] 기존 데이터를 불러와 게임을 이어갑니다.");
        if (ServiceLocator.Instance.LoadGame())
        {
            ServiceLocator.Instance.Scene.Load(_nextSceneName);
        }
    }

    private void OnClickQuit()
    {
        Debug.Log("[TitleUI] 프로그램을 종료합니다.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion

    #region New Game Logic

    private void OnClickStartWithDifficulty()
    {
        // 이미 데이터가 존재하면 경고 팝업 출력
        if (SaveService.HasSaveData())
        {
            _confirmPopup.SetActive(true);
        }
        else
        {
            StartNewGameFinal();
        }
    }

    // 진짜로 새 게임을 생성하고 씬을 넘기는 최종 단계
    private void StartNewGameFinal()
    {
        //_confirmPopup.SetActive(false);

        // 드롭다운 값 읽기 (0: Easy, 1: Hard 등)
        Difficulty selectedMode = (Difficulty)_difficultyDropdown.value;

        ServiceLocator.Instance.CreateNewGame(selectedMode);
        ServiceLocator.Instance.Scene.Load(_nextSceneName);
    }

    #endregion

    #region continue Logic
    private void UpdateContinuePreview()
    {
        UserSaveDTO saveDTO = SaveService.PeekSaveData();

        if (saveDTO == null)
        {
            _lastStageInfoText.text = "기록 없음";
            _saveDateText.text = "-";
            return;
        }
        int currentDay = 1;
        if (saveDTO.Cleared_Stage_IDs != null && saveDTO.Cleared_Stage_IDs.Count > 0)
        {
            currentDay = saveDTO.Cleared_Stage_IDs.Max() + 1;
        }
        _lastStageInfoText.text = $"Day {currentDay}";

        if (File.Exists(SaveService.SavePath))
        {
            System.DateTime lastWriteTime = File.GetLastWriteTime(SaveService.SavePath);
            _saveDateText.text = lastWriteTime.ToString("yyyy.MM.dd HH:mm:ss");
        }
        else
        {
            _saveDateText.text = "-";
        }
    }
    #endregion

}

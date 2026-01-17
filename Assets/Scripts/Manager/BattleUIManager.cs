using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 시작버튼을 관리하는 스크립트,,,, 이거에 넣을게 정말 시작 버튼밖에 없단말인가
/// </summary>
public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance;

    [Header("스테이지 UI")]
    [SerializeField] private Button _startButton;

    [Header("자막 메모리 UI")]
    [SerializeField]
    private TextMeshProUGUI _storageTxtMemory;
    [SerializeField]
    private Slider _textmemorySlider;

    [Header("슬라이더 핸들 바")]
    [SerializeField]
    private Image _sliderHandleBar;

    [Header("배속 버튼")]
    [SerializeField]
    private Button _speedButton;
    [SerializeField]
    private Image _speedButtonIcon;
    [SerializeField]
    private Sprite[] _speedSprites;
    [SerializeField]
    private TextMeshProUGUI _speedTxt;
    private int _currentSpeedIndex = 0;

    private void StartSpeedButton()
    {
        if (_speedButton != null)
        {
            _currentSpeedIndex = 0;
            _speedButtonIcon.sprite = _speedSprites[_currentSpeedIndex];
            _speedButton.onClick.AddListener(() =>
            {
                if (TimelineManager.Instance != null)
                {
                    _currentSpeedIndex++;

                    switch(_currentSpeedIndex % _speedSprites.Length)
                    {
                        case 0:
                            TimelineManager.Instance.SetTimeScale(1.0f);
                            _speedButtonIcon.sprite = _speedSprites[0];
                            _speedTxt.text = $"X1";
                            break;
                        case 1:
                            TimelineManager.Instance.SetTimeScale(1.5f);
                            _speedButtonIcon.sprite = _speedSprites[1];
                            _speedTxt.text = $"X2";
                            break;
                        case 2:
                            TimelineManager.Instance.SetTimeScale(2.0f);
                            _speedButtonIcon.sprite = _speedSprites[2];
                            _speedTxt.text = $"X3";
                            break;
                    }
                }
            });
        }
    }

    private BattleSystem battleSystem;

    private void Awake()
    {
        Instance = this;
    }

    

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.BattleSystem != null)
        {
            battleSystem = GameManager.Instance.BattleSystem;

            battleSystem.OnBattleInitialized += HandleBattleInitialized;
            //battle.UpdateCureGauage += HandleCureChanged;

            battleSystem.OnCriticalChanceChanged += UpdateStackUI;

            GameManager.Instance.OnGameStateChanged += RefreshStartButtonState;
            GameManager.Instance.OnBattleEnded += RefreshStartButtonState;

            TimelineManager.Instance.OnTextMemoryChanged += Update_TextSlider;
        }
        RefreshStartButtonState();
        StartSpeedButton();
        //RefreshSectorSelectionPanel();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.BattleSystem != null)
        {
            battleSystem = GameManager.Instance.BattleSystem;
            GameManager.Instance.OnGameStateChanged -= RefreshStartButtonState;
            battleSystem.OnCriticalChanceChanged -= UpdateStackUI;
            battleSystem.OnBattleInitialized -= HandleBattleInitialized;

            TimelineManager.Instance.OnTextMemoryChanged -= Update_TextSlider;
            //battle.UpdateCureGauage -= HandleCureChanged;
        }
    }

    public void UpdateStackUI(float chance)
    {
        //_adTxt.text = $"크리티컬 확률: {Mathf.RoundToInt(chance * 100f)}%";
        //_adTxt.text = $"편집 콤보: +{(int)chance} / 최대 자막수: {(int)chance / 8}";
    }

    public void Update_TextSlider(int current, int max)
    {
        float slider_size = (float)current / max;

        _textmemorySlider.value = slider_size;
        _storageTxtMemory.text = $"{current}/{max} <size=20>mb</size>";
    }
    private void HandleBattleInitialized()
    {
        if (GameManager.Instance != null && GameManager.Instance.BattleSystem != null)
            InitializeUI(GameManager.Instance.BattleSystem);
    }

    public void InitializeUI(BattleSystem battle)
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsExecutingRound)
        {
            Hide_startBtn();
        }
        else
        {
            RefreshStartButtonState();
        }
    }

    public void RefreshStartButtonState()
    {
        if (_startButton == null || GameManager.Instance == null) return;
        bool isRoundRunning = GameManager.Instance.IsExecutingRound;
        bool isSequencePlaying = GameManager.Instance.IsSequencePlaying;
        bool isSectorSelected = GameManager.Instance.IsSectorSelected;
        bool isGameOver = GameManager.Instance.IsBattleEnded;

        bool interactable = !isRoundRunning && isSectorSelected && !isGameOver && !isSequencePlaying;

        _startButton.interactable = interactable;
        if (GameManager.Instance.MapSystem != null)
        {
            GameManager.Instance.MapSystem.SetAllSectorsVisibility(interactable);
        }
    }

    private void RefreshStartButtonState(EndCondition victory)
    {
        _startButton.interactable = false;
        //Hide_startBtn();
    }


    public void Show_startBtn()
    {
        _startButton.gameObject.SetActive(true);
    }

    public void Hide_startBtn()
    {
        _startButton.gameObject.SetActive(false);
    }


    // 슬라이더 핸들 바 레이캐스트 온
    public void HandleBar_raycastOn()
    {
        _sliderHandleBar.raycastTarget = true;
    }
}

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

    [Header("메모리 UI")]
    [SerializeField] 
    private GameObject _memoryObject;
    [SerializeField]
    private TextMeshProUGUI _percentTxt;
    [SerializeField]
    private TextMeshProUGUI _storageTxt;
    [SerializeField] 
    private Slider _memorySlider;

    [Header("아드레날린 UI")]
    [SerializeField]
    private Toggle _hitToggle;
    [SerializeField]
    private Toggle _eightToggle;
    [SerializeField]
    private TextMeshProUGUI _adTxt;

    [Header("슬라이더 핸들 바")]
    [SerializeField]
    private Image _sliderHandleBar;

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
            GameManager.Instance.OnMemoryUpdate += UpdateSlider;
        }
        RefreshStartButtonState();
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
            GameManager.Instance.OnMemoryUpdate -= UpdateSlider;
            //battle.UpdateCureGauage -= HandleCureChanged;
        }
    }

    private void Update()
    {
        //_hitToggle.isOn = !TimelineManager.Instance.Is_Hit;
        //_eightToggle.isOn = TimelineManager.Instance.Is_Eight;

        
    }

    public void UpdateStackUI(float chance)
    {
        _adTxt.text = $"크리티컬 확률: {Mathf.RoundToInt(chance * 100f)}%";
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

    private void RefreshStartButtonState(bool isVictory, int a, int b, int c, int d)
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

    private void UpdateSlider(int current, int max)
    {
        int final_memory = 0;

        foreach (var effect in TimelineManager.Instance.additional_Effects)
        {
            if(effect == null) continue;
            final_memory += effect.cost;
        }

        float slider_size = (float)(8 * current + final_memory) / (8 * max);
        float percent = slider_size * 100f;

        _memorySlider.value = slider_size;
        _percentTxt.text = $"{percent}%";
        _storageTxt.text = $"{(8*current) + final_memory}/{8 * max} <size=20>mb</size>";
    }

    // 슬라이더 핸들 바 레이캐스트 온
    public void HandleBar_raycastOn()
    {
        _sliderHandleBar.raycastTarget = true;
    }
}

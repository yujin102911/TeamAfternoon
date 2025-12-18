using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance;

    [Header("스테이지 UI")]
    [SerializeField] private Button _startButton;
    [SerializeField] private GameObject _sectorSelectionPanel;
    [SerializeField] private TextMeshProUGUI _pageText;
    [SerializeField] private TextMeshProUGUI _totalPageText;
    [SerializeField] private TextMeshProUGUI _chapterText;

    [Header("정화 게이지 UI")]
    [SerializeField]
    private UnitStatusUI _cureUI;

    [Header("플레이어 UI")]
    [SerializeField] private PlayerHeartUI _playerStatusUI;

    [Header("적 UI 설정")]
    [SerializeField] private GameObject _enemyStatusPrefab;
    [SerializeField] private Transform _enemyUIContainer;

    private Dictionary<RuntimeEnemy, UnitStatusUI> _enemyUIMap = new Dictionary<RuntimeEnemy, UnitStatusUI>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.BattleSystem != null)
        {
            BattleSystem battle = GameManager.Instance.BattleSystem;

            battle.OnPlayerHPChanged += HandlePlayerHPChanged;
            battle.OnBattleInitialized += HandleBattleInitialized;
            battle.UpdateCureGauage += HandleCureChanged;

            GameManager.Instance.OnGameStateChanged += RefreshStartButtonState;
            GameManager.Instance.OnRoundChanged += HandleRoundChanged;
            GameManager.Instance.OnBattleEnded += RefreshStartButtonState;
            GameManager.Instance.OnGameStateChanged += TryBindPlayerUI;
        }
        RefreshStartButtonState();
        //RefreshSectorSelectionPanel();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.BattleSystem != null)
        {
            BattleSystem battle = GameManager.Instance.BattleSystem;
            GameManager.Instance.OnGameStateChanged -= RefreshStartButtonState;
            GameManager.Instance.OnRoundChanged -= HandleRoundChanged;
            GameManager.Instance.OnGameStateChanged -= TryBindPlayerUI;
            battle.OnPlayerHPChanged -= HandlePlayerHPChanged;
            battle.OnBattleInitialized -= HandleBattleInitialized;
            battle.UpdateCureGauage -= HandleCureChanged;
        }
    }


    private void HandleBattleInitialized()
    {
        if (GameManager.Instance != null && GameManager.Instance.BattleSystem != null)
            InitializeUI(GameManager.Instance.BattleSystem);
    }

    public void InitializeUI(BattleSystem battle)
    {
        // 플레이어 UI초기화
        if (_playerStatusUI != null)
        {
            _playerStatusUI.Init(battle.PlayerHP, battle.PlayerMaxHP);
        }
        if (_cureUI != null && battle.Enemies.Count > 0)
        {
            int maxCure = battle.Enemies[0].Data.MaxCureValue;
            // 이름 표시 기능이 UnitStatusUI에 있다면 활용 가능
            _cureUI.Init("페이지 정화 진행도:", 0, maxCure);
        }
        Show_startBtn();
    }

    private void HandleCureChanged(int current, int max)
    {
        if (_cureUI != null)
        {
            _cureUI.UpdateCureGauage(current, max);
        }
    }

    private void HandlePlayerHPChanged(int current, int max)
    {
        if (_playerStatusUI != null)
        {
            _playerStatusUI.UpdateHearts(current, max);
        }
    }

    private void HandleRoundChanged(int chapter, int page, int totalPage)
    {
        _pageText.text = $"<size=56pt>{0+page.ToString()}";
        _totalPageText.text = $"LINE";
        _chapterText.text = $"Chapter {chapter.ToString()}.";
    }
    private void RefreshStartButtonState()
    {
        if (_startButton == null || GameManager.Instance == null) return;
        bool isRoundRunning = GameManager.Instance.IsExecutingRound;
        bool isSectorSelected = GameManager.Instance.IsSectorSelected;
        bool isGameOver = GameManager.Instance.IsBattleEnded;

        bool interactable = !isRoundRunning && isSectorSelected && !isGameOver;

        _startButton.interactable = interactable;
        //_startButton.gameObject.SetActive(interactable);

        //RefreshSectorSelectionPanel();
    }

    private void RefreshStartButtonState(bool isVictory, int a, int b, int c, int d)
    {
        _startButton.interactable = false;
        Hide_startBtn();
    }

    private void TryBindPlayerUI()
    {
        var visualController = FindAnyObjectByType<PlayerVisualController>();

        if (visualController != null && _playerStatusUI != null)
        {
            Transform playerTr = visualController.CurrentPlayerTransform;
            if (playerTr != null)
            {
                _playerStatusUI.SetFollowTarget(playerTr);
            }
        } 
    }

    public void Show_startBtn()
    {
        _startButton.gameObject.SetActive(true);
    }

    public void Hide_startBtn()
    {
        _startButton.gameObject.SetActive(false);
    }
}

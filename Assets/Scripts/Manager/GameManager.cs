using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("맵 구성")]
    [SerializeField] private MapConfiguration mapConfig;
    [SerializeField] private Transform mapRootTransform;

    [Header("데이터 참조")]
    [SerializeField] private DataRepository dataRepository;
    [SerializeField] private UserGameData userGameData;

    [Header("비주얼 컨트롤러")]
    [SerializeField] private TimelineUI _timelineUI;
    [SerializeField] private MapVisualController _mapVisualController;
    [SerializeField] private PlayerVisualController _playerVisualController;

    [Header("게임 설정")]
    [SerializeField] private int _startHandSize = 5;
    [SerializeField] private int _playerMaxHP = 20;
    [SerializeField] private MapSize _mapSize = MapSize.Sectors_8;

    [Header("테스트용 스테이지 데이터")]
    [SerializeField] private StageData currentStageData;


    // System
    private DeckSystem _deckSystem;
    private BattleSystem _battleSystem;
    private MapSystem _mapSystem;

    private TimelineManager _timelineManager;

    private int _currentRound = 0;
    private bool _isExecutingRound = false;

    public DeckSystem DeckSystem => _deckSystem;
    public BattleSystem BattleSystem => _battleSystem;
    public MapSystem MapSystem => _mapSystem;
    public int CurrentRound => _currentRound;
    public bool IsExecutingRound => _isExecutingRound;

    // Events
    public event Action<int> OnRoundChanged;
    

    private void Awake()
    {
        if (Instance == null) 
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        if (dataRepository == null)
        {
            dataRepository = DataRepository.Instance;
        }
        Initialize();

    }

    private void Start()
    {
        LateInitialize();
        SetupGame();
    }

    #region Initializatioin
    /// <summary>
    /// 내부 변수 초기화 (시스템 초기화 등)
    /// </summary>
    private void Initialize()
    {
        SaveService.Save(userGameData);
        if (mapRootTransform == null) mapRootTransform = this.transform;

        _deckSystem = new DeckSystem(dataRepository, userGameData);
        _battleSystem = new BattleSystem();
        _mapSystem = new MapSystem(mapConfig, mapRootTransform);

        _mapSystem.OnSectorSelected += OnStartingSectorSelected;

        Debug.Log("[GameManager] 내부 시스템 생성 완료 (Awake)");
    }

    /// <summary>
    /// 외부 연결 초기화
    /// </summary>
    private void LateInitialize()
    {
        _timelineManager = TimelineManager.Instance;
        // 타임라인 매니저한테 배틀 시스템 전달
        if (_timelineManager != null) _timelineManager.Initialize(_battleSystem);
        else Debug.LogError("[GameManager] TimelineManager를 찾을 수 없습니다!");

        // MapVisualController 연결
        if (_mapVisualController != null)
        {
            _mapVisualController.Initialize(_mapSystem);
        }
        else
        {
            _mapVisualController = FindAnyObjectByType<MapVisualController>();
            if (_mapVisualController != null) _mapVisualController.Initialize(_mapSystem);
        }
        if (_timelineUI != null && _mapVisualController != null)
        {
            _timelineUI.OnRequestHighlight += _mapVisualController.OnRequestHighlight;
            _timelineUI.OnRequestClearHighlight += _mapVisualController.OnRequestClearHighlight;
            _battleSystem.OnEnemyAttackExecute += _mapVisualController.OnEnemyAttackVisual;
        }

        // PlayerVisualController 연결
        if (_playerVisualController != null)
        {
            _playerVisualController.Initialize(_mapSystem);
            _battleSystem.OnPlayerMoved += _playerVisualController.OnPlayerMoved;
        }
        Debug.Log("[GameManager] 외부 시스템 연결 완료 (Start)");

    }
    #endregion
    public void SetupGame()
    {
        _currentRound = 0;
        _isExecutingRound = false;
        if (_deckSystem == null)
        {
            Debug.LogError("[GameManager] 덱 시스템이 초기화되지 않았습니다");
            return;
        }
        if (_battleSystem == null)
        {
            Debug.LogError("[GameManager] 배틀 시스템이 초기화되지 않았습니다");
            return;
        }
        if (_mapSystem == null)
        {
            Debug.LogError("[GameManager] 맵 시스템이 초기화되지 않았습니다");
            return;
        }
        _mapSystem.GenerateMap(_mapSize);
        _deckSystem.InitializeDeck();

        _mapSystem.EnableSelectionMode();

    }
    public void StartNewBattle()
    {
        // 덱 드로우
        _deckSystem.DrawCards(_startHandSize);
        if (_timelineManager != null)
        {
            _timelineManager.ReceiveHand(_deckSystem.Hand);
        }
        // 적 배치
        if (currentStageData != null)
            SetupEnemiesFromStage(currentStageData);
        else
            Debug.LogError("StageData가 없습니다");
        Debug.Log("[GameManager] 전투 시작");

    }

    private void LoadEnemyPattern()
    {
        EnemyPattern pattern = ScriptableObject.CreateInstance<EnemyPattern>();
        // TODO: 스테이지 데이터에서 로드할 수 있도록 추가
        if (_timelineManager != null)
        {
            _timelineManager.SetEnemyPattern(pattern);
        }
    }

    /// <summary>
    /// 일반 패턴 중에 랜덤으로 적 패턴 가져오는 로직
    /// </summary>
    /// <param name="stage"></param>
    private void SetupEnemiesFromStage(StageData stage)
    {
        List<RuntimeEnemy> enemies = new List<RuntimeEnemy>();
        foreach (StageEnemySetup spawn in stage.EnemySpawns)
        {
            if (spawn.enemyData == null) continue;
            RuntimeEnemy newEnemy = new RuntimeEnemy(
                spawn.enemyData,
                spawn.hitSectors,
                spawn.tickRange.x,
                spawn.tickRange.y
                );
            enemies.Add(newEnemy);
        }
        _battleSystem.InitializeBattle(enemies, _playerMaxHP, _mapSystem.TotalSectors);
        UpdateEnemyPatterns();
    }

    private void UpdateEnemyPatterns()
    {
        if (_battleSystem == null || _battleSystem.Enemies == null) return;
        foreach(RuntimeEnemy enemy in _battleSystem.Enemies)
        {
            if (enemy.IsDead) continue;
            if (enemy.Data.Nomal_Patterns.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, enemy.Data.Nomal_Patterns.Count);
                enemy.SetPattern(enemy.Data.Nomal_Patterns[randomIndex]);
            }
        }
        if (_timelineManager != null)
        {
            _timelineManager.RefreshCombinedEnemyPattern();
        }
        Debug.Log("[GameManager] 적 패턴 갱신 완료");
    }

    public void ExecuteRound()
    {
        if (_isExecutingRound)
        {
            Debug.LogWarning("[GameManager] 이미 라운드가 실행 중입니다.");
            return;
        }
        StartCoroutine(ExecuteRoundCoroutine());
    }

    private IEnumerator ExecuteRoundCoroutine()
    {
        _isExecutingRound = true;
        _currentRound++;
        Debug.Log($"[GameManager] ==== 라운드 {_currentRound} 시작 ====");

        if (_timelineManager != null)
        {
            yield return StartCoroutine(_timelineManager.ExecuteTimeline());
        }

        EndRound();

        _isExecutingRound = false;
        Debug.Log($"[GameManager] ==== 라운드 {_currentRound} 종료 ====");
    }

    private void EndRound()
    {
        if (_timelineManager != null)
        {
            _timelineManager.OnRoundEnded();
        }
        if (_battleSystem != null)
        {
            _battleSystem.DecayBuffs();
        }

        UpdateEnemyPatterns();

        _deckSystem.DiscardHand();
        _deckSystem.DrawCards(_startHandSize);

        if (_timelineManager != null)
        {
            _timelineManager.ReceiveHand(_deckSystem.Hand);
        }
    }

    public void EndBattle(bool victory)
    {
        Debug.Log($"[GameManager] 전투 종료 - {(victory ? "승리" : "패배")}");
        if (victory)
        {
            // TODO: 보상 처리
        }
        SaveService.Save(userGameData);
    }

    private void OnStartingSectorSelected(int sectorNum)
    {
        if (_isExecutingRound) return;
        _mapSystem.DisableSelectionMode();
        if (_playerVisualController != null)
            _playerVisualController.SpawnPlayer(sectorNum);
        StartNewBattle();
        _battleSystem.SetPlayerStartPosition(sectorNum);
    }


}

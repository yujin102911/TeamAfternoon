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
    private int _globalTurnIndex = 0;

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
                spawn.hitSectors
                );
            enemies.Add(newEnemy);
        }
        _battleSystem.InitializeBattle(enemies, _playerMaxHP, _mapSystem.TotalSectors);
        UpdateEnemyPatterns();
    }

    private void UpdateEnemyPatterns()
    {
        if (_battleSystem == null || _battleSystem.Enemies == null) return;
        if (currentStageData == null) return;

        List<RuntimeEnemy> aliveEnemies = new List<RuntimeEnemy>();
        foreach (RuntimeEnemy e in _battleSystem.Enemies)
        {
            if (!e.IsDead) aliveEnemies.Add(e);
        }

        if (aliveEnemies.Count == 0) return;

        // 페이즈를 넘겨야 하는지 검사
        CheckAndApplyPhaseTransition(aliveEnemies);

        foreach (RuntimeEnemy e in aliveEnemies) e.SetPattern(null);

        int activeEnemyIndex = _globalTurnIndex % aliveEnemies.Count;
        RuntimeEnemy activeEnemy = aliveEnemies[activeEnemyIndex];

        EnemyPattern nextPattern = activeEnemy.GetNextPattern();
        if (nextPattern != null)
        {
            activeEnemy.SetPattern(nextPattern);
            Debug.Log($"[GameManager] 이번 턴 행동: {activeEnemy.Data.Enemy_Name} / {nextPattern.Pattern_Name}");
        }
        _globalTurnIndex++;

        if (_timelineManager !=  null) 
            _timelineManager.RefreshCombinedEnemyPattern();

        Debug.Log("[GameManager] 적 패턴 갱신 로직 완료");

    }

    private void CheckAndApplyPhaseTransition(List<RuntimeEnemy> enemies)
    {
        if (currentStageData.PhaseConditions == null) return;

        for (int i = 0; i < currentStageData.PhaseConditions.Count; i++)
        {
            PhaseTransitionData condition = currentStageData.PhaseConditions[i];
            bool isMet = false;

            if (condition.ConditionType == PhaseConditionType.EnemyCount)
            {
                if (enemies.Count <= condition.ConditionValue) isMet = true;
            }
            else if(condition.ConditionType == PhaseConditionType.HpThreshold)
            {
                float totalMax = 0;
                float totalCur = 0;
                foreach (RuntimeEnemy e in _battleSystem.Enemies)
                {
                    totalMax += e.MaxHP;
                    totalCur += e.CurrentHP;
                }
                if (totalMax > 0 && (totalCur / totalMax) <= condition.ConditionValue) 
                    isMet = true;
            }
            if (isMet)
            {
                int targetPhaseIndex = i + 1;
                foreach (RuntimeEnemy e in enemies)
                    e.ForceChangePhase(targetPhaseIndex);
            }
        }
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
        _globalTurnIndex = 0;
        StartNewBattle();
        _battleSystem.SetPlayerStartPosition(sectorNum);
    }


}

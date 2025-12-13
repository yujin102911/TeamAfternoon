using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 게임 전투 흐름 총괄 관리
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static int SelectedStageID = 0;

    #region Serialize Fields
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
    [SerializeField] private EnemyVisualController _enemyVisualController;
    [SerializeField] private BattleSequenceController _battleSequenceController;

    [Header("게임 설정")]
    [SerializeField] private int _startHandSize = 5;
    [SerializeField] private int _playerMaxHP = 20;
    [SerializeField] private MapSize _mapSize = MapSize.Sectors_8;

    [Header("테스트용 스테이지 데이터")]
    [SerializeField] private StageData currentStageData;
    #endregion

    #region Private Fields
    // System
    private DeckSystem _deckSystem;
    private BattleSystem _battleSystem;
    private MapSystem _mapSystem;

    private TimelineManager _timelineManager;

    private int _currentRound = 0;
    private int _globalTurnIndex = 0;
    private int _completePhaseIndex = -1;

    // 게임 상태 변수
    private bool _isSectorSelected = false;
    private bool _isExecutingRound = false;
    private bool _isBattleEnded = false;

    // UI 용 변수
    private int _currentPhase = 1; // 기본 1
    private int _phaseTurnCount = 0;
    #endregion

    #region Properties
    public DeckSystem DeckSystem => _deckSystem;
    public BattleSystem BattleSystem => _battleSystem;
    public MapSystem MapSystem => _mapSystem;
    public int CurrentRound => _currentRound;
    public StageData CurrentStageData => currentStageData;
    public bool IsExecutingRound
    {
        get => _isExecutingRound;
        private set
        {
            if (_isExecutingRound != value)
            {
                _isExecutingRound = value;
                OnGameStateChanged?.Invoke(); // 값이 바뀌면 알림
            }
        }
    }
    public bool IsSectorSelected => _isSectorSelected;
    public bool IsBattleEnded => _isBattleEnded;
    #endregion

    #region Events

    public event Action OnGameStateChanged;
    public event Action<int, int> OnRoundChanged;
    public event Action<bool> OnBattleEnded; // True: 승리 False: 패배

    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        // 싱글톤 설정
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
        SubscribeEvents();
        SetupGame();
    }

    private void OnDestroy()
    {
        UnSubscribeEvents();
    }

    #endregion

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

        // 선택된 스테이지가 있다면 (SelectedStageID 변수가 1 이상이면) DataRepository에서 갖다 덮어 씌워버리깅
        if (SelectedStageID > 0)
        {
            StageData selectedStage = dataRepository.GetStage(SelectedStageID);
            if (selectedStage != null)
            {
                currentStageData = selectedStage;
                Debug.Log($"[GameManager] 스테이지 {SelectedStageID} 데이터를 로드했습니다");
            }
            else
            {
                Debug.LogError($"[GameManager] 스테이지 ID에 해당하는 데이터가 없습니다");
            }
        }
        else
        {
            Debug.LogWarning($"[GameManager] 선택된 스테이지 ID가 없습니다");
        }

        Debug.Log("[GameManager] 내부 시스템 생성 완료 (Awake)");
    }

    /// <summary>
    /// 외부 연결 초기화
    /// </summary>
    private void LateInitialize()
    {
        // 타임라인 매니저
        _timelineManager = TimelineManager.Instance;
        if (_timelineManager != null) _timelineManager.Initialize(_battleSystem);
        else Debug.LogError("[GameManager] TimelineManager를 찾을 수 없습니다");

        // MapVisualController 연결
        if (_mapVisualController != null) _mapVisualController.Initialize(_mapSystem, _battleSystem, mapConfig);
        else Debug.LogError("[GameManager] MapVisualController를 찾을 수 없습니다");

        // EnemyVisualController 연결
        if (_enemyVisualController != null)
        {
            _enemyVisualController.Initialize(_battleSystem);
        }

        // PlayerVisualController 연결
        if (_playerVisualController != null) _playerVisualController.Initialize(_mapSystem);
        else Debug.LogError("[GameManager] PlayerVisualController를 찾을 수 없습니다");

        Debug.Log("[GameManager] 외부 시스템 연결 완료 (Start)");

    }

    /// <summary>
    /// 이벤트 구독 관리 함수
    /// </summary>
    private void SubscribeEvents()
    {
        _mapSystem.OnSectorSelected += OnStartingSectorSelected;
        if (_timelineUI != null && _mapVisualController != null)
        {
            _timelineUI.OnRequestHighlight += _mapVisualController.OnRequestHighlight;
            _timelineUI.OnRequestClearHighlight += () => _mapVisualController.OnRequestClearHighlight();
            _battleSystem.OnEnemyAttack += _mapVisualController.OnEnemyAttackVisual;
            _battleSystem.OnEnemyDied += (e) => _mapVisualController.RefreshMapOwnershipVisuals();
        }

        if (_timelineUI != null && _playerVisualController != null)
        {
            _battleSystem.OnPlayerMoved += _playerVisualController.OnPlayerMoved;
            _battleSystem.OnPlayerHit += _playerVisualController.PlayHitEffect;
            _battleSystem.OnPlayerAttack += _playerVisualController.PlayAttackShake;
            _timelineUI.OnRequestPreviewPlayer += _playerVisualController.ShowPlayerPreview;
            _timelineUI.OnRequestHidePreview += _playerVisualController.HidePlayerPreview;
        }

    }

    private void UnSubscribeEvents()
    {
        _mapSystem.OnSectorSelected -= OnStartingSectorSelected;
        if (_timelineUI != null && _mapVisualController != null)
        {
            _timelineUI.OnRequestHighlight -= _mapVisualController.OnRequestHighlight;
            _timelineUI.OnRequestClearHighlight -= () => _mapVisualController.OnRequestClearHighlight();
            _battleSystem.OnEnemyAttack -= _mapVisualController.OnEnemyAttackVisual;
            _battleSystem.OnEnemyDied -= (e) => _mapVisualController.RefreshMapOwnershipVisuals();
        }

        if (_timelineUI != null && _playerVisualController != null)
        {
            _battleSystem.OnPlayerMoved -= _playerVisualController.OnPlayerMoved;
            _battleSystem.OnPlayerHit -= _playerVisualController.PlayHitEffect;
            _battleSystem.OnPlayerAttack -= _playerVisualController.PlayAttackShake;
            _timelineUI.OnRequestPreviewPlayer -= _playerVisualController.ShowPlayerPreview;
            _timelineUI.OnRequestHidePreview -= _playerVisualController.HidePlayerPreview;
        }
    }

    #endregion

    #region Game Flow Methods
    /// <summary>
    /// 게임 초기화 함수
    /// EnableSelectionMode -> OnStartingSectorSelected
    /// </summary>
    public void SetupGame()
    {
        _currentRound = 0; 
        _globalTurnIndex = 0;

        IsExecutingRound = false;
        _isBattleEnded = false;

        _currentPhase = 1;
        _phaseTurnCount = 0;

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

        // 맵 생성
        if (currentStageData != null)
        {
            _mapSystem.GenerateMap(currentStageData.MapSize, currentStageData.SectorPoints);
        }
        else
        {
            _mapSystem.GenerateMap(_mapSize);
        }
        // UserData 기반 덱 생성
        _deckSystem.InitializeDeck();

        // 적 배치
        if (currentStageData != null)
            SetupEnemiesFromStage(currentStageData);
        else
            Debug.LogError("StageData가 없습니다");

        if (_mapVisualController != null)
        {
            _mapVisualController.RefreshMapOwnershipVisuals();
        }
        // 혹시 인트로가 없는 씬인 경우에는 그냥 바로 섹터 선택 모드 진입
        if (FindAnyObjectByType<SceneIntroController>() == null)
            OnIntroCompleted();
    }

    public void OnIntroCompleted()
    {
        Debug.Log("[GameManager] 인트로 종료. 턴 시작 연출 재생");
        _battleSequenceController.PlayerTurnStartSequence(() =>
        {
            Debug.Log("[GameManager] 연출 종료. 맵 선택 활성화");
            _mapSystem.EnableSelectionMode();
        });
    }

    /// <summary>
    /// 섹터 선택 후 호출되는 함수
    /// OnSelectineSectorSelected -> StartNewBattle
    /// </summary>
    private void OnStartingSectorSelected(int sectorNum)
    {
        if (_isExecutingRound) return;
        _mapSystem.DisableSelectionMode();
        _battleSequenceController.TurnOffSectorSelectText();
        _isSectorSelected = true;
        OnGameStateChanged?.Invoke();

        if (_playerVisualController != null) _playerVisualController.SpawnPlayer(sectorNum);
        _battleSystem.SetPlayerStartPosition(sectorNum);
       
        StartNewBattle();
    }

    /// <summary>
    /// 전투 시작 함수
    /// StartNewBattle -> (버튼) -> ExecuteRound
    /// </summary>
    public void StartNewBattle()
    {
        _isBattleEnded = false;
        // 덱 드로우
        _deckSystem.DrawCards(_startHandSize);
        if (_timelineManager != null)
        {
            _timelineManager.ReceiveHand(_deckSystem.Hand);
        }
        
        Debug.Log("[GameManager] 전투 시작");
        if (_mapVisualController != null)
        {
            _mapVisualController.RefreshMapOwnershipVisuals();
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
        IsExecutingRound = true;
        _currentRound++;
        Debug.Log($"[GameManager] ==== 라운드 {_currentRound} 시작 ====");

        if (_timelineManager != null)
        {
            yield return StartCoroutine(_timelineManager.ExecuteTimeline());
        }

        EndRound();

        IsExecutingRound = false;
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
        NotifyRoundChanged();

        _deckSystem.DiscardHand();
        _deckSystem.DrawCards(_startHandSize);

        _battleSequenceController.PlayerTurnStartSequence(() =>
        {
            if (_timelineManager != null)
            {
                _timelineManager.ReceiveHand(_deckSystem.Hand);
            }
        });
        
    }

    public void EndBattle(bool victory)
    {
        if (_isBattleEnded) return;
        _isBattleEnded = true;
        IsExecutingRound = false;

        Debug.Log($"[GameManager] 전투 종료 - {(victory ? "승리" : "패배")}");

        // 현재 돌아가고 있는 모든 코루틴 종료
        StopAllCoroutines();

         SaveService.Save(userGameData);

        OnBattleEnded?.Invoke(victory);
    }
    #endregion

    #region Enemy Pattern Methods
    /// <summary>
    /// 적 세팅하는 함수
    /// </summary>
    private void SetupEnemiesFromStage(StageData stage)
    {
        List<RuntimeEnemy> enemies = new List<RuntimeEnemy>();
        foreach (StageEnemySetup spawn in stage.EnemySpawns)
        {
            if (spawn.enemyData == null) continue;
            RuntimeEnemy newEnemy = new RuntimeEnemy(
                spawn.enemyData,
                new List<int>(spawn.hitSectors)
                );
            enemies.Add(newEnemy);
        }
        _battleSystem.InitializeBattle(enemies, _playerMaxHP, _mapSystem.TotalSectors);
        UpdateEnemyPatterns();
        NotifyRoundChanged();
    }
    /// <summary>
    /// 있는 적 중 패턴 번갈아가며 뽑아오는 함수
    /// </summary>
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
        int enemyCurrentPhase = activeEnemy.CurrentPhaseIndex + 1;
        if (enemyCurrentPhase > _currentPhase)
        {
            _currentPhase = enemyCurrentPhase;
            _phaseTurnCount = 0;
            Debug.Log($"[GameManager] 패턴 고갈로 인한 {_currentPhase} 페이즈 강제 진입");
        }
        if (nextPattern != null)
        {
            activeEnemy.SetPattern(nextPattern);
            Debug.Log($"[GameManager] 이번 턴 행동: {activeEnemy.Data.Enemy_Name} / {nextPattern.Pattern_Name}");
            _timelineUI.OnPatternChanged(nextPattern);
        }
        _globalTurnIndex++;
        _phaseTurnCount++;

        if (_timelineManager !=  null) 
            _timelineManager.RefreshCombinedEnemyPattern();

        Debug.Log("[GameManager] 적 패턴 갱신 로직 완료");

    }
    /// <summary>
    /// 페이즈 검사
    /// </summary>
    private void CheckAndApplyPhaseTransition(List<RuntimeEnemy> enemies)
    {
        if (currentStageData.PhaseConditions == null) return;

        int currentPhaseIndex = _currentPhase - 1;
        int nextPhaseIndex = currentPhaseIndex;

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
                int targetIndex = i + 1;
                if (targetIndex > currentPhaseIndex)
                {
                    if (targetIndex == currentPhaseIndex + 1)
                    {
                        nextPhaseIndex = targetIndex;
                    }
                }
            }
        }
        if (nextPhaseIndex > currentPhaseIndex)
        {
            _currentPhase = nextPhaseIndex + 1;
            _phaseTurnCount = 0;
            Debug.Log($"[GameManager] {_currentPhase} 페이즈 진입");

            foreach (RuntimeEnemy e in enemies)
                e.ForceChangePhase(nextPhaseIndex);
        }
    }

    private void NotifyRoundChanged()
    {
        OnRoundChanged?.Invoke(_currentPhase, _phaseTurnCount);
    }
    #endregion

}

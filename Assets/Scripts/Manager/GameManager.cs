using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.UI;

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
    private int _currentEnemyIndex = 0;

    // 게임 상태 변수
    private bool _isSectorSelected = false;
    private bool _isExecutingRound = false;
    private bool _isBattleEnded = false;

    // UI 용 변수
    private int _currentPhase = 1; // 기본 1
    private int _phaseTurnCount = 0;

    // 통계용 변수
    private int _statPlayerAttackCount = 0;
    private int _statPlayerHitCount = 0;
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
    public bool IsRoundInterrupted { get; private set; }
    #endregion

    #region Events

    public event Action OnGameStateChanged;
    public event Action<int, int, int> OnRoundChanged;
    public event Action<bool, int, int, int, int> OnBattleEnded; // True: 승리 False: 패배

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
        }

        if (_timelineUI != null && _playerVisualController != null)
        {
            _battleSystem.OnPlayerMoved += _playerVisualController.OnPlayerMoved;
            _battleSystem.OnPlayerHit += _playerVisualController.PlayHitEffect;
            _battleSystem.OnPlayerAttack += _playerVisualController.PlayAttackShake;
            _timelineUI.OnRequestPreviewPlayer += _playerVisualController.ShowPlayerPreview;
            _timelineUI.OnRequestHidePreview += _playerVisualController.HidePlayerPreview;
        }

        if (_battleSystem != null)
        {
            _battleSystem.OnPlayerAttack += CountPlayerAttack;
            _battleSystem.OnPlayerHit += CountPlayerHit;
            _battleSystem.OnEnemyPurified += HandleEnemyPurified; 
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
        }

        if (_timelineUI != null && _playerVisualController != null)
        {
            _battleSystem.OnPlayerMoved -= _playerVisualController.OnPlayerMoved;
            _battleSystem.OnPlayerHit -= _playerVisualController.PlayHitEffect;
            _battleSystem.OnPlayerAttack -= _playerVisualController.PlayAttackShake;
            _timelineUI.OnRequestPreviewPlayer -= _playerVisualController.ShowPlayerPreview;
            _timelineUI.OnRequestHidePreview -= _playerVisualController.HidePlayerPreview;
        }
        
        if (_battleSystem != null)
        {
            _battleSystem.OnPlayerAttack -= CountPlayerAttack;
            _battleSystem.OnPlayerHit -= CountPlayerHit;
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
        _currentEnemyIndex = 0;

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
        // 혹시 모를 예외 상황을 위함
        else
        {
            _mapSystem.GenerateMap(_mapSize);
        }
        // UserData 기반 덱 생성
        _deckSystem.InitializeDeck();

        if (_mapVisualController != null)
        {
            _mapVisualController.RefreshMapOwnershipVisuals();
        }
        // 혹시 인트로가 없는 씬인 경우에는 그냥 바로 섹터 선택 모드 진입
        if (FindAnyObjectByType<SceneIntroController>() == null)
            OnIntroCompleted();
        LoadEnemyAtIndex(0, false);
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

    private void HandleEnemyPurified()
    {
        Debug.Log("[GameManager] 적 정화 감지! 라운드 중단을 요청합니다.");
        IsRoundInterrupted = true;
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

        if (_playerVisualController != null) _playerVisualController.SpawnPlayer(sectorNum);
        _battleSystem.SetPlayerStartPosition(sectorNum);
       
        StartNewBattle();
        if (_timelineUI != null)
            _timelineUI.UpdateDangerIndicators();
        OnGameStateChanged?.Invoke();

    }

    /// <summary>
    /// 전투 시작 함수
    /// StartNewBattle -> (버튼) -> ExecuteRound
    /// </summary>
    public void StartNewBattle()
    {
        _isBattleEnded = false;
        // 통계 초기화
        _statPlayerAttackCount = 0;
        _statPlayerHitCount = 0;

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

    // 버튼과 연결
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
        IsRoundInterrupted = false;
        _currentRound++;
        Debug.Log($"[GameManager] ==== 라운드 {_currentRound} 시작 ====");

        _battleSequenceController.PlayCameraEffect(true);
        // 전투로 넘어가는 연출 코루틴으로 넣기
        BattleUIManager.Instance.Hide_startBtn();
        yield return StartCoroutine(_battleSequenceController.Move_HandPanel(false));
        yield return StartCoroutine(_battleSequenceController.Move_enemyCardUIs(true));

        if (_timelineManager != null)
        {
            yield return StartCoroutine(_timelineManager.ExecuteTimeline());
        }

        yield return StartCoroutine(_battleSequenceController.Move_enemyCardUIs(false));
        if (IsRoundInterrupted)
        {
            HandleRoundInterrupted(); // 적 교체 및 리셋
        }
        else
        {
            EndRound(); // 정상적인 턴 종료 (패턴 넘기기 포함)
        }
        IsExecutingRound = false;
        Debug.Log($"[GameManager] ==== 라운드 {_currentRound} 종료 ====");
    }
    private void HandleRoundInterrupted()
    {
        Debug.Log("[GameManager] 라운드 중단됨. 다음 적 로드 시퀀스 진입.");

        if (_timelineManager != null) _timelineManager.OnRoundEnded();
        _currentEnemyIndex++;
        LoadEnemyAtIndex(_currentEnemyIndex, true);
        if (!_isBattleEnded)
            PrepareNextHand();
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
            _battleSystem.ChooseCureSector();
            _battleSystem.OnRoundEnded();
        }
        UpdateEnemyPatterns();
        PrepareNextHand();

        
    }
    private void PrepareNextHand()
    {
        _deckSystem.DiscardHand();
        _deckSystem.DrawCards(_startHandSize);

        if (_timelineManager != null && TimelineManager.Instance.Is_Cure)
            _mapVisualController.RefreshMapOwnershipVisuals();
        _battleSequenceController.PlayerTurnStartSequence(() =>
        {
            if (_timelineManager != null)
                _timelineManager.ReceiveHand(_deckSystem.Hand);
        });
    }

    public void EndBattle(bool victory)
    {
        if (_isBattleEnded) return;
        _isBattleEnded = true;
        IsExecutingRound = false;
        int _leftPlayerHP = _battleSystem.PlayerHP;
        Debug.Log($"[GameManager] 전투 종료 - {(victory ? "승리" : "패배")}");

        if (victory)
        {
            if (currentStageData != null)
            {
                currentStageData.IsCleared = true;
                Debug.Log($"[GameManager] 스테이지 '{currentStageData.StageName}'(ID: {currentStageData.StageNumber}) 클리어 처리 완료!");

                // (선택 사항) 에디터 상에서 변경 사항을 즉시 파일에 저장하고 싶다면 아래 코드 사용
                // 빌드 후에는 UserGameData 같은 별도의 저장 시스템을 사용해야 영구 저장됩니다.
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(currentStageData);
#endif
            }
        }
            // 현재 돌아가고 있는 모든 코루틴 종료
            StopAllCoroutines();

         SaveService.Save(userGameData);

        OnBattleEnded?.Invoke(victory, _currentRound, _statPlayerHitCount, _statPlayerAttackCount, _leftPlayerHP);
    }
    #endregion

    #region Enemy Pattern Methods
    private void LoadEnemyAtIndex(int index, bool keepPlayerHP)
    {
        if (currentStageData == null || index >= currentStageData.EnemySpawns.Count)
        {
            // 더 이상 적이 없으면 겜 끗
            EndBattle(true);
            return;
        }
        StageEnemySetup spawn = currentStageData.EnemySpawns[index];

        List<RuntimeEnemy> enemies = new List<RuntimeEnemy>
        {
            new RuntimeEnemy(spawn.enemyData, new List<int>(spawn.hitSectors))
        };
        _battleSystem.InitializeBattle(enemies, _playerMaxHP, _mapSystem.TotalSectors, keepPlayerHP);
        UpdateEnemyPatterns();
        Debug.Log($"[GameManager] {_currentEnemyIndex + 1}번째 적 등장: {spawn.enemyData.Enemy_Name}");
    }


    /// <summary>
    /// 있는 적 중 패턴 번갈아가며 뽑아오는 함수
    /// </summary>
    private void UpdateEnemyPatterns()
    {
        if (_battleSystem.Enemies.Count ==0) return;
        RuntimeEnemy activeEnemy = _battleSystem.Enemies[0];

        EnemyPattern nextPattern = activeEnemy.GetNextPattern();

        if (nextPattern != null)
        {
            activeEnemy.SetPattern(nextPattern);
            _timelineUI?.OnPatternChanged(nextPattern);
            if (TimelineManager.Instance != null)
            {
                TimelineManager.Instance.SetEnemyPattern(nextPattern);
            }
        }

    }
    #endregion

    #region Counting Helper Methods
    private void CountPlayerAttack()
    {
        if (_isBattleEnded) return;
        _statPlayerAttackCount++;
    }

    private void CountPlayerHit()
    {
        if (_isBattleEnded) return;
        _statPlayerHitCount++;
    }
    #endregion

}

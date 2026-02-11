using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.UI;
using UnityEngine.UI;

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
    [SerializeField] private Transform _backgroundTransform;

    [Header("데이터 참조")]
    [SerializeField] private DataRepository dataRepository;
    [SerializeField] private UserGameData userGameData; // TODO: 타이틀씬 생기면 ServiceLocator로부터 받아오도록!

    [Header("비주얼 컨트롤러")]
    [SerializeField] private TimelineUI _timelineUI;
    [SerializeField] private MapVisualController _mapVisualController;
    [SerializeField] private PlayerVisualController _playerVisualController;
    [SerializeField] private EnemyVisualController _enemyVisualController;
    [SerializeField] private BattleSequenceController _battleSequenceController;
    [SerializeField] private EnemyStoneVisualController _enemyStoneVisualController;

    [Header("게임 설정")]
    [SerializeField] private int _startHandSize = 5;
    [SerializeField] private int _playerMaxHP = 3; // 기본값 (-> 정상적으로 실행 시 UserData에서 받아옴)
    [SerializeField] private MapSize _mapSize = MapSize.Grid_3x3;
    [SerializeField] private bool isTutorial;

    [Header("테스트용 스테이지 데이터")]
    [SerializeField] private StageData currentStageData;

    [Header("UI 알림")]
    [SerializeField] private SpecialEffectNotification _effectNotification;
    #endregion

    #region Private Fields
    // System
    private DeckSystem _deckSystem;
    private BattleSystem _battleSystem;
    private MapSystem _mapSystem;

    private TimelineManager _timelineManager;

    private int _currentRound = 0;
    private int _currentEnemyIndex = 0;
    private int _memory = 0;
    private int _limitRound = 0;

    // 게임 상태 변수
    private bool _isSectorSelected = false;
    private bool _isExecutingRound = false;
    private bool _isBattleEnded = false;
    [SerializeField] private bool isDebugging = false;
    [SerializeField] private GameObject _debugmodeChecking;
    private bool _isGamestarted = false;

    // UI 용 변수
    private int _currentPhase = 1; // 기본 1
    private int _phaseTurnCount = 0;

    // 통계용 변수
    private int _statPlayerAttackCount = 0;
    private int _statPlayerHitCount = 0;

    // 배경 전환 저장용
    private Sprite _bgSprite;

    // 로그 수집용 변수
    private float _stageStartTime;
    #endregion

    #region Properties
    public DeckSystem DeckSystem => _deckSystem;
    public BattleSystem BattleSystem => _battleSystem;
    public MapSystem MapSystem => _mapSystem;
    public int CurrentRound => _currentRound;
    public int StartHandSize => _startHandSize;
    public StageData CurrentStageData => currentStageData;
    // 덱빌딩에서 사용
    public UserGameData UserGameData => userGameData;
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
    public bool IsTutorial => isTutorial;
    public bool IsSequencePlaying { get; set; } = false;
    public bool IsRoundInterrupted { get; private set; }
    public bool IsDebugging => isDebugging;

    public bool IsGameStarted => _isGamestarted;
    #endregion

    #region Events

    public event Action OnGameStateChanged;
    public event Action<int, int, int, int> OnRoundChanged;      // 현재 라인, 총 라인, 현재 적, 총 적
    public event Action<EndCondition> OnBattleEnded; // Victory: 승리, Dead: 사망, RoundOver: 라운드 초과
    public event Action<int, int> OnMemoryUpdate;

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
        if (ServiceLocator.Instance != null)
        {
            dataRepository = ServiceLocator.Instance.CurrentRepository;
        }
        Initialize();

    }

    private void Start()
    {
        SetupGame();
        LateInitialize();
        SubscribeEvents();
        LoadEnemyAtIndex(0, false);

        // 베틀씬 사운드
        Set_BGM();

        

        if (ShortcutManager.Instance != null)
        {
            ShortcutManager.Instance.Register(new ActionCommand(
                "game.excute",
                ExecuteRound,
                () => _isGamestarted && !IsExecutingRound && !IsBattleEnded
            ));
        }
    }

    private void Set_BGM()
    {
        if (SoundManager.Instance == null) return;

        bool is_hard = UserGameData.Difficulty == Difficulty.Hard;

        if (is_hard)
        {
            if (currentStageData.StageNumber < 3)
            {
                SoundManager.Instance.Play(SoundID.BGM_Hard);
            }
            else
            {
                SoundManager.Instance.Play(SoundID.BGM_Hard_Final);
            }
        }
        else
        {
            if (currentStageData.StageNumber < 5)
            {
                SoundManager.Instance.Play(SoundID.BGM_Battle);
            }
            else
            {
                SoundManager.Instance.Play(SoundID.BGM_Boss);
            }
        }
    }


    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.F10))
        //{
        //    Debug.Log("[GameManager] F10 키 입력 감지 - 현재 스테이지 클리어 처리 실행");
        //    ServiceLocator.Instance.CurrentUser.SetStageCleared(currentStageData.StageNumber);
        //    OnBattleEnded?.Invoke(EndCondition.Victory);
        //}
        //if (Input.GetKeyDown(KeyCode.F9))
        //{
        //    ToggleDebugging();
        //    if (isDebugging == true)
        //    {
        //        Debug.Log("[GameManager] F9 키 입력 감지 - 디버그 모드 진입(무적, 라운드 무제한)");
        //    }
        //    else
        //    {
        //        Debug.Log("[GameManager] F9 키 입력 감지 - 디버그 모드 해제");
        //    }

        //}
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("[GameManager] Ctrl + Z 입력 감지 - 도전과제 호출");
            CtrlZAchievement();
        }

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
        // 서비스 로케이터가 있다면 (타이틀 씬부터 정상 실행됐다면, 그 데이터 받아옴)
        // 없다면 인스펙터에 연결된 userData로 세팅
        if (ServiceLocator.Instance != null && ServiceLocator.Instance.CurrentUser != null)
        {
            userGameData = ServiceLocator.Instance.CurrentUser;
            _playerMaxHP = userGameData.MaxHP();
            ServiceLocator.Instance.SaveNowUserData(); // 전투 시작 전에도 한번 저장
        }
        else if (userGameData != null) // 여기서 Tutorial의 유저데이터로 설정 가능
        {
            _playerMaxHP = userGameData.MaxHP();
        }
        else
        {
            Debug.LogWarning("[GameManager] UserGameData를 찾을 수 없어 기본 체력(3)으로 설정합니다.");
            _playerMaxHP = 3;
        }

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

                // 맵 생성
                GameObject mapObj = currentStageData.EnemySpawns[0].enemyData.EnemyBackPrefab;
                if (mapObj != null && _backgroundTransform != null)
                {
                    Instantiate(mapObj, _backgroundTransform);
                    Debug.Log($"[GameManager] 스테이지 배경 오브젝트를 생성했습니다");
                }
            }
            else
            {
                Debug.LogError($"[GameManager] 스테이지 ID에 해당하는 데이터가 없습니다");
            }
        }
        else
        {
            Debug.LogWarning($"[GameManager] 선택된 스테이지 ID가 없습니다");

            // 맵 생성
            GameObject mapObj = currentStageData.EnemySpawns[0].enemyData.EnemyBackPrefab;
            if (mapObj != null && _backgroundTransform != null)
            {
                Instantiate(mapObj, _backgroundTransform);
                Debug.Log($"[GameManager] 인스펙터 데이터로 스테이지 배경 오브젝트를 생성했습니다");
            }
        }
        _limitRound = currentStageData.LimitRound;
        
        Debug.Log("[GameManager] 내부 시스템 생성 완료 (Awake)");
    }

    /// <summary>
    /// 외부 연결 초기화
    /// </summary>
    private void LateInitialize()
    {
        // 타임라인 매니저
        _timelineManager = TimelineManager.Instance;
        if (_timelineManager != null) _timelineManager.Initialize(_battleSystem, _mapSystem.TotalSectors, _mapSystem.Columns);
        else Debug.LogError("[GameManager] TimelineManager를 찾을 수 없습니다");

        // MapVisualController 연결
        if (_mapVisualController != null) _mapVisualController.Initialize(_mapSystem, _battleSystem, mapConfig);
        else Debug.LogError("[GameManager] MapVisualController를 찾을 수 없습니다");

        // EnemyVisualController 연결
        if (_enemyVisualController != null)
        {
            _enemyVisualController.Initialize(_battleSystem);
        }

        if (_enemyStoneVisualController != null)
        {
            _enemyStoneVisualController.Initialize(_mapSystem, _battleSystem);
        }

        // PlayerVisualController 연결
        if (_playerVisualController != null) _playerVisualController.Initialize(_battleSystem, _mapSystem);
        else Debug.LogError("[GameManager] PlayerVisualController를 찾을 수 없습니다");

        Debug.Log("[GameManager] 외부 시스템 연결 및 목표 설정 완료 (Start)");

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
            _battleSystem.OnEnemyAttackSuccess += _mapVisualController.OnEnemyAttackVisual;
            _battleSystem.OnEnemyAttackSuccess += _enemyVisualController.PlayEnemyAttack;
            _battleSystem.OnHeal += _mapVisualController.PlayHealEffect;
        }

        if (_timelineUI != null && _playerVisualController != null)
        {
            _battleSystem.OnPlayerMoved += _playerVisualController.OnPlayerMoved;
            _battleSystem.OnPlayerHit += _playerVisualController.PlayHitEffect;
            _battleSystem.OnPlayerMeleeAttack += _playerVisualController.PlaySwordAttack;
            _battleSystem.OnPlayerLongRangeAttack += _playerVisualController.PlayBowAttack;
            _battleSystem.OnPlayerAttackSuccess += _playerVisualController.PlayAttackEffect;
            _battleSystem.OnPlayerLongRangeStart += _playerVisualController.PlayBowCharging;
            _battleSystem.OnPlayerLongRangeMiddle += _playerVisualController.PlayBowMiddle;
            _battleSystem.OnPlayerGuard += _playerVisualController.PlayGuard;
            _battleSystem.OnPlayerIdle += _playerVisualController.PlayIdle;

            //트리거 변경
            _battleSystem.OnChangePlayerAnim += _playerVisualController.ChangeAnim;
            _battleSystem.OnChangeEnemyAnim += _enemyVisualController.ChangeAnim;

            //근거리 차징
            _battleSystem.OnStartMelee += _playerVisualController.PlayMeleeStart;
            _battleSystem.OnMiddleMelee += _playerVisualController.PlayMeleeMiddle;
            _battleSystem.OnEndMelee += _playerVisualController.PlayMeleeEnd;

            _battleSystem.OnEnemyHit += _enemyVisualController.PlayDamage;
            _battleSystem.OnEnemyDash += _enemyVisualController.PlayEnemyDash;
            _timelineUI.OnRequestPreviewPlayer += (sector, action, dir, isEnemyLeft) => {
                _playerVisualController.ShowPlayerPreview(sector, action, dir, isEnemyLeft);
                _enemyVisualController.PreviewFlip(isEnemyLeft);
            };
            _timelineUI.OnRequestHidePreview += () => {
                _playerVisualController.HidePlayerPreview();
                _enemyVisualController.RestoreActualSide();
            };

            _enemyVisualController.OnEnemySideChanged += _playerVisualController.PlayerFlip;
        }

        if (_battleSystem != null)
        {
            _battleSystem.OnPlayerAttackSuccess += CountPlayerAttack;
            _battleSystem.OnPlayerHit += CountPlayerHit;
            _battleSystem.OnEnemyDied += HandleEnemyPurified;
            _battleSystem.OnPlayerDied += _playerVisualController.PlayDeath;

            _battleSystem.OnEnemyWind += _mapVisualController.Play_WindEffect;
        }

    }

    private void UnSubscribeEvents()
    {
        _mapSystem.OnSectorSelected -= OnStartingSectorSelected;
        if (_timelineUI != null && _mapVisualController != null)
        {
            _timelineUI.OnRequestHighlight -= _mapVisualController.OnRequestHighlight;
            _timelineUI.OnRequestClearHighlight -= () => _mapVisualController.OnRequestClearHighlight();
            _battleSystem.OnEnemyAttackSuccess -= _mapVisualController.OnEnemyAttackVisual;
            _battleSystem.OnEnemyAttackSuccess -= _enemyVisualController.PlayEnemyAttack;
            _battleSystem.OnHeal -= _mapVisualController.PlayHealEffect;
        }

        if (_timelineUI != null && _playerVisualController != null)
        {
            _battleSystem.OnPlayerMoved -= _playerVisualController.OnPlayerMoved;
            _battleSystem.OnPlayerHit -= _playerVisualController.PlayHitEffect;
            _battleSystem.OnPlayerMeleeAttack -= _playerVisualController.PlaySwordAttack;
            _battleSystem.OnPlayerLongRangeAttack -= _playerVisualController.PlayBowAttack;
            _battleSystem.OnPlayerLongRangeStart -= _playerVisualController.PlayBowCharging;
            _battleSystem.OnPlayerLongRangeMiddle -= _playerVisualController.PlayBowMiddle;
            _battleSystem.OnPlayerGuard -= _playerVisualController.PlayGuard;
            _battleSystem.OnPlayerIdle -= _playerVisualController.PlayIdle;

            //트리거 변경
            _battleSystem.OnChangePlayerAnim -= _playerVisualController.ChangeAnim;
            _battleSystem.OnChangeEnemyAnim -= _enemyVisualController.ChangeAnim;

            //근거리 차징
            _battleSystem.OnStartMelee -= _playerVisualController.PlayMeleeStart;
            _battleSystem.OnMiddleMelee -= _playerVisualController.PlayMeleeMiddle;
            _battleSystem.OnEndMelee -= _playerVisualController.PlayMeleeEnd;

            _battleSystem.OnEnemyHit -= _enemyVisualController.PlayDamage;
            _battleSystem.OnPlayerAttackSuccess -= _playerVisualController.PlayAttackEffect;
            _battleSystem.OnEnemyDash -= _enemyVisualController.PlayEnemyDash;
            _timelineUI.OnRequestPreviewPlayer -= (sector, action, dir, isEnemyLeft) => {
                _playerVisualController.ShowPlayerPreview(sector, action, dir, isEnemyLeft);
                _enemyVisualController.PreviewFlip(isEnemyLeft);
            };
            _timelineUI.OnRequestHidePreview -= () => {
                _playerVisualController.HidePlayerPreview();
                _enemyVisualController.RestoreActualSide();
            };

            _enemyVisualController.OnEnemySideChanged -= _playerVisualController.PlayerFlip;
        }
        
        if (_battleSystem != null)
        {
            _battleSystem.OnPlayerAttackSuccess -= CountPlayerAttack;
            _battleSystem.OnPlayerHit -= CountPlayerHit;
            _battleSystem.OnPlayerDied -= _playerVisualController.PlayDeath;

            _battleSystem.OnEnemyWind -= _mapVisualController.Play_WindEffect;
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
        Time.timeScale = 1.0f;

        _currentRound = 0;
        _currentEnemyIndex = 0;

        IsExecutingRound = false;
        _isBattleEnded = false;

        _currentPhase = 1;
        _phaseTurnCount = 0;
        CheckLimitEffectChange();
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
        //if (FindAnyObjectByType<SceneIntroController>() == null)
        //OnIntroCompleted();


        //TODO:추후에 8 자리에 최대 턴수 기입
        OnMemoryUpdate?.Invoke(_currentRound, _limitRound);

        _stageStartTime = Time.time;
        Debug.Log("[GameManager] 스테이지 타이머 시작");
    }

    public void GameStart(List<RuntimeBlock> hand)
    {
        // 덱 교체
        _deckSystem.InitializeDeck(hand);
        
        //시작 연출
        _battleSequenceController.GameStartSequence();

        //5섹터 고정 및 게임 시작
        _mapSystem.Handle_SetSector(5);
        
        _isGamestarted = true;
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

        _playerVisualController.Stop_PlayerIdle();
        _enemyVisualController.Stop_EnemyIdle();
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
        if (_isBattleEnded) return;
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
        if (!isDebugging)
        {
            _currentRound++;
        }
        int final_memory = 0;

        foreach (var effect in TimelineManager.Instance.additional_Effects)
        {
            if (effect == null) continue;
            final_memory += effect.cost;
        }

        if (!isDebugging)
        {
            _memory += 8;
        }

        OnMemoryUpdate?.Invoke(_memory, _limitRound);
        Debug.Log($"[GameManager] ==== 라운드 {_currentRound} 시작 ====");

        //idle 실행
        _playerVisualController.Play_PlayerIdle();
        _enemyVisualController.Play_EnemyIdle();

        //_battleSequenceController.PlayCameraEffect(true);
        // 전투로 넘어가는 연출 코루틴으로 넣기
        //BattleUIManager.Instance.Hide_startBtn();
        //yield return StartCoroutine(_battleSequenceController.Move_HandPanel(false));
        //yield return StartCoroutine(_battleSequenceController.Move_enemyCardUIs(true));

        //if (_battleSystem != null)
        //{
        //    UpdateEnemyImagesWrapper(_battleSystem.EnemyHP, _battleSystem.EnemyMaxHP);
        //}

        //yield return StartCoroutine(_battleSequenceController.Slide_Enemy(true));

        // 타임라인 실행
        if (_timelineManager != null)
        {
            _battleSequenceController.PlaySlider();
            yield return StartCoroutine(_timelineManager.ExecuteTimeline());
        }

        //yield return StartCoroutine(_battleSequenceController.Move_enemyCardUIs(false));
        //yield return StartCoroutine(_battleSequenceController.Slide_Enemy(false));
        if (IsRoundInterrupted)
        {
            HandleRoundInterrupted(); // 적 교체 및 리셋
            yield return StartCoroutine(_battleSequenceController.ScrollCoroutine(_bgSprite));
        }
        else
        {
            EndRound(); // 정상적인 턴 종료 (패턴 넘기기 포함)
        }
        IsExecutingRound = false;
        Debug.Log($"[GameManager] ==== 라운드 {_currentRound} 종료 ====");

        //idle 정지
        _playerVisualController.Stop_PlayerIdle();
        _enemyVisualController.Stop_EnemyIdle();

        if(CardTooltip.Instance != null)
        {
            CardTooltip.Instance.Hide();
        }
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
        if (currentStageData != null && _memory >= 8 * _limitRound)
        {
            Debug.Log($"[GameManager] 제한 라운드 ({_limitRound}) 도달. 패배");
            EndBattle(EndCondition.RoundOver); // 라운드 초과 실패 함수 호출
            return;
        }

        if (_timelineManager != null)
        {
            _timelineManager.OnRoundEnded();
        }

        if (_battleSystem != null)
        {
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

    public void EndBattle(EndCondition victory)
    {
        if (_isBattleEnded) return;
        _isBattleEnded = true;
        IsExecutingRound = false;

        //idle 정지
        _playerVisualController.Stop_PlayerIdle();
        _enemyVisualController.Stop_EnemyIdle();

        if (_battleSequenceController != null)
        {
            _battleSequenceController.StopSlider();
        }
        int _leftPlayerHP = _battleSystem.PlayerHP;
        if (victory == EndCondition.Victory)
        {
            Debug.Log("[GameManager] 전투 종료 - 승리");
            if (currentStageData != null)
            {
                bool isLastStage = dataRepository.GetStage(currentStageData.StageNumber + 1) == null;
                if (isLastStage)
                {
                    
                }
                else
                {
                    if (ServiceLocator.Instance.CurrentUser != null)
                    {
                        ServiceLocator.Instance.CurrentUser.SetStageCleared(currentStageData.StageNumber);
                    }
                }
                
            }
        }
        else if (victory == EndCondition.Dead)
        {
            Debug.Log("[GameManager] 전투 종료 - 패배 (플레이어 사망)");
            if (ServiceLocator.Instance.CurrentUser != null)
            {
                ServiceLocator.Instance.CurrentUser.AddGameOverCount();
            }
        }
        else if (victory == EndCondition.RoundOver)
        {
            Debug.Log("[GameManager] 전투 종료 - 패배 (라운드 초과)");
            {
                if (ServiceLocator.Instance.CurrentUser != null)
                {
                    ServiceLocator.Instance.CurrentUser.AddGameOverCount();
                }
            }
        }
        // 현재 돌아가고 있는 모든 코루틴 종료
        StopAllCoroutines();

        float stageDuration = Time.time - _stageStartTime; // 스테이지 소요 시간
        Difficulty diff = userGameData.Difficulty;
        float totalSessionTime = Time.realtimeSinceStartup; // 게임 켜고 여기까지 시간
        int stageId = currentStageData.StageNumber;
        int totalTurns = _currentRound;

        FindAnyObjectByType<PlayLogManager>().SendStageLog(
            stageId,
            diff.ToString(),
            totalTurns,
            stageDuration,
            victory.ToString(),
            totalSessionTime
            );



        OnBattleEnded?.Invoke(victory);

    }
    #endregion

    #region Notification Methods
    private void CheckLimitEffectChange()
    {
        if (currentStageData == null || _effectNotification == null) return;

        bool shouldShow = false;
        if (currentStageData.StageNumber == 1)
        {
            shouldShow = true;
        }
        else
        {
            StageData prevStage = dataRepository.GetStage(currentStageData.StageNumber - 1);
            if (prevStage != null)
            {
                if (prevStage.LimitEffect != currentStageData.LimitEffect)
                    shouldShow = true;
            }
        }
        if (shouldShow)
            _effectNotification.Show();
    }
    #endregion

    #region Enemy Pattern Methods
    private void LoadEnemyAtIndex(int index, bool keepPlayerHP)
    {
        if (currentStageData == null || index >= currentStageData.EnemySpawns.Count)
        {
            _bgSprite = null;
            EndBattle(EndCondition.Victory); // 더 이상 적이 없으면 겜 끗 (승리 호출)
            return;
        }
        StageEnemySetup spawn = currentStageData.EnemySpawns[index];

        List<RuntimeEnemy> enemies = new List<RuntimeEnemy>
        {
            new RuntimeEnemy(spawn.enemyData, new List<int>(spawn.hitSectors))
        };
        _battleSystem.InitializeBattle(enemies, _playerMaxHP, _mapSystem.TotalSectors, _mapSystem.Columns,keepPlayerHP);
        UpdateEnemyPatterns();

        // 변경할 배경정보 받아오기
        _bgSprite = enemies[0].Data.BackGroundSprite;

        Debug.Log($"[GameManager] {_currentEnemyIndex + 1}번째 적 등장: {spawn.enemyData.Enemy_Name}");
    }


    /// <summary>
    /// 있는 적 중 패턴 번갈아가며 뽑아오는 함수
    /// </summary>
    private void UpdateEnemyPatterns()
    {
        if (_battleSystem.Enemies.Count ==0) return;
        RuntimeEnemy activeEnemy = _battleSystem.Enemies[0];

        PatternSelectionType selectionType = activeEnemy.Data.SelectionType;
        EnemyPattern nextPattern = null;

        if (selectionType == PatternSelectionType.Random)
        { 
            nextPattern = activeEnemy.GetRandomPattern();
        }
        else if (selectionType == PatternSelectionType.Sequential)
        {
            nextPattern = activeEnemy.GetNextPattern();
        }

        if (nextPattern != null)
        {
            activeEnemy.SetPattern(nextPattern);
            _timelineUI?.OnPatternChanged(nextPattern);
            if (TimelineManager.Instance != null)
            {
                TimelineManager.Instance.SetEnemyPattern(nextPattern);
            }
        }
        int currentChapter = _currentEnemyIndex + 1;
        int currentLine = activeEnemy.PatternSequenceIndex;
        int totalLine = activeEnemy.Data.Patterns.Count;
        int totalChapter = currentStageData.EnemySpawns.Count;
        OnRoundChanged?.Invoke(currentLine, totalLine, currentChapter, totalChapter);

    }

    private void UpdateEnemyImagesWrapper(int currentCure, int maxCure)
    {
        if (_battleSystem.Enemies.Count > 0)
        {
            RuntimeEnemy currentEnemy = _battleSystem.Enemies[0];
            float curePercent = 0f;
            if (maxCure > 0)
            {
                curePercent = (float)currentCure / maxCure;
            }
            _battleSequenceController.UpdateEnemyImages(currentEnemy.Data, curePercent);
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

    #region Debug Methods

    private void ToggleDebugging()
    {
        if (isDebugging == true)
        {
            isDebugging = false;
            _debugmodeChecking.SetActive(false);
        }
        else
        {
            isDebugging = true;
            _debugmodeChecking.SetActive(true);
        }
    }

    private void CtrlZAchievement()
    {
        SteamAchievementManager.Unlock("NEW_ACHIEVEMENT_16_0");
    }

    #endregion

}

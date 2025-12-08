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

    [Header("플레이어")]
    [SerializeField] private GameObject playerPrefab;
    private GameObject _currentPlayerVisual;

    [Header("데이터 참조")]
    [SerializeField] private DataRepository dataRepository;
    [SerializeField] private UserGameData userGameData;

    [Header("게임 설정")]
    [SerializeField] private int _startHandSize = 5;
    [SerializeField] private int _playerMaxHP = 20;
    [SerializeField] private int _enemyMaxHP = 20;
    [SerializeField] private MapSize _mapSize = MapSize.Sectors_8;


    // System
    private DeckSystem _deckSystem;
    private BattleSystem _battleSystem;
    private MapSystem _mapSystem;

    private TimelineManager _timelineManager;

    private int _currentRound = 0;
    private bool _isExecutingRound = false;

    public DeckSystem DeckSystem => _deckSystem;
    public BattleSystem BattleSystem => _battleSystem;
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

    }

    private void Start()
    {
        InitializeSystem();
        SetupGame();
    }

    private void InitializeSystem()
    {
        SaveService.Load(userGameData);
        if (mapRootTransform == null) mapRootTransform = this.transform;
        _timelineManager = TimelineManager.Instance;
        _deckSystem = new DeckSystem(dataRepository, userGameData);
        _battleSystem = new BattleSystem();
        _mapSystem = new MapSystem(mapConfig, mapRootTransform);

        _mapSystem.OnSectorSelected += OnStartingSectorSelected;
        _battleSystem.OnPlayerMoved += HandlePlayerVisualMove;

        if (_timelineManager != null)
            _timelineManager.Initialize(_battleSystem);

        Debug.Log("[GameManager] 시스템 초기화 완료");
    }
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
        _battleSystem.InitializeBattle(_playerMaxHP, _enemyMaxHP, _mapSystem.TotalSectors);
        _deckSystem.InitializeDeck();

        _mapSystem.EnableSelectionMode();

    }
    public void StartNewBattle()
    {
        List<RuntimeBlock> initialHand = new List<RuntimeBlock>();
        _deckSystem.DrawCards(_startHandSize);

        if (_timelineManager != null)
        {
            _timelineManager.ReceiveHand(_deckSystem.Hand);
        }
        LoadEnemyPattern();
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
        SpawnPlayer(sectorNum);
        _battleSystem.SetPlayerStartPosition(sectorNum);
        StartNewBattle();
    }

    private void SpawnPlayer(int startSectorIndex)
    {
        Vector3 startPos = _mapSystem.GetSectorPosition(startSectorIndex);

        if (playerPrefab != null)
        {
            _currentPlayerVisual = Instantiate(playerPrefab, startPos, Quaternion.identity);
        }
    }

    private void HandlePlayerVisualMove(int newSectorIndex)
    {
        Vector3 targetPos = _mapSystem.GetSectorPosition(newSectorIndex);
        if (_currentPlayerVisual != null)
        {
            _currentPlayerVisual.transform.position = targetPos;
        }
    }

}

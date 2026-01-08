using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EffectType
{
    None,
    Critical,
    Duble_Dash,
    Damage_Up
}



/// <summary>
/// 손패와 타임라인 배치를 관리하는 Director
/// TimelineSystem과 BattleSystem 사이의 중재자
/// </summary>
public class TimelineManager : MonoBehaviour
{
    public static TimelineManager Instance { get; private set; }

    [Header("특수효과 정보")]
    public EffectData effectData;

    [Header("편집 콤보")]
    public int Combo;

    [Header("자막 메모리 설정")]
    public int Max_memory;
    public int _currentMemory;
    public int Decrease_Mem;

    [Header("퀘스트 상태 클래스")]
    public QuestOptionState QuestOptionState { get; private set; }

    [Header("POC 온오프")]
    public bool Is_POC = false;
    public bool Is_One = false;
    public int POC_Damage = 4;

    [Header("타임 스케일")]
    public float timeScale = 0.5f;

    [Header("정화 온오프")]
    public bool Is_Cure = false;

    [Header("틱 설정")]
    public float Tick_interval = 0.4f;
    [SerializeField] private int _totalTicks = 8;

    // 기본적으로 8로 설정(오류 방지, Init할 때 게임매니저한테 토탈 섹터 받음)
    [SerializeField] private int _totalSectors = 8;
    private int _totalColumns = 0;


    // System
    private TimelineSystem _timelineSystem;
    private BattleSystem _battleSystem;

    private List<Additional_Effect> _placedEffect = new List<Additional_Effect>();

    // 현재 손패 (GameDirector로부터 받음)
    private List<RuntimeBlock> _currentHand = new List<RuntimeBlock>();

    // 현재 적 패턴
    private EnemyPattern _currentEnemyPattern;

    // 이벤트 (UI가 구독)
    public event Action<List<RuntimeBlock>> OnHandChanged;
    public event Action<IReadOnlyList<PlacedBlock>, IReadOnlyList<PlacedBlock>> OnTimelineChanged;
    public event Action<EnemyPattern> OnEnemyPatternChanged;
    public event Action<int> OnCurrentTickChanged;
    public event Action<IReadOnlyList<Additional_Effect>> OnEffectChanged;
    public event Action<int, int> OnTextMemoryChanged;

    // 외부 접근용 프로퍼티
    public IReadOnlyList<RuntimeBlock> CurrentHand => _currentHand;
    public IReadOnlyList<PlacedBlock> PlacedBlocks => _timelineSystem.PlacedBlocks;
    public IReadOnlyList<PlacedBlock> PrevPlacedBlocks => _timelineSystem.PrevPlacedBlocks;

    public IReadOnlyList<Additional_Effect> additional_Effects => _placedEffect;
    public int TotalTicks => _totalTicks;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // QuestOptionState 할당
        QuestOptionState = new QuestOptionState();

        // TimelineSystem 초기화
        _timelineSystem = new TimelineSystem();

        // TimelineSystem 이벤트 구독 (중재자 역할)
        _timelineSystem.OnNoneStarted += HandleNonetick;
        _timelineSystem.OnMeleeAttackRequested += HandleMeleeAttackRequest;
        _timelineSystem.OnLongRangeAttackRequested += HandleLongRangeAttackRequest;
        _timelineSystem.OnMoveRequested += HandleMoveRequest;
        _timelineSystem.OnBlockStarted += HandleBlockStarted;
        _timelineSystem.OnBlockEnded += HandleBlockEnded;
        _timelineSystem.OnBlockTick += HandleBlockTick;
        _timelineSystem.OnLongRangeAttackStarted += HandleLongRangeAttack_Start;
        _timelineSystem.OnMeleeAttackStarted += HandleMeleeAttack_Start;
        _timelineSystem.OnGuardRequested += HandleGuardRequest;
        _timelineSystem.OnLongRangeAttacking += HandleLongRangeAttack_Middle;

        // 이펙트 초기화
        InitializedEffect(_totalTicks);
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (_timelineSystem != null)
        {
            _timelineSystem.OnNoneStarted -= HandleNonetick;
            _timelineSystem.OnMeleeAttackRequested -= HandleMeleeAttackRequest;
            _timelineSystem.OnLongRangeAttackRequested -= HandleLongRangeAttackRequest;
            _timelineSystem.OnMoveRequested -= HandleMoveRequest;
            _timelineSystem.OnBlockStarted -= HandleBlockStarted;
            _timelineSystem.OnBlockEnded -= HandleBlockEnded;
            _timelineSystem.OnBlockTick -= HandleBlockTick;
            _timelineSystem.OnLongRangeAttackStarted -= HandleLongRangeAttack_Start;
            _timelineSystem.OnLongRangeAttacking -= HandleLongRangeAttack_Middle;
            _timelineSystem.OnMeleeAttackStarted -= HandleMeleeAttack_Start;
            _timelineSystem.OnGuardRequested -= HandleGuardRequest;
        }
    }

    // ========================================
    // TimelineSystem 이벤트 핸들러 (중재자)
    // ========================================

    private void HandleMeleeAttackRequest(BlockData data, bool isChargeRequired)
    {
        int currentStack = _battleSystem.MeleeAttackStack;
        int finalDamage = data.CalculateStackedDamage(currentStack);
        bool isSuccess = _battleSystem.MeleeAttack(finalDamage, isChargeRequired);
        if (isSuccess)
        {
            //_battleSystem.IncreaseMeleeStack();
        }
    }
    private void HandleMeleeAttack_Start()
    {
        _battleSystem.MeleeAttack_Start();
    }
    private void HandleLongRangeAttackRequest(int power, bool isChargeRequired)
    {
        _battleSystem.LongRangeAttack(power, isChargeRequired);
    }

    private void HandleLongRangeAttack_Start()
    {
        _battleSystem.LongRangeAttack_Start();
    }

    private void HandleLongRangeAttack_Middle()
    {
        _battleSystem.LongRangeAttack_Middle();
    }

    private void HandleMoveRequest(MoveDirection direction)
    {
        _battleSystem.MovePlayer(direction);
    }
    private void HandleGuardRequest(bool state)
    {
        _battleSystem.SetGuard(state);

        if (state)
            _battleSystem.Guard();
    }

    private void HandleBlockStarted(PlacedBlock placed, RuntimeBlock runtime, int tick)
    {
        // 만약 이 키워드가 잔상에 붙어있는 키워드면 실행 안함
        if (_timelineSystem.PrevPlacedBlocks.Contains(placed)) return;

        // 키워드 OnBlockStart 호출
        foreach (KeywordData keyword in runtime.AttachedKeywords)
        {
            keyword.OnBlockStart(placed, tick, _battleSystem);
        }
    }

    private void HandleBlockEnded(PlacedBlock placed, RuntimeBlock runtime, int tick)
    {
        // 만약 이 키워드가 잔상에 붙어있는 키워드면 실행 안함
        if (_timelineSystem.PrevPlacedBlocks.Contains(placed)) return;

        // 키워드 OnBlockEnded 호출
        foreach (KeywordData keyword in runtime.AttachedKeywords)
        {
            keyword.OnBlockEnded(placed, tick, _battleSystem);
        }
    }

    private void HandleBlockTick(PlacedBlock placed, RuntimeBlock runtime, int tick, ActionType action)
    {
        //틱당 크리 확률 증가
        _battleSystem.IncreaseMeleeStack();

        // 특수효과 플래그 처리
        _battleSystem.SetEffectFrag(GetEffectTypeAt(tick));

        // 만약 이 키워드가 잔상에 붙어있는 키워드면 실행 안함
        if (_timelineSystem.PrevPlacedBlocks.Contains(placed)) return;

        // 키워드 OnTick 호출
        foreach (KeywordData keyword in runtime.AttachedKeywords)
        {
            keyword.OnTick(placed, tick, action, _battleSystem);
        }
    }

    private void HandleNonetick()
    {
        _battleSystem.ResetMeleeStack();
        _battleSystem.Release_Guard();
    }
    // ========================================
    // 공개 메서드
    // ========================================

    /// <summary>
    /// GameDirector로부터 손패를 받음
    /// </summary>
    public void ReceiveHand(IReadOnlyList<RuntimeBlock> hand)
    {
        _currentHand.Clear();
        _currentHand.AddRange(hand);

        Debug.Log($"[TimelineDirector] 손패 수신: {_currentHand.Count}장");

        OnHandChanged?.Invoke(_currentHand);
    }

    /// <summary>
    /// 적 패턴 설정
    /// </summary>
    public void SetEnemyPattern(EnemyPattern pattern)
    {
        _currentEnemyPattern = pattern;
        OnEnemyPatternChanged?.Invoke(pattern);

        Debug.Log($"[TimelineDirector] 적 패턴 설정 완료");
    }

    /// <summary>
    /// 블록을 타임라인에 배치 시도
    /// </summary>
    public bool TryPlaceBlock(RuntimeBlock runtimeBlock, int startTick)
    {
        // 손패에 있는 블록인지 확인
        if (!_currentHand.Contains(runtimeBlock))
        {
            Debug.LogWarning("[TimelineDirector] 손패에 없는 블록을 배치하려 했습니다");
            return false;
        }

        // TimelineSystem에 배치 요청
        bool success = _timelineSystem.TryPlaceBlock(runtimeBlock, startTick);

        if (success)
        {
            // 손패에서 제거
            _currentHand.Remove(runtimeBlock);

            Debug.Log($"[TimelineDirector] 블록 배치 성공: {runtimeBlock.BaseData.BlockName} at T{startTick}");

            // UI 업데이트
            OnHandChanged?.Invoke(_currentHand);
            OnTimelineChanged?.Invoke(_timelineSystem.PlacedBlocks, _timelineSystem.PrevPlacedBlocks);

            return true;
        }

        return false;
    }

    public bool TryMoveBlock_OnTimeline(RuntimeBlock runtimeBlock, int startTick) 
    {
        // TimelineSystem에 배치 요청
        bool success = _timelineSystem.TryPlaceBlock(runtimeBlock, startTick);

        if (success)
        {
            // UI 업데이트
            OnTimelineChanged?.Invoke(_timelineSystem.PlacedBlocks, _timelineSystem.PrevPlacedBlocks);

            return true;
        }

        return false;
    }

    /// <summary>
    /// 타임라인에서 블록 제거
    /// </summary>
    public void RemovePlacedBlock(PlacedBlock placedBlock)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.Play(SoundID.SFX_Spell_Cancle);

        RuntimeBlock runtimeBlock = _timelineSystem.RemovePlacedBlock(placedBlock);

        if (runtimeBlock != null)
        {
            // 손패로 복귀 전 방향 초기화
            runtimeBlock.InitializeDirections();

            // 손패로 복귀
            _currentHand.Add(runtimeBlock);

            Debug.Log($"[TimelineDirector] 블록 제거: {runtimeBlock.BaseData.BlockName}");

            // UI 업데이트
            OnHandChanged?.Invoke(_currentHand);
            OnTimelineChanged?.Invoke(_timelineSystem.PlacedBlocks, _timelineSystem.PrevPlacedBlocks);
        }
    }

    public void RemovePlacedBlock_OnTimeline(PlacedBlock placedBlock)
    {
        RuntimeBlock runtimeBlock = _timelineSystem.RemovePlacedBlock(placedBlock);
        
        // UI 업데이트
        OnTimelineChanged?.Invoke(_timelineSystem.PlacedBlocks, _timelineSystem.PrevPlacedBlocks);
    }

    public void ReturnToHand(RuntimeBlock runtimeBlock)
    {
        if (runtimeBlock != null)
        {
            // 손패로 복귀 전 방향 초기화
            runtimeBlock.InitializeDirections();

            // 손패로 복귀
            _currentHand.Add(runtimeBlock);

            Debug.Log($"[TimelineDirector] 손패로 복귀: {runtimeBlock.BaseData.BlockName}");

            // UI 업데이트
            OnHandChanged?.Invoke(_currentHand);
            OnTimelineChanged?.Invoke(_timelineSystem.PlacedBlocks, _timelineSystem.PrevPlacedBlocks);
        }
    }
    public void ToggleBlockDirectionForMove(PlacedBlock placedBlock, int tick)
    {
        if (placedBlock == null || placedBlock.linkedRuntimeBlock == null) return;
        int index = tick - placedBlock.startTick;
        placedBlock.linkedRuntimeBlock.ToggleDirectionsForMove(index);
        Debug.Log($"[TimelineManager] 방향 전환: T{tick}");
        OnTimelineChanged?.Invoke(_timelineSystem.PlacedBlocks, _timelineSystem.PrevPlacedBlocks);
    }
    public void ToggleBlockDirectionForJump(PlacedBlock placedBlock, int tick)
    {
        if (placedBlock == null || placedBlock.linkedRuntimeBlock == null) return;
        int index = tick - placedBlock.startTick;
        placedBlock.linkedRuntimeBlock.ToggleDirectionsForJump(index);
        Debug.Log($"[TimelineManager] 방향 전환: T{tick}");
        OnTimelineChanged?.Invoke(_timelineSystem.PlacedBlocks, _timelineSystem.PrevPlacedBlocks);
    }

    // ========================================
    // 특수효과 관련
    // ========================================
    private void InitializedEffect(int size)
    {
        _placedEffect.Clear();

        for (int i = 0; i < size; i++)
        {
           _placedEffect.Add(null);
        }
    }

    public bool TryPlaceEffect(Additional_Effect effect, int startTick) 
    {
        if (CanPlaceEffect(startTick) && effect.cost + _currentMemory <= Max_memory)
        {
            _placedEffect[startTick - 1] = effect;
            _currentMemory += effect.cost;

            OnEffectChanged.Invoke(_placedEffect);
            OnTextMemoryChanged?.Invoke(_currentMemory, Max_memory);
            return true;
        }

        return false;
    }

    public void RemovePlacedEffect(Additional_Effect effect)
    {
        _placedEffect.Remove(effect);
        OnEffectChanged.Invoke(_placedEffect);
        OnTextMemoryChanged?.Invoke(_currentMemory, Max_memory);
    }

    public void RemovePlacedEffect_Index(int tick)
    {
        _currentMemory -= _placedEffect[tick - 1].cost;
        _placedEffect[tick - 1] = null;
        OnEffectChanged.Invoke(_placedEffect);
        OnTextMemoryChanged?.Invoke(_currentMemory, Max_memory);
    }

    public bool CanPlaceEffect(int startTick) 
    {
        if (_placedEffect[startTick - 1] == null)
            return true;
        return false;
    }

    public EffectType GetEffectTypeAt(int tick)
    {
        Additional_Effect effect = _placedEffect[tick - 1];
        if (effect != null)
            return effect.effectType;
        return EffectType.None;
    }

    public int GetEffectCostAt(int tick)
    {
        Additional_Effect effect = _placedEffect[tick - 1];
        if (effect != null)
            return effect.cost;
        return 0;
    }

    private bool Count_Effect()
    {
        int count = 0;
        foreach (Additional_Effect effect in _placedEffect)
        {
            if (effect != null)
                count++;
        }
        return count < Combo / 8;
    }

    /// <summary>
    /// 타임라인 실행
    /// </summary>
    public IEnumerator ExecuteTimeline()
    {
        float startTime = Time.unscaledTime;   // 시작 시간 기록
        // 늦추기
        Time.timeScale = timeScale;
        Debug.Log("[TimelineDirector] 타임라인 실행 시작");

        // 라운드 시작 키워드 호출
        List<(PlacedBlock, RuntimeBlock)> allBlocks = _timelineSystem.GetAllPlacedBlocksWithRuntime();
        foreach (var (placed, runtime) in allBlocks)
        {
            foreach (KeywordData keyword in runtime.AttachedKeywords)
            {
                keyword.OnRoundStart(placed);
            }
        }

        for (int tick = 1; tick <= _totalTicks; tick++)
        {
            // 매 틱마다 방어 초기화하고 시작
            _battleSystem.SetGuard(false);
            if (GameManager.Instance.IsRoundInterrupted)
            {
                OnCurrentTickChanged?.Invoke(0);
                break;
            }
            //틱이 분리됨에 따른 틱 쪼개기
            OnCurrentTickChanged?.Invoke(2*tick - 1);

            Debug.Log($"[TimelineDirector] --- 틱 {tick} ---");

            // 1. 플레이어 블록 처리 (TimelineSystem이 이벤트 발행 → Director가 BattleSystem 호출)
            _timelineSystem.ProcessTick(tick);
            if (GameManager.Instance.IsRoundInterrupted)
            {
                OnCurrentTickChanged?.Invoke(0);
                break;
            }
            // 플레이어 행동 끝나는 시점에 돌 다 없애기
            if (tick == _totalTicks)
            {
                _battleSystem.ClearStones();
            }

            yield return new WaitForSeconds(Tick_interval);
            if (GameManager.Instance.IsBattleEnded) yield break;

            //// 정화 시도
            //if (_battleSystem.TryCurePage(tick, Is_Cure))
            //{
            //    // TODO: 정화 이펙트 넣기
            //    yield return new WaitForSeconds(Tick_interval);
            //}

            //틱이 분리됨에 따른 틱 쪼개기
            OnCurrentTickChanged?.Invoke(2 * tick);

            // 2. 적 공격 처리
            if (_currentEnemyPattern != null && _battleSystem != null)
            {
                EnemyAttack attack = _currentEnemyPattern.GetAttackAt(tick);
                if (attack != null)
                {
                    _battleSystem.ProcessEnemyAttack(attack);
                }
                EnemyStone stone = _currentEnemyPattern.GetStoneAt(tick);
                if (stone != null)
                {
                    _battleSystem.ProcessEnemyStone(stone.count);
                }
                EnemyWind wind = _currentEnemyPattern?.GetWindAt(tick);
                if (wind != null)
                {
                    WindDirection actualDirection = wind.GetDynamicDirection(_currentEnemyPattern.Get_Is_left());
                    _battleSystem.ProcessEnemyWind(actualDirection);
                }
                EnemyDash dash = _currentEnemyPattern?.GetDashAt(tick);
                if (dash != null)
                {
                    _battleSystem.ProcessEnemyDash(dash);
                }

            }

            if (GameManager.Instance.IsRoundInterrupted)
            {
                OnCurrentTickChanged?.Invoke(0);
                break;
            }

            // 연출 대기
            yield return new WaitForSeconds(Tick_interval);
            // 모든 틱 끝나면 0 으로 신호 보내서 하이라이트 끄기
            OnCurrentTickChanged?.Invoke(0);
        }

        // 라운드 종료 키워드 호출
        allBlocks = _timelineSystem.GetAllPlacedBlocksWithRuntime();
        foreach (var (placed, runtime) in allBlocks)
        {
            foreach (KeywordData keyword in runtime.AttachedKeywords)
            {
                keyword.OnRoundEnded(placed);
            }
        }

        float elapsed = Time.unscaledTime - startTime;   // 총 실행 시간
        Debug.Log($"[TimelineDirector] 타임라인 실행 완료 - 총 소요 시간: {elapsed:F2}초");
        Time.timeScale = 1f;
        //_battleSystem.ResetMeleeStack();
    }

    /// <summary>
    /// 라운드 종료 시 호출 (GameDirector가 호출)
    /// </summary>
    public void OnRoundEnded()
    {
        // 방향 전환할 블록들 보관하는 리스트
        List<RuntimeBlock> blocksToReset = new List<RuntimeBlock>();
        foreach (PlacedBlock placed in _timelineSystem.PlacedBlocks)
        {
            if (placed.linkedRuntimeBlock != null)
            {
                blocksToReset.Add(placed.linkedRuntimeBlock);
            }
        }
        // 현재 배치를 이전 배치로 이동 (잔상용)
        _timelineSystem.SaveCurrentAsPreview();

        foreach (RuntimeBlock rb in blocksToReset)
        {
            rb.InitializeDirections();
        }

        // 손패 클리어 (GameDirector가 새로 줄 예정)
        _currentHand.Clear();

        // 퀘스트 보상 메모리 처리
        if (!QuestOptionState.IsHit)
        {
            DecreaseMem();
        }

        if (QuestOptionState.IsEight)
        {
            DecreaseMem();
        }

        if (QuestOptionState.IsTwice)
        {
            DecreaseMem();
        }

        // 퀘스트 상태 초기화
        QuestOptionState.Initialize();

        //아드 UI업뎃
        //BattleUIManager.Instance.UpdateStackUI();
        OnTextMemoryChanged?.Invoke(_currentMemory, Max_memory);

        // 이펙트 클리어
        InitializedEffect(_totalTicks);
        OnEffectChanged?.Invoke(_placedEffect);

        Debug.Log("[TimelineDirector] 라운드 종료 처리 완료");

        // UI 업데이트
        OnHandChanged?.Invoke(_currentHand);
        OnTimelineChanged?.Invoke(_timelineSystem.PlacedBlocks, _timelineSystem.PrevPlacedBlocks);
    }

    private void DecreaseMem()
    {
        _currentMemory -= Decrease_Mem;

        if( _currentMemory < 0)
        {
            _currentMemory = 0;
        }
    }

    /// <summary>
    /// 특정 틱 범위가 배치 가능한지 확인 (UI에서 미리보기용)
    /// </summary>
    public bool CanPlaceAt(int startTick, int length, RuntimeBlock runtimeBlock)
    {
        return _timelineSystem.CanPlaceBlock(startTick, length, runtimeBlock);
    }

    public void Initialize(BattleSystem battleSystem, int totalSector, int columns)
    {
        _battleSystem = battleSystem;
        _totalSectors = totalSector;
        _totalColumns = columns;
        OnTextMemoryChanged?.Invoke(_currentMemory, Max_memory);
    }

    #region Preview Methods - public
    /// <summary>
    /// 특정 틱의 플레이어의 위치를 시뮬레이션해 반환하는 함수
    /// </summary>
    public int SimulatePlayerPosition(int targetTick)
    {
        if (_battleSystem == null || targetTick <= 0)
        {
            return _battleSystem != null ? _battleSystem.PlayerCurrentSector : 1;
        }
        int currentSimulatedSector = _battleSystem.PlayerCurrentSector;
        int columns = _totalColumns > 0 ? _totalColumns : 3; // 호옥시 모를 오류 방지
        int rows = _totalSectors / columns;

        for (int t = 1; t <= targetTick; t++)
        {
            EffectType currentTickEffect = GetEffectTypeAt(t);
            int moveAmount = (currentTickEffect == EffectType.Duble_Dash) ? 2 : 1;
            PlacedBlock placed = _timelineSystem.FindFirstAction(t);

            if (placed != null)
            {
                int cardIndex = placed.GetCardTickIndex(t);
                BlockData data = placed.GetBlockData();
                ActionType action = data.GetEffectAt(cardIndex);

                // 이동 액션일 경우 시뮬레이션
                if (action == ActionType.Move || action == ActionType.Jump)
                {
                    MoveDirection dir = placed.GetDirectionAt(cardIndex);
                    if (dir != MoveDirection.None)
                    {
                        int nextSector = CalculateNextSector(currentSimulatedSector, dir, columns, rows, moveAmount);
                        if (nextSector != -1 && !_battleSystem.IsSectorBlocked(nextSector))
                        {
                            currentSimulatedSector = nextSector;
                        }
                    }
                }
            }
            if (_currentEnemyPattern != null)
            {
                EnemyWind wind = _currentEnemyPattern.GetWindAt(t);
                if (wind != null)
                {
                    WindDirection actualDirection = wind.GetDynamicDirection(_currentEnemyPattern.Get_Is_left());
                    int windTarget = _battleSystem.GetWindTargetSector(currentSimulatedSector, actualDirection);

                    if (windTarget != -1 && !_battleSystem.IsSectorBlocked(windTarget))
                    {
                        currentSimulatedSector = windTarget;
                    }
                }
            }
        }

        return currentSimulatedSector;
    }

    /// <summary>
    /// 좌표 계산용 헬퍼 함수 (Preview 전용)
    /// </summary>
    private int CalculateNextSector(int currentSector, MoveDirection dir, int columns, int rows, int moveAmount)
    {
        int curIdx = currentSector - 1;
        int r = curIdx / columns;
        int c = curIdx % columns;

        switch (dir)
        {
            case MoveDirection.Front: c += moveAmount; break;
            case MoveDirection.Back: c -= moveAmount; break;
            case MoveDirection.Left: r -= moveAmount; break;
            case MoveDirection.Right: r += moveAmount; break;
            case MoveDirection.DiagonalLu: r -= moveAmount; c += moveAmount; break;
            case MoveDirection.DiagonalRu: r += moveAmount; c += moveAmount; break;
            case MoveDirection.DiagonalLd: r -= moveAmount; c -= moveAmount; break;
            case MoveDirection.DiagonalRd: r += moveAmount; c -= moveAmount; break;
        }

        if (r >= 0 && r < rows && c >= 0 && c < columns)
            return (r * columns) + 1 + c;

        return -1;

    }

    public List<int> GetEnemyAttackSectors(int tick)
    {
        List<int> sectors = new List<int>();
        if (_currentEnemyPattern != null)
        {
            // 적 일반 공격 섹터 추가
            EnemyAttack attack = _currentEnemyPattern.GetAttackAt(tick);
            if (attack != null && attack.targetSectors != null)
                sectors.AddRange(attack.targetSectors);

            // 적 돌진 공격 섹터 추가
            EnemyDash dash = _currentEnemyPattern.GetDashAt(tick);
            if (dash != null && dash.targetRows != null)
            {
                List<int> dashSectors = dash.Convert_9sector();
                foreach(int sector in dashSectors)
                {
                    if (!sectors.Contains(sector))
                        sectors.Add(sector);
                }
            }
        }
        return sectors;
    }

    public ActionType GetActionAtTick(int targetTick)
    {
        PlacedBlock placed = _timelineSystem.FindFirstAction(targetTick);
        if (placed != null)
        {
            int cardIndex = placed.GetCardTickIndex(targetTick);
            return placed.GetBlockData().GetEffectAt(cardIndex);
        }
        return ActionType.None;
    }

    public List<int> GetProjectedDangerTicks()
    {
        List<int> dangerTicks = new List<int>();
        if (_battleSystem == null || _currentEnemyPattern == null) return dangerTicks;

        int currentSimulatedSector = _battleSystem.PlayerCurrentSector;

        int columns = _totalColumns;
        int rows = _totalSectors /  columns;

        for (int t = 1; t <= _totalTicks; t++)
        {
            currentSimulatedSector = SimulatePlayerPosition(t);
            
            EnemyAttack attack = _currentEnemyPattern.GetAttackAt(t);
            if (attack != null && attack.targetSectors != null)
            {
                if (attack.targetSectors.Contains(currentSimulatedSector))
                {
                    dangerTicks.Add(t);
                    continue;
                }
            }
            EnemyDash dash = _currentEnemyPattern.GetDashAt(t);
            if (dash != null)
            {
                bool isHit = false;
                foreach (int row in dash.targetRows)
                {
                    for (int col = 0; col < columns; col++)
                    {
                        int dashSector = (row * columns) + col + 1;
                        if (dashSector == currentSimulatedSector)
                        {
                            isHit = true;
                            break;
                        }
                    }
                    if (isHit) break;
                }
                if (isHit) dangerTicks.Add(t);
            }

        }
        return dangerTicks;

    }
    #endregion
}
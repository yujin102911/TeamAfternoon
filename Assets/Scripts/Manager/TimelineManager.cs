using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 손패와 타임라인 배치를 관리하는 Director
/// TimelineSystem과 BattleSystem 사이의 중재자
/// </summary>
public class TimelineManager : MonoBehaviour
{
    public static TimelineManager Instance { get; private set; }

    [Header("정화 온오프")]
    public bool Is_Cure = false;

    [SerializeField] private int _totalTicks = 8;

    // System
    private TimelineSystem _timelineSystem;
    private BattleSystem _battleSystem;

    // 현재 손패 (GameDirector로부터 받음)
    private List<RuntimeBlock> _currentHand = new List<RuntimeBlock>();

    // 현재 적 패턴
    private EnemyPattern _currentEnemyPattern;

    // 이벤트 (UI가 구독)
    public event Action<List<RuntimeBlock>> OnHandChanged;
    public event Action<IReadOnlyList<PlacedBlock>, IReadOnlyList<PlacedBlock>> OnTimelineChanged;
    public event Action<EnemyPattern> OnEnemyPatternChanged;
    public event Action<int> OnCurrentTickChanged;

    // 외부 접근용 프로퍼티
    public IReadOnlyList<RuntimeBlock> CurrentHand => _currentHand;
    public IReadOnlyList<PlacedBlock> PlacedBlocks => _timelineSystem.PlacedBlocks;
    public IReadOnlyList<PlacedBlock> PrevPlacedBlocks => _timelineSystem.PrevPlacedBlocks;
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

        // TimelineSystem 초기화
        _timelineSystem = new TimelineSystem();

        // TimelineSystem 이벤트 구독 (중재자 역할)
        _timelineSystem.OnAttackRequested += HandleAttackRequest;
        _timelineSystem.OnMoveRequested += HandleMoveRequest;
        _timelineSystem.OnBlockStarted += HandleBlockStarted;
        _timelineSystem.OnBlockEnded += HandleBlockEnded;
        _timelineSystem.OnBlockTick += HandleBlockTick;
        _timelineSystem.OnCureRequested += HandleCureRequest;
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (_timelineSystem != null)
        {
            _timelineSystem.OnAttackRequested -= HandleAttackRequest;
            _timelineSystem.OnMoveRequested -= HandleMoveRequest;
            _timelineSystem.OnBlockStarted -= HandleBlockStarted;
            _timelineSystem.OnBlockEnded -= HandleBlockEnded;
            _timelineSystem.OnBlockTick -= HandleBlockTick;
            _timelineSystem.OnCureRequested -= HandleCureRequest;
        }
    }

    // ========================================
    // TimelineSystem 이벤트 핸들러 (중재자)
    // ========================================

    private void HandleAttackRequest(int baseDamage, int currentTick)
    {
        _battleSystem.DealDamageToCurrentSector(baseDamage, currentTick);
    }

    private void HandleMoveRequest(MoveDirection direction)
    {
        _battleSystem.MovePlayer(direction);
    }

    private void HandleCureRequest(int power)
    {
        _battleSystem.Cure(power);
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
        // 만약 이 키워드가 잔상에 붙어있는 키워드면 실행 안함
        if (_timelineSystem.PrevPlacedBlocks.Contains(placed)) return;

        // 키워드 OnTick 호출
        foreach (KeywordData keyword in runtime.AttachedKeywords)
        {
            keyword.OnTick(placed, tick, action, _battleSystem);
        }

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
    public void ToggleBlockDirection(PlacedBlock placedBlock, int tick)
    {
        if (placedBlock == null || placedBlock.linkedRuntimeBlock == null) return;
        int index = tick - placedBlock.startTick;
        placedBlock.linkedRuntimeBlock.ToggleDirections(index);
        Debug.Log($"[TimelineManager] 방향 전환: T{tick}");
        OnTimelineChanged?.Invoke(_timelineSystem.PlacedBlocks, _timelineSystem.PrevPlacedBlocks);
    }

    /// <summary>
    /// 타임라인 실행
    /// </summary>
    public IEnumerator ExecuteTimeline()
    {
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

            yield return new WaitForSeconds(0.4f);
            if (GameManager.Instance.IsBattleEnded) yield break;

            //// 정화 시도
            //if (_battleSystem.TryCurePage(tick, Is_Cure))
            //{
            //    // TODO: 정화 이펙트 넣기
            //    yield return new WaitForSeconds(0.4f);
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

            }

            if (GameManager.Instance.IsRoundInterrupted)
            {
                OnCurrentTickChanged?.Invoke(0);
                break;
            }

            // 연출 대기
            yield return new WaitForSeconds(0.4f);
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

        Debug.Log("[TimelineDirector] 타임라인 실행 완료");
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

        Debug.Log("[TimelineDirector] 라운드 종료 처리 완료");

        // UI 업데이트
        OnHandChanged?.Invoke(_currentHand);
        OnTimelineChanged?.Invoke(_timelineSystem.PlacedBlocks, _timelineSystem.PrevPlacedBlocks);
    }

    /// <summary>
    /// 특정 틱 범위가 배치 가능한지 확인 (UI에서 미리보기용)
    /// </summary>
    public bool CanPlaceAt(int startTick, int length, RuntimeBlock runtimeBlock)
    {
        return _timelineSystem.CanPlaceBlock(startTick, length, runtimeBlock);
    }

    public void Initialize(BattleSystem battleSystem)
    {
        _battleSystem = battleSystem;
    }

    #region Preview Methods - public
    /// <summary>
    /// 특정 틱의 플레이어의 위치를 시뮬레이션해 반환하는 함수
    /// </summary>
    public int SimulatePlayerPosition(int targetTick)
    {
        if (_battleSystem == null) return 1;
        int currentSimulatedSector = _battleSystem.PlayerCurrentSector;
        int baseBuffSpeed = _battleSystem.GetBuffValue("Speed", true);
        int columns = 3; // TODO: 추후 MapSystem에 있는 정보를 가져올 수 있도록 수정

        for (int t = 1; t <= targetTick; t++)
        {
            PlacedBlock placed = _timelineSystem.FindFirstAction(t);

            if (placed != null)
            {
                int cardIndex = placed.GetCardTickIndex(t);
                BlockData data = placed.GetBlockData();

                // 이번 틱의 속도 계산
                int currentTickSpeed = 1 + baseBuffSpeed;
                if (placed.linkedRuntimeBlock != null)
                {
                    foreach (var keyword in placed.linkedRuntimeBlock.AttachedKeywords)
                    {
                        currentTickSpeed += keyword.GetSpeedBonus();
                    }
                }

                // 이동 액션일 경우 시뮬레이션
                if (data.GetEffectAt(cardIndex) == ActionType.Move)
                {
                    MoveDirection dir = placed.GetDirectionAt(cardIndex);

                    if (dir != MoveDirection.None)
                    {
                        int direction = (dir == MoveDirection.Right) ? 1 : -1;

                        int moveAmount = 1 + baseBuffSpeed;

                        switch (dir)
                        {
                            case MoveDirection.Front: currentSimulatedSector += moveAmount; break;
                            case MoveDirection.Back: currentSimulatedSector -= moveAmount; break;
                            case MoveDirection.Left: currentSimulatedSector -= (columns * moveAmount); break;
                            case MoveDirection.Right: currentSimulatedSector += (columns * moveAmount); break;
                        }
                        currentSimulatedSector = Mathf.Clamp(currentSimulatedSector, 1, 8); // TODO: totalSector사용가능하면 사용하도록
                    }
                }
            }
        }

        return currentSimulatedSector;
    }

    public List<int> GetEnemyAttackSectors(int tick)
    {
        List<int> sectors = new List<int>();
        if (_currentEnemyPattern != null)
        {
            EnemyAttack attack = _currentEnemyPattern.GetAttackAt(tick);
            if (attack != null && attack.targetSectors != null)
                sectors.AddRange(attack.targetSectors);
        }
        return sectors;
    }

    public List<int> GetProjectedDangerTicks()
    {
        List<int> dangerTicks = new List<int>();
        if (_battleSystem == null || _currentEnemyPattern == null) return dangerTicks;

        int currentSimulatedSector = _battleSystem.PlayerCurrentSector;
        int baseBuffSpeed = _battleSystem.GetBuffValue("Speed", true);
        int totalSectors = 8;  // _mapSystem.TotalSectors 접근 가능하면 사용

        for (int t = 1; t <= _totalTicks; t++)
        {
            PlacedBlock placed = _timelineSystem.FindFirstAction(t);
            if (placed != null)
            {
                int cardIndex = placed.GetCardTickIndex(t);
                BlockData data = placed.GetBlockData();
                if (data.GetEffectAt(cardIndex) == ActionType.Move)
                {
                    int currentTickSpeed = 1 + baseBuffSpeed;
                    if (placed.linkedRuntimeBlock != null)
                    {
                        foreach (var keyword in placed.linkedRuntimeBlock.AttachedKeywords)
                        {
                            currentTickSpeed += keyword.GetSpeedBonus();
                        }
                    }
                    MoveDirection dir = placed.GetDirectionAt(cardIndex);
                    if (dir != MoveDirection.None)
                    {
                        int direction = (dir == MoveDirection.Right ? 1 : -1);
                        currentSimulatedSector += (direction * currentTickSpeed);

                        while (currentSimulatedSector > totalSectors) currentSimulatedSector -= totalSectors;
                        while (currentSimulatedSector < 1) currentSimulatedSector += totalSectors;
                    } 
                }

            }
            EnemyAttack attack = _currentEnemyPattern.GetAttackAt(t);
            if (attack != null && attack.targetSectors != null)
            {
                if (attack.targetSectors.Contains(currentSimulatedSector))
                    dangerTicks.Add(t);
            }
        }
        return dangerTicks;

    }
    #endregion
}
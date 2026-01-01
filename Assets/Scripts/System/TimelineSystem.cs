using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.VolumeComponent;

/// <summary>
/// 타임라인 배치 로직을 처리하는 System
/// PlacedBlocks 관리, 배치 가능 여부 체크, 틱 처리 등
/// 직접 실행하지 않고 이벤트로 요청만 함
/// </summary>
public class TimelineSystem
{
    // 현재 배치된 블록들
    private List<PlacedBlock> _placedBlocks = new List<PlacedBlock>();

    // 이전 라운드 배치 (잔상용)
    private List<PlacedBlock> _prevPlacedBlocks = new List<PlacedBlock>();

    // RuntimeBlock 참조 저장 (제거 시 필요)
    private Dictionary<PlacedBlock, RuntimeBlock> _blockMap = new Dictionary<PlacedBlock, RuntimeBlock>();

    // 과거 RuntimeBlock 검증용
    private Dictionary<PlacedBlock, RuntimeBlock> _prevblockMap = new Dictionary<PlacedBlock, RuntimeBlock>();

    // ========================================
    // 이벤트 정의 (Director가 구독)
    // ========================================

    /// <summary>
    /// 근접 공격 요청 이벤트 (데미지)
    /// </summary>
    public event Action<BlockData, bool> OnMeleeAttackRequested;

    /// <summary>
    /// 근접 공격 (검 차징) 시작 이벤트
    /// </summary>
    public event Action OnMeleeAttackStarted;

    /// <summary>
    /// 원거리 공격 요청 이벤트 (데미지)
    /// </summary>
    public event Action<int, bool> OnLongRangeAttackRequested;

    /// <summary>
    /// 원거리 공격 시작 이벤트 (활 차징 애니매이션 재생)
    /// </summary>
    public event Action OnLongRangeAttackStarted;

    /// <summary>
    /// 원거리 공격 차징 중 이벤트 (2번째)
    /// </summary>
    public event Action OnLongRangeAttacking;

    /// <summary>
    /// 방어 요청 이벤트 (방어여부)
    /// </summary>
    public event Action<bool> OnGuardRequested;

    /// <summary>
    /// 이동 요청 이벤트 (방향)
    /// </summary>
    public event Action<MoveDirection> OnMoveRequested;

    /// <summary>
    /// 블록 시작 이벤트 (키워드 처리용)
    /// </summary>
    public event Action<PlacedBlock, RuntimeBlock, int> OnBlockStarted;

    /// <summary>
    /// 블록 종료 이벤트 (키워드 처리용)
    /// </summary>
    public event Action<PlacedBlock, RuntimeBlock, int> OnBlockEnded;

    /// <summary>
    /// 틱 이벤트 (키워드 처리용)
    /// </summary>
    public event Action<PlacedBlock, RuntimeBlock, int, ActionType> OnBlockTick;


    // 외부 접근용 프로퍼티
    public IReadOnlyList<PlacedBlock> PlacedBlocks => _placedBlocks;
    public IReadOnlyList<PlacedBlock> PrevPlacedBlocks => _prevPlacedBlocks;

    private void Check_PlaceBlockLength()
    {
        int length = 0;
        foreach (PlacedBlock block in _placedBlocks)
        {
            length += block.linkedRuntimeBlock.BaseData.blockLength;
        }

        Debug.LogWarning($"[TimelineSystem] 현재 놓인 블럭의 총 길이 {length}");

        if (length == TimelineManager.Instance.TotalTicks)
        {
            TimelineManager.Instance.Is_Eight = true;
        }
        else
        {
            TimelineManager.Instance.Is_Eight = false;
        }
    }

    /// <summary>
    /// 블록을 배치 시도
    /// </summary>
    public bool TryPlaceBlock(RuntimeBlock runtimeBlock, int startTick)
    {
        if (runtimeBlock == null || runtimeBlock.BaseData == null)
        {
            Debug.LogWarning("[TimelineSystem] 잘못된 RuntimeBlock");
            return false;
        }

        int length = runtimeBlock.BaseData.BlockLength;

        // 배치 가능 여부 확인
        if (!CanPlaceBlock(startTick, length, runtimeBlock))
        {
            Debug.LogWarning($"[TimelineSystem] T{startTick}에 배치 불가 (길이: {length})");
            return false;
        }

        // PlacedBlock 생성
        Saved_BlockData savedData = new Saved_BlockData
        {
            Owner_blockID = runtimeBlock.BlockID,
            Attached_Keyword_IDs = runtimeBlock.AttachedKeywords.ConvertAll(k => k.KeywordID)
        };

        PlacedBlock placedBlock = new PlacedBlock(startTick, runtimeBlock);
        _placedBlocks.Add(placedBlock);
        _blockMap[placedBlock] = runtimeBlock;

        Check_PlaceBlockLength();

        Debug.Log($"[TimelineSystem] 블록 배치: {runtimeBlock.BaseData.BlockName} at T{startTick}-{startTick + length - 1}");
        return true;
    }

    /// <summary>
    /// 배치된 블록 제거
    /// </summary>
    public RuntimeBlock RemovePlacedBlock(PlacedBlock placedBlock)
    {
        if (!_placedBlocks.Contains(placedBlock))
        {
            Debug.LogWarning("[TimelineSystem] 존재하지 않는 PlacedBlock 제거 시도");
            return null;
        }

        // RuntimeBlock 참조 가져오기
        RuntimeBlock runtimeBlock = null;
        if (_blockMap.TryGetValue(placedBlock, out runtimeBlock))
        {
            _blockMap.Remove(placedBlock);
        }

        _placedBlocks.Remove(placedBlock);

        Check_PlaceBlockLength();

        Debug.Log($"[TimelineSystem] 블록 제거: {placedBlock.GetBlockData()?.BlockName}");
        return runtimeBlock;
    }

    /// <summary>
    /// 특정 위치에 블록 배치 가능한지 확인
    /// </summary>
    public bool CanPlaceBlock(int startTick, int length, RuntimeBlock block)
    {
        // 범위 체크
        if (startTick < 1 || startTick + length > 9)
        {
            return false;
        }

        // 겹치는 블록 확인
        for (int tick = startTick; tick < startTick + length; tick++)
        {
            int darg_block_tick = tick - startTick;

            if (IsTickOccupied(tick, darg_block_tick, block))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 특정 틱이 이미 차지되어 있는지 확인
    /// </summary>
    private bool IsTickOccupied(int tick, int darg_block_tick, RuntimeBlock block)
    {
        foreach (PlacedBlock placed in _placedBlocks)
        {
            if (placed.IsActiveAt(tick))
            {
                //겹치기 가능인지 판단
                if (placed.linkedRuntimeBlock.BaseData.actionTypes[placed.GetCardTickIndex(tick)]
                    == block.BaseData.actionTypes[darg_block_tick])
                {
                    Debug.Log($"놓인거 {tick}틱 {placed.linkedRuntimeBlock.BaseData.actionTypes[placed.GetCardTickIndex(tick)].ToString()}");
                    Debug.Log($"들고있는거 {darg_block_tick}틱: {block.BaseData.actionTypes[darg_block_tick].ToString()}");
                    //return false;
                }
                else
                {
                    
                    
                }

                return true;

            }
        }
        return false;
    }

    /// <summary>
    /// 특정 틱에 활성화된 블록들 가져오기
    /// </summary>
    public List<PlacedBlock> GetActiveBlocksAt(int tick)
    {
        List<PlacedBlock> activeBlocks = new List<PlacedBlock>();

        // 잔상과 현재중 실행할 액션 하나만 반환하도록 수정
        PlacedBlock placed = FindFirstAction(tick);
        activeBlocks.Add(placed);

        return activeBlocks;
    }

    /// <summary>
    /// 실행할 액션 선정
    /// </summary>
    public PlacedBlock FindFirstAction(int tick)
    {
        int indexA = -1;
        int first = _placedBlocks.FindIndex(x => x.IsActiveAt(tick));
        int second = _placedBlocks.FindIndex(first + 1, x => x.IsActiveAt(tick));

        if(second != -1)
        {
            indexA = second;
        }
        else
        {
            indexA = first;
        }

        int indexB = -1;
        int first_B = _prevPlacedBlocks.FindIndex(x => x.IsActiveAt(tick));
        int second_B = _prevPlacedBlocks.FindIndex(first_B + 1, x => x.IsActiveAt(tick));

        if (second_B != -1)
        {
            indexB = second_B;
        }
        else
        {
            indexB = first_B;
        }

        //int indexA = _placedBlocks.FindIndex(x => x.IsActiveAt(tick));
        //int indexB = _prevPlacedBlocks.FindIndex(x => x.IsActiveAt(tick));

        bool foundA = indexA != -1;
        bool foundB = indexB != -1;

        // A만 찾음
        if (foundA && !foundB)
        {
            return _placedBlocks[indexA];
        }
            

        // B만 찾음
        if (!foundA && foundB)
        {
            return _prevPlacedBlocks[indexB];
        }
            

        // 둘 다 찾음 → 무조건 listA 우선
        if (foundA && foundB)
        {
            return _placedBlocks[indexA];
        }
            

        // 아무것도 없음
        return null;
    }

    /// <summary>
    /// 특정 틱의 액션 처리 (이벤트 발행만)
    /// </summary>
    public void ProcessTick(int tick)
    {
        PlacedBlock placed = FindFirstAction(tick);

        //Debug.Log(placed.linkedRuntimeBlock.BaseData.blockName);

        if (placed == null) return;

        int cardTickIndex = placed.GetCardTickIndex(tick);
        BlockData blockData = placed.GetBlockData();

        if (blockData == null) return;

        RuntimeBlock runtimeBlock = null;

        // RuntimeBlock 참조 찾기 (잔상 포함)
        if (_blockMap.ContainsKey(placed))
        {
            runtimeBlock = _blockMap[placed];
        }
        else if(_prevblockMap.ContainsKey(placed))
        {
            runtimeBlock = _prevblockMap[placed];
        }

        if (runtimeBlock == null) return;

        // 블록 시작 이벤트
        if (cardTickIndex == 0)
        {
            OnBlockStarted?.Invoke(placed, runtimeBlock, tick);
        }

        ActionType action = blockData.GetEffectAt(cardTickIndex);

        // 틱 이벤트 (키워드용)
        OnBlockTick?.Invoke(placed, runtimeBlock, tick, action);

        // 액션 이벤트 발행
        ExecuteAction(placed, runtimeBlock, action, cardTickIndex, tick);

        // 블록 종료 이벤트
        if (cardTickIndex == blockData.BlockLength - 1)
        {
            OnBlockEnded?.Invoke(placed, runtimeBlock, tick);
        }
    }

    /// <summary>
    /// 액션 실행 (이벤트 발행만)
    /// </summary>
    private void ExecuteAction(PlacedBlock placedBlock, RuntimeBlock runtimeBlock, ActionType action, int cardTickIndex, int currentTick)
    {
        BlockData blockData = placedBlock.GetBlockData();

        switch (action)
        {
            case ActionType.Attack:
                int damage = blockData.AttackDamage;
                Debug.Log($"  → {blockData.BlockName}: 공격 요청 {damage}");

                // 이벤트 발행 (Director가 BattleSystem에 전달)
                OnMeleeAttackRequested?.Invoke(blockData, false);
                break;

            case ActionType.Move:
            case ActionType.Jump:
                MoveDirection dir = runtimeBlock.CurrentMoveDirections[cardTickIndex];
                Debug.Log($"  → {blockData.BlockName}: 이동 요청 ({dir})");

                // 이벤트 발행 (Director가 BattleSystem에 전달)
                OnMoveRequested?.Invoke(dir);
                break;
            case ActionType.Guard:
                OnGuardRequested?.Invoke(true);
                break;
            case ActionType.Bow_single:
                OnLongRangeAttackRequested?.Invoke(blockData.AttackDamage, false);
                break;
            case ActionType.Bow_start:
                int bow_power = 0;

                if (TimelineManager.Instance.Is_POC)
                {
                    bow_power = blockData.AttackDamage;
                }
                Debug.Log($"블럭내 {cardTickIndex}번째의 원거리 공격 액션 발동!! - 딜량: {bow_power}");
                OnLongRangeAttackRequested?.Invoke(bow_power, true);
                break;

            case ActionType.Bow_end:
                OnLongRangeAttackStarted?.Invoke();
                break;

            case ActionType.Bow_middle:
                OnLongRangeAttacking?.Invoke();
                break;
            case ActionType.Sword_end:
                OnMeleeAttackStarted?.Invoke();
                break;
            case ActionType.Sword_start:
                OnMeleeAttackRequested?.Invoke(blockData, true); 
                break;
            case ActionType.Sword_middle:
                break;
            case ActionType.None:
                Debug.Log($"  → {blockData.BlockName}: 대기");
                break;
        }
    }

    /// <summary>
    /// 현재 배치를 이전 배치로 저장 (잔상용)
    /// </summary>
    public void SaveCurrentAsPreview()
    {
        //과거 정보 복사

        if (!TimelineManager.Instance.Is_POC) 
        {
            _prevPlacedBlocks.Clear();
            _prevblockMap.Clear();

            foreach (var p in _placedBlocks)
            {
                PlacedBlock placedBlock = p.Clone();
                _prevPlacedBlocks.Add(placedBlock);
                _prevblockMap.Add(placedBlock, _blockMap[p].Clone());
            }
        }
        
            

        //_prevblockMap = new Dictionary<PlacedBlock, RuntimeBlock>(_blockMap);

        _placedBlocks.Clear();
        _blockMap.Clear();

        Debug.Log("[TimelineSystem] 현재 배치를 잔상으로 저장");
    }

    /// <summary>
    /// 모든 배치 초기화 (전투 시작 시)
    /// </summary>
    public void ClearAll()
    {
        _placedBlocks.Clear();
        _prevPlacedBlocks.Clear();
        _blockMap.Clear();
        _prevblockMap.Clear();

        Debug.Log("[TimelineSystem] 모든 배치 초기화");
    }

    /// <summary>
    /// 모든 배치된 블록 가져오기 (라운드 시작/종료 키워드용)
    /// </summary>
    public List<(PlacedBlock, RuntimeBlock)> GetAllPlacedBlocksWithRuntime()
    {
        List<(PlacedBlock, RuntimeBlock)> result = new List<(PlacedBlock, RuntimeBlock)>();

        foreach (var pair in _blockMap)
        {
            result.Add((pair.Key, pair.Value));
        }

        return result;
    }
}
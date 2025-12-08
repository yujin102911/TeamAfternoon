using UnityEngine;

/// <summary>
/// 타임라인에 배치된 블럭 인스턴스
/// </summary>
public class PlacedBlock
{
    public Saved_BlockData block;             // 블럭 데이터
    public int startTick;                     // 시작 틱 (1~8)
    public RuntimeBlock linkedRuntimeBlock;   // 연결된 실체

    public PlacedBlock(Saved_BlockData c, int start, RuntimeBlock runtime)
    {
        block = c;
        startTick = start;
        linkedRuntimeBlock = runtime;
    }

    /// <summary>
    /// 이 카드의 블럭 데이터 가져오기
    /// </summary>
    public BlockData GetBlockData()
    {
        return DataRepository.Instance.GetBlock(block.Owner_blockID);
    }

    /// <summary>
    /// 이 카드가 특정 틱에 활성화되어 있는지 확인
    /// </summary>
    public bool IsActiveAt(int tick)
    {
        return tick >= startTick && tick < startTick + DataRepository.Instance.GetBlock(block.Owner_blockID).blockLength;
    }

    /// <summary>
    /// 특정 틱에서 카드 내부 인덱스 계산
    /// </summary>
    public int GetCardTickIndex(int tick)
    {
        return tick - startTick;
    }

    public MoveDirection GetDirectionAt(int tick)
    {
        if (linkedRuntimeBlock != null && linkedRuntimeBlock.CurrentMoveDirections != null)
            return linkedRuntimeBlock.CurrentMoveDirections[tick];
        return GetBlockData().moveDirections[tick];
    }

    public void ToggleDirection(int tick)
    {
        int index = tick - startTick;
        linkedRuntimeBlock?.ToggleDirections(index);
    }
}

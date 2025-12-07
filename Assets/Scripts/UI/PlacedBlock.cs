using UnityEngine;

/// <summary>
/// 타임라인에 배치된 블럭 인스턴스
/// </summary>
public class PlacedBlock
{
    public Saved_BlockData block;            // 블럭 데이터
    public int startTick;             // 시작 틱 (1~8)
    //public bool isClockwise;          // 이동 방향 (시계방향 true, 반시계 false)

    public PlacedBlock(Saved_BlockData c, int start, bool clockwise = true)
    {
        block = c;
        startTick = start;
        //isClockwise = clockwise;
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
}

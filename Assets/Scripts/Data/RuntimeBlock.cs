using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 실행 중 저장되어있는 데이터 기반으로 생성하는 런타임 블록 오브젝트
/// </summary>
public class RuntimeBlock
{
    public int BlockID { get; private set; }
    public BlockData BaseData { get; private set; }
    public List<KeywordData> AttachedKeywords { get; private set; } = new List<KeywordData>();

    public MoveDirection[] CurrentMoveDirections { get; private set; }

    public RuntimeBlock(BlockData baseData)
    {
        BlockID = baseData.BlockID;
        BaseData = baseData;

        InitializeDirections();

    }

    public void ApplySavedData(Saved_BlockData saved, DataRepository repo)
    {
        foreach (int keywordId in saved.Attached_Keyword_IDs)
        {
            if (repo.keywordDatas.TryGetValue(keywordId, out KeywordData keyword))
                AttachedKeywords.Add(keyword);
        }
    }

    public void InitializeDirections()
    {
        if (BaseData.MoveDirections != null)
            CurrentMoveDirections = (MoveDirection[])BaseData.MoveDirections.Clone();
    }

    public void ToggleDirections(int index)
    {
        if (CurrentMoveDirections == null || index < 0 || index >= CurrentMoveDirections.Length) return;
        
        if (CurrentMoveDirections[index] == MoveDirection.Left)
            CurrentMoveDirections[index] = MoveDirection.Right;
        else if (CurrentMoveDirections[index] == MoveDirection.Right)
            CurrentMoveDirections[index] = MoveDirection.Left;
    }
}

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

    /// <summary>
    /// 클론 생성
    /// </summary>
    public RuntimeBlock Clone()
    {
        // BaseData는 ScriptableObject라 원본 공유가 일반적으로 맞음(원본 데이터 변경 방지)
        RuntimeBlock copy = new RuntimeBlock(this.BaseData);

        // CurrentMoveDirections 깊은 복사
        if (this.CurrentMoveDirections != null)
            copy.CurrentMoveDirections = (MoveDirection[])this.CurrentMoveDirections.Clone();

        // Keyword 리스트 복사 (KeywordData도 ScriptableObject면 참조복사로 충분)
        copy.AttachedKeywords = new List<KeywordData>(this.AttachedKeywords);

        return copy;
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

    public void ToggleDirectionsForMove(int index)
    {
        if (CurrentMoveDirections == null || index < 0 || index >= CurrentMoveDirections.Length) return;

        switch (CurrentMoveDirections[index])
        {
            case MoveDirection.Front:
                CurrentMoveDirections[index] = MoveDirection.Right;
                break;
            case MoveDirection.Right:
                CurrentMoveDirections[index] = MoveDirection.Back;
                break;
            case MoveDirection.Back:
                CurrentMoveDirections[index] = MoveDirection.Left;
                break;
            case MoveDirection.Left:
                CurrentMoveDirections[index] = MoveDirection.Front;
                break;
            default:
                CurrentMoveDirections[index] = MoveDirection.Front;
                break;
        }
    }
    public void ToggleDirectionsForJump(int index)
    {
        if (CurrentMoveDirections == null || index < 0 || index >= CurrentMoveDirections.Length) return;

        switch (CurrentMoveDirections[index])
        {
            case MoveDirection.DiagonalLu:
                CurrentMoveDirections[index] = MoveDirection.DiagonalRu; break;
            case MoveDirection.DiagonalRu:
                CurrentMoveDirections[index] = MoveDirection.DiagonalRd; break;
            case MoveDirection.DiagonalRd:
                CurrentMoveDirections[index] = MoveDirection.DiagonalLd; break;
            case MoveDirection.DiagonalLd:
                CurrentMoveDirections[index] = MoveDirection.DiagonalLu; break;
            default:
                CurrentMoveDirections[index] = MoveDirection.DiagonalLu;
                break;
        }
    }
}

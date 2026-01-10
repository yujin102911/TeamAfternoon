using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



[CreateAssetMenu(fileName = "New UserGameData", menuName = "Data/User Game Data")]
public class UserGameData : ScriptableObject
{
    [Header("해금 완료된 블럭 정보들")]
    public List<Saved_BlockData> Unlocked_Blocks = new List<Saved_BlockData>();

    [Header("덱의 블럭 ID들")]
    public List<int> Deck_Block_IDs = new List<int>();

    // 추후 덱빌딩 시스템에서 사용하기 용이하게 수정함
    [Header("소지 키워드 정보들")]
    public List<Owned_Keyword_Data> Owned_Keywords = new List<Owned_Keyword_Data>();

    [Header("소지 유물 ID들")]
    public List<int> Owned_Relics_IDs = new List<int>();

    [Header("난이도 설정")]
    public int MaxHP;

    [Header("튜토리얼 클리어 여부")]
    public bool Is_Tutorial_Cleared = false;

    [Header("스테이지 진행도 (클리어된 스테이지 ID 리스트")]
    public List<int> Cleared_Stage_IDs = new List<int>();

    [Header("읽은 메일 정보")]
    public List<string> Read_Mail_Keys = new List<string>();

    [Header("읽은 게시판 정보 (StageID 리스트)")]
    public List<int> Read_Board_Stage_IDs = new List<int>();

    #region Helper Methods
    #region 클립 헬퍼 함수
    public void AddUnlockedBlock(int blockID, List<int> keywordIDs = null)
    {
        if (Unlocked_Blocks.Any(b => b.Owner_blockID == blockID))
        {
            Debug.LogWarning($"Block {blockID}는 이미 해금되어 있습니다");
            return;
        }
        Saved_BlockData newBlock = new Saved_BlockData
        {
            Owner_blockID = blockID,
            Attached_Keyword_IDs = keywordIDs ?? new List<int>(),
            IsFavorite = false,
        };
        Unlocked_Blocks.Add(newBlock);
        Debug.Log($"[UserData] 새로운 블록 해금: {blockID}");
    }
    #endregion

    #region 스테이지 헬퍼 함수
    /// <summary>
    /// 특정 스테이지를 클리어 했는지 확인하는 헬퍼 함수
    /// </summary>
    public bool IsStageCleared(int stageID) => Cleared_Stage_IDs.Contains(stageID);

    /// <summary>
    /// 특정 스테이지를 클리어 처리하는 헬퍼 함수
    /// </summary>
    public void SetStageCleared(int stageID)
    {
        if (!Cleared_Stage_IDs.Contains (stageID)) 
            Cleared_Stage_IDs.Add(stageID);
    }
    #endregion

    #region 메일 헬퍼 함수
    /// <summary>
    /// 메일 읽음 여부 확인하는 헬퍼 함수
    /// </summary>
    public bool IsMailRead(int stageID, int mailIndex)
        => Read_Mail_Keys.Contains($"{stageID}_{mailIndex}");

    /// <summary>
    /// 메일 읽음 처리하는 함수
    /// </summary>
    public void SetMailRead(int stageID, int mailIndex)
    {
        string key = $"{stageID}_{mailIndex}";
        if (!Read_Mail_Keys.Contains (key))
            Read_Mail_Keys .Add (key);
    }
    #endregion

    #region 보드 헬퍼 함수
    public bool IsBoardRead(int stageID) => Read_Board_Stage_IDs .Contains (stageID);

    public void SetBoardRead(int stageID)
    {
        if (!Read_Board_Stage_IDs.Contains(stageID))
            Read_Board_Stage_IDs.Add(stageID);
    }
    #endregion
    #endregion
}

[System.Serializable]
public class Saved_BlockData
{
    public int Owner_blockID;
    public List<int> Attached_Keyword_IDs;
    public bool IsFavorite; // 즐겨찾기 여부 저장 필드
}

[System.Serializable]
public class Owned_Keyword_Data
{
    public int Owned_KeywordID;
    public int Keyword_Num;
}

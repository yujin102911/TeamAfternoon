using System;
using System.Collections.Generic;
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


    // =================게시판 헬퍼 함수 =========
    public bool IsBoardRead(int stageID) => Read_Board_Stage_IDs .Contains (stageID);

    public void SetBoardRead(int stageID)
    {
        if (!Read_Board_Stage_IDs.Contains(stageID))
            Read_Board_Stage_IDs.Add(stageID);
    }
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

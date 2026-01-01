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

    [Header("튜토리얼 클리어 여부")]
    public bool Is_Tutorial_Cleared = false;

    [Header("스테이지 클리어 확인 리스트")]
    public List<bool> Cleared_Stages = new List<bool>();
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

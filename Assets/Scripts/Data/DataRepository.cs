using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "New DataRepository", menuName = "Data/DataRepository")]
public class DataRepository : SerializedScriptableObject   // ★ SerializedScriptableObject!!
{
    [TabGroup("블럭 DB"), TableList(ShowIndexLabels = true)]
    [LabelText("블럭 데이터")]
    public Dictionary<int, BlockData> blockDatas = new();

    [TabGroup("키워드 DB"), TableList(ShowIndexLabels = true)]
    [LabelText("키워드 데이터")]
    public Dictionary<int, KeywordData> keywordDatas = new();

    [TabGroup("유물 DB"), TableList(ShowIndexLabels = true)]
    [LabelText("유물 데이터")]
    public Dictionary<int, RelicData> relicDatas = new();

    [TabGroup("스테이지 DB"), TableList(ShowIndexLabels = true)]
    [LabelText("스테이지 데이터")]
    public Dictionary<int, StageData> stageDatas = new();
}

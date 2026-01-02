using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

[CreateAssetMenu(fileName = "New DataRepository", menuName = "Data/DataRepository")]
public class DataRepository : SerializedScriptableObject   // ★ SerializedScriptableObject!!
{
    private static DataRepository _instance;

    public static DataRepository Instance
    {
        get
        {
            if (_instance == null)
            {
                // Resources 폴더에 있어야 자동 로딩 가능
                _instance = Resources.Load<DataRepository>("DataRepository");

                if (_instance == null)
                    Debug.LogError("❌ Resources/DataRepository.asset 을 찾을 수 없습니다!");
            }

            return _instance;
        }
    }

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

    public BlockData GetBlock(int id)
    {
        return blockDatas.TryGetValue(id, out var data) ? data : null;
    }

    public KeywordData GetKeyword(int id)
    {
        return keywordDatas.TryGetValue(id, out var data) ? data : null;
    }

    public RelicData GetRelic(int id)
    {
        return relicDatas.TryGetValue(id, out var data) ? data : null;
    }

    public StageData GetStage(int id)
    {
        return stageDatas.TryGetValue(id, out var data) ? data : null;
    }
    public bool HasUnreadMail()
    {
        List<StageData> sortedStages = stageDatas.Values
            .OrderBy(s => s.StageNumber)
            .ToList();
        
        for (int i = 0; i < sortedStages.Count; i++)
        {
            StageData stage = sortedStages[i];
            bool isStageAvailable = (i == 0) || sortedStages[i-1].IsCleared;

            if (isStageAvailable)
            {
                foreach (MailContent mail in stage.Mails)
                {
                    bool isMailUnlocked = (mail.unlockCondition == MailContent.MailUnlockCondition.Always) || (mail.unlockCondition == MailContent.MailUnlockCondition.AfterClear && stage.IsCleared);
                    if (isMailUnlocked && !mail.isRead)
                        return true;
                }
            }
            else
            {
                break;
            }
        }
        return false;
    }

}

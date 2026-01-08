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

    // 스테이지 데이터에 있는 메일들을 유저데이터 기준으로 읽었는지 안읽었는지 반환
    // 그때까지의 진행도 포함한
    public bool HasUnreadMail()
    {
        UserGameData currentUser = ServiceLocator.Instance.CurrentUser;
        if (currentUser == null) return false;

        List<StageData> sortedStages = stageDatas.Values
            .OrderBy(s => s.StageNumber)
            .ToList();
        
        for (int i = 0; i < sortedStages.Count; i++)
        {
            StageData stage = sortedStages[i];
            bool isStageAvailable = (i == 0) || currentUser.IsStageCleared(sortedStages[i-1].StageNumber);

            if (isStageAvailable)
            {
                for (int m = 0; m < stage.Mails.Count; m++)
                {
                    MailContent mail = stage.Mails[m];
                    bool isMailUnlocked = (mail.unlockCondition == MailContent.MailUnlockCondition.Always) ||
                        (mail.unlockCondition == MailContent.MailUnlockCondition.AfterClear && currentUser.IsStageCleared(stage.StageNumber));
                    if (isMailUnlocked && !currentUser.IsMailRead(stage.StageNumber, m))
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

    public bool HasUnreadBoard()
    {
        UserGameData currentUser = ServiceLocator.Instance.CurrentUser;
        if (currentUser == null) return false;

        List<StageData> sortedStages = stageDatas.Values
            .OrderBy(s => s.StageNumber)
            .ToList();

        for (int i = 0; i < sortedStages.Count; i++)
        {
            StageData stage = sortedStages[i];
            bool isStageAvailable = (i == 0) || currentUser.IsStageCleared(sortedStages[i - 1].StageNumber);
            if (isStageAvailable)
            {
                if (!currentUser.IsBoardRead(stage.StageNumber))
                    return true;
            }
            else break;
        }
        return false;
    }

}

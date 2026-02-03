using Steamworks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

[System.Serializable]
public class TwitSaveDTO
{
    public List<TwitItem> TwitDatas;
}

public static class TwitSaveService
{
    public const string FILE_NAME = "twitdata.json";
    public static string SavePath => Application.persistentDataPath + "/" + FILE_NAME;
    private static bool IsSteamAvailable => SteamManager.Initialized;

    public static void Save(TwitData data)
    {
        TwitSaveDTO dto = ConvertToDTO(data);
        string json = JsonUtility.ToJson(dto, true);

        File.WriteAllText(SavePath, json);

        if (IsSteamAvailable)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            bool success = SteamRemoteStorage.FileWrite(FILE_NAME, bytes, bytes.Length);
            Debug.Log($"[TwitSaveService] Steam Cloud Save: {success}");
        }
        Debug.Log($"[TwitSaveService] 트윗 정보 저장 완료\n{SavePath}");
    }

    public static void Load(TwitData targetSO)
    {
        string json = "";

        if (IsSteamAvailable && SteamRemoteStorage.FileExists(FILE_NAME))
        {
            int size = SteamRemoteStorage.GetFileSize(FILE_NAME);
            byte[] buffer = new byte[size];
            int read = SteamRemoteStorage.FileRead(FILE_NAME, buffer, size);
            json = Encoding.UTF8.GetString(buffer, 0 , read);
            Debug.Log("[TwitSaveService] Steam Cloud에서 로드 성공");
        }else if (File.Exists(SavePath))
        {
            json = File.ReadAllText(SavePath);
            Debug.Log("[TwitSaveService] 로컬 파일에서 로드 성공");
        }
        else
        {
            Debug.LogWarning("저장 데이터 없음");
            return;
        }
        if (!string.IsNullOrEmpty(json))
        {
            TwitSaveDTO dto = JsonUtility.FromJson<TwitSaveDTO>(json);
            ApplyToSO(dto, targetSO);
        }
    }

    public static void UpdateTwit(TwitData targetSO, int twitID, System.Action<TwitItem> modifyAction)
    {
        TwitItem targetTwit = targetSO.TwitDatas.Find(t => t.TwitID == twitID);
        if (targetTwit != null)
        {
            modifyAction?.Invoke(targetTwit);
            Save(targetSO);
            Debug.Log($"[TwitSaveService] ID {twitID} 트윗 수정 밎 저장 완료");
        }
        else
        {
            Debug.LogError($"[TwitSaveService] ID {twitID}를 가진 트윗을 찾을 수 없습니다.");
        }
    }


    public static void SetRandomReactionsForStage(TwitData targetSO, int stageNum, Difficulty difficulty)
    {
        var targets = targetSO.TwitDatas.Where(t => t.UploadDay == stageNum && t.Difficulty == difficulty).ToList();

        if (targets.Count > 0)
        {
            foreach (var twit in targets)
            {
                twit.RetweetCount = UnityEngine.Random.Range(twit.MinRetweet, twit.MaxRetweet + 1);
                twit.LikeCount = UnityEngine.Random.Range(twit.MinLike, twit.MaxLike + 1);
            }

            Save(targetSO);
            Debug.Log($"[TwitSaveService] Day {stageNum} ({difficulty}) 트윗 {targets.Count}개의 반응 수치 일괄 설정 완료");
        }
    }


    public static void DeleteSave()
    {
        TwitSaveDTO emptyDto = new TwitSaveDTO()
        {
            TwitDatas = new List<TwitItem>(),
        };
        string json = JsonUtility.ToJson(emptyDto, true);

        File.WriteAllText(SavePath, json);

        if (IsSteamAvailable)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            SteamRemoteStorage.FileWrite(FILE_NAME, bytes, bytes.Length);
        }
        Debug.Log("[TwitSaveService] 세이브 데이터가 초기화(덮어쓰기)되었습니당.");
    }


    private static TwitSaveDTO ConvertToDTO(TwitData so)
    {
        return new TwitSaveDTO()
        {
            TwitDatas = so.TwitDatas,
        };
    }

    private static void ApplyToSO(TwitSaveDTO dto, TwitData so)
    {
        so.TwitDatas = dto.TwitDatas;
    }

    public static bool HasSaveData()
    {
        if (IsSteamAvailable && SteamRemoteStorage.FileExists(FILE_NAME)) return true;
        return File.Exists(SavePath);
    }

}

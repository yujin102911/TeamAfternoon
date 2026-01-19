using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Steamworks;
using System.Text;

[System.Serializable]
public class UserSaveDTO
{
    public List<Saved_BlockData> Unlocked_Blocks;
    public List<int> Deck_Block_IDs;
    public List<Owned_Keyword_Data> Owned_Keywords;
    public List<int> Owned_Relics_IDs;
    public bool Is_Tutorial_Cleared;
    public List<int> Cleared_Stage_IDs;
    public List<string> Read_Mail_Keys;
    public List<string> Read_Board_Keys;
    public Difficulty Difficulty;
    public int GameOverCount;
}


public static class SaveService
{
    public const string FILE_NAME = "userdata.json";
    public static string SavePath => Application.persistentDataPath + "/" + FILE_NAME;

    private static bool IsSteamAvailable => SteamManager.Initialized;

    // 프로젝트 어디서든 SaveService.Save(userData); 로 저장 가능
    public static void Save(UserGameData data)
    {
        UserSaveDTO dto = ConvertToDTO(data);
        string json = JsonUtility.ToJson(dto, true);
        // 로컬 저장용
        File.WriteAllText(SavePath, json);

        // steamCloud 저장용
        if (IsSteamAvailable)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            bool success = SteamRemoteStorage.FileWrite(FILE_NAME, bytes, bytes.Length);
            Debug.Log($"[SaveService] Steam Cloud Save: {success}");
        }

        Debug.Log($"[SaveService] 저장 완료\n{SavePath}");
    }

    // Load
    public static void Load(UserGameData targetSO)
    {
        string json = "";

        // steam cloud에서 먼저 시도
        if (IsSteamAvailable && SteamRemoteStorage.FileExists(FILE_NAME))
        {
            int size = SteamRemoteStorage.GetFileSize(FILE_NAME);
            byte[] buffer = new byte[size];
            int read = SteamRemoteStorage.FileRead(FILE_NAME, buffer, size);
            json = Encoding.UTF8.GetString(buffer, 0, read);
            Debug.Log("[SaveService] Steam Cloud에서 로드 성공");
        }
        else if (File.Exists(SavePath))
        {
            json = File.ReadAllText(SavePath);
            Debug.Log("[SaveService] 로컬 파일에서 로드 성공");
        }
        else
        {
            Debug.LogWarning("저장 데이터 없음");
            return;
        }

        if (!string.IsNullOrEmpty(json))
        {
            UserSaveDTO dto = JsonUtility.FromJson<UserSaveDTO>(json);
            ApplyToSO(dto, targetSO);
        }
    }

    // Delete Save
    public static void DeleteSave()
    {
        UserSaveDTO emptyDto = new UserSaveDTO()
        {
            Unlocked_Blocks = new List<Saved_BlockData>(),
            Deck_Block_IDs = new List<int>(),
            Cleared_Stage_IDs = new List<int>(),
            Is_Tutorial_Cleared = false,
            GameOverCount = 0
        };
        string json = JsonUtility.ToJson(emptyDto, true);

        File.WriteAllText(SavePath, json);
        // steam 삭제
        if (IsSteamAvailable)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            SteamRemoteStorage.FileWrite(FILE_NAME, bytes, bytes.Length);
        }

        Debug.Log("[SaveService] 세이브 데이터가 초기화(덮어쓰기)되었습니다.");
    }

    // ======================= 변환 함수 =======================

    private static UserSaveDTO ConvertToDTO(UserGameData so)
    {
        return new UserSaveDTO()
        {
            Unlocked_Blocks = so.Unlocked_Blocks,
            Deck_Block_IDs = so.Deck_Block_IDs,
            Owned_Keywords = so.Owned_Keywords,
            Owned_Relics_IDs = so.Owned_Relics_IDs,
            Is_Tutorial_Cleared = so.Is_Tutorial_Cleared,
            Cleared_Stage_IDs = so.Cleared_Stage_IDs,
            Read_Mail_Keys = so.Read_Mail_Keys,
            Read_Board_Keys = new List<string>(so.Read_Board_Keys),
            Difficulty = so.Difficulty,
            GameOverCount = so.GameOverCount,
        };
    }

    private static void ApplyToSO(UserSaveDTO dto, UserGameData so)
    {
        so.Unlocked_Blocks = dto.Unlocked_Blocks;
        so.Deck_Block_IDs = dto.Deck_Block_IDs;
        so.Owned_Keywords = dto.Owned_Keywords;
        so.Owned_Relics_IDs = dto.Owned_Relics_IDs;
        so.Is_Tutorial_Cleared = dto.Is_Tutorial_Cleared;
        so.Cleared_Stage_IDs = dto.Cleared_Stage_IDs;
        so.Read_Mail_Keys = dto.Read_Mail_Keys;
        so.Read_Board_Keys = dto.Read_Board_Keys;
        so.Difficulty = dto.Difficulty;
        so.GameOverCount = dto.GameOverCount;
    }

    // ============= 검증 함수 ================
    public static bool HasSaveData()
    {
        if (IsSteamAvailable && SteamRemoteStorage.FileExists(FILE_NAME)) return true;
        return File.Exists(SavePath);
    }

    // ============== title용 엿보기 함수~~ ==============
    public static UserSaveDTO PeekSaveData()
    {
        string json = "";
        try
        {
            // 1. Steam Cloud 확인
            if (IsSteamAvailable && SteamRemoteStorage.FileExists(FILE_NAME))
            {
                int size = SteamRemoteStorage.GetFileSize(FILE_NAME);
                byte[] buffer = new byte[size];
                int read = SteamRemoteStorage.FileRead(FILE_NAME, buffer, size);
                json = Encoding.UTF8.GetString(buffer, 0, read);
                Debug.Log("[SaveService] Steam Cloud에서 데이터를 미리 가져옵니다.");
            }
            // 2. Steam에 데이터가 없으면 로컬 파일 확인
            else if (File.Exists(SavePath))
            {
                json = File.ReadAllText(SavePath);
                Debug.Log("[SaveService] 로컬 파일에서 데이터를 미리 가져옵니다.");
            }

            if (string.IsNullOrEmpty(json)) return null;

            return JsonUtility.FromJson<UserSaveDTO>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveService] 데이터 엿보기 실패: {e.Message}");
            return null;
        }
    }
    public static bool CanContinue()
    {
        UserSaveDTO dto = PeekSaveData();

        if (dto == null) return false;

        bool hasProgress = dto.Is_Tutorial_Cleared
            || (dto.Cleared_Stage_IDs != null && dto.Cleared_Stage_IDs.Count > 0);

        return hasProgress;
    }
}

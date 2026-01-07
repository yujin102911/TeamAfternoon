using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
    public int MaxHP;
}


public static class SaveService
{
    public static string SavePath => Application.persistentDataPath + "/userdata.json";


    // 프로젝트 어디서든 SaveService.Save(userData); 로 저장 가능
    public static void Save(UserGameData data)
    {
        UserSaveDTO dto = ConvertToDTO(data);
        string json = JsonUtility.ToJson(dto, true);
        File.WriteAllText(SavePath, json);

        Debug.Log($"[SaveService] 저장 완료\n{SavePath}");
    }

    // Load
    public static void Load(UserGameData targetSO)
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("[SaveService] 저장 파일 없음 → 새 데이터 생성");
            return;
        }

        string json = File.ReadAllText(SavePath);
        UserSaveDTO dto = JsonUtility.FromJson<UserSaveDTO>(json);
        ApplyToSO(dto, targetSO);

        Debug.Log("[SaveService] 로드 완료");
    }

    // Delete Save
    public static void DeleteSave()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
        Debug.Log("[SaveService] 저장 파일 삭제 완료");
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
            MaxHP = so.MaxHP,
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
        so.MaxHP = dto.MaxHP;
    }

    // ============= 검증 함수 ================
    public static bool HasSaveData()
    {
        return File.Exists(SavePath);
    }

    // ============== title용 엿보기 함수~~ ==============
    public static UserSaveDTO PeekSaveData()
    {
        if (!File.Exists(SavePath)) return null;

        try
        {
            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<UserSaveDTO>(json);
        }
        catch(System.Exception e)
        {
            Debug.LogError($"[SaveService] 데이터 미리보기 실패: {e.Message}");
            return null;
        }
    }
}

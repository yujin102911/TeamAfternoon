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
    public List<bool> Cleared_Stages;
}


public static class SaveService
{
    private static string SavePath => Application.persistentDataPath + "/userdata.json";

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
            Cleared_Stages = so.Cleared_Stages
        };
    }

    private static void ApplyToSO(UserSaveDTO dto, UserGameData so)
    {
        so.Unlocked_Blocks = dto.Unlocked_Blocks;
        so.Deck_Block_IDs = dto.Deck_Block_IDs;
        so.Owned_Keywords = dto.Owned_Keywords;
        so.Owned_Relics_IDs = dto.Owned_Relics_IDs;
        so.Is_Tutorial_Cleared = dto.Is_Tutorial_Cleared;
        so.Cleared_Stages = dto.Cleared_Stages;
    }
}

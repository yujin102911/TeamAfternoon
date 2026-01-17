using UnityEngine;
using Steamworks;

[System.Serializable]
public class GlobalSaveDTO
{
    public bool IsGameCleared = false;
}


public static class GlobalSaveService
{
    private const string FILE_NAME = "global.json";
    public static string SavePath => Application.persistentDataPath + "/" + FILE_NAME;

    private static bool IsSteamAvailable => SteamManager.Initialized;


    public static void Save(GlobalSaveDTO data)
    {
        string json = JsonUtility.ToJson(data, true);
        System.IO.File.WriteAllText(SavePath, json);

        if (IsSteamAvailable)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
            bool success = SteamRemoteStorage.FileWrite(FILE_NAME, bytes, bytes.Length);
            Debug.Log($"[GlobalSave] Steam Cloud Save: {success}");
        }
    }

    public static GlobalSaveDTO Load()
    {
        string json = "";
        if (IsSteamAvailable && SteamRemoteStorage.FileExists(FILE_NAME))
        {
            int size = SteamRemoteStorage.GetFileSize(FILE_NAME);
            byte[] buffer = new byte[size];
            SteamRemoteStorage.FileRead(FILE_NAME, buffer, size);
            json = System.Text.Encoding.UTF8.GetString(buffer);
            Debug.Log("[GlobalSave] Loaded from Steam Cloud");
        }
        else if(System.IO.File.Exists(SavePath))
        {
            json = System.IO.File.ReadAllText(SavePath);
            Debug.Log("[GlobalSave] Loaded from Local");
        }
        return string.IsNullOrEmpty(json) ? new GlobalSaveDTO() : JsonUtility.FromJson<GlobalSaveDTO>(json);
    }

}

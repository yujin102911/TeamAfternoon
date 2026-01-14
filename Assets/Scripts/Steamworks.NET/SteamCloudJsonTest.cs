using UnityEngine;
using Sirenix.OdinInspector;
using Steamworks;
using System.Text;

public class SteamCloudJsonTest : MonoBehaviour
{
    private const string FILE_NAME = "save.json";

    [System.Serializable]
    public class SaveData
    {
        public int level;
        public int gold;
        public string playerName;
    }

    [ShowInInspector] public SaveData data = new SaveData { level = 3, gold = 120, playerName = "Player" };

    private void Awake()
    {
        if (!SteamAPI.Init())
        {
            Debug.LogError("SteamAPI.Init failed");
            return;
        }
    }

    private void Update() => SteamAPI.RunCallbacks();
    private void OnDestroy() => SteamAPI.Shutdown();

    [Button("JSON 저장 (Steam Cloud)")]
    public void SaveJsonToCloud()
    {
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        byte[] bytes = Encoding.UTF8.GetBytes(json);

        bool ok = SteamRemoteStorage.FileWrite(FILE_NAME, bytes, bytes.Length);
        Debug.Log($"SaveJsonToCloud => {ok}\n{json}");
    }

    [Button("JSON 로드 (Steam Cloud)")]
    public void LoadJsonFromCloud()
    {
        if (!SteamRemoteStorage.FileExists(FILE_NAME))
        {
            Debug.LogWarning($"Cloud file not found: {FILE_NAME}");
            return;
        }

        int size = SteamRemoteStorage.GetFileSize(FILE_NAME);
        byte[] buffer = new byte[size];

        int read = SteamRemoteStorage.FileRead(FILE_NAME, buffer, size);
        string json = Encoding.UTF8.GetString(buffer, 0, read);

        data = JsonUtility.FromJson<SaveData>(json);
        Debug.Log($"LoadJsonFromCloud ({read} bytes)\n{json}");
    }
}

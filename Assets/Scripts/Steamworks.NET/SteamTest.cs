using UnityEngine;
using Sirenix.OdinInspector;
using Steamworks;

public class SteamAchievementSimpleTest : MonoBehaviour
{
    private CSteamID _mySteamId;
    private const string ACH_ID = "NEW_ACHIEVEMENT_1_0";

    private void Awake()
    {
        if (!SteamAPI.Init())
        {
            Debug.LogError("SteamAPI.Init failed");
            return;
        }
        _mySteamId = SteamUser.GetSteamID();

        SteamUserStats.RequestUserStats(_mySteamId);
    }

    private void Update()
    {
        SteamAPI.RunCallbacks();
    }

    private void OnDestroy()
    {
        SteamAPI.Shutdown();
    }

    // ✅ 업적 달성
    [Button("업적 달성")]
    public void UnlockAchievement()
    {
        SteamUserStats.SetAchievement(ACH_ID);
        SteamUserStats.StoreStats();

        Debug.Log($"Achievement Unlocked: {ACH_ID}");
    }

    // ❌ 업적 초기화 (전체 초기화)
    [Button("업적 초기화")]
    public void ResetAchievements()
    {
        SteamUserStats.ResetAllStats(true);
        SteamUserStats.StoreStats();

        Debug.Log("All achievements reset");
    }
}

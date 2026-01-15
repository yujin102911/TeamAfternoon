using Sirenix.OdinInspector;
using Steamworks;
using System;
using UnityEngine;

public class SteamAchievementSimpleTest : MonoBehaviour
{
    private CSteamID _mySteamId;
    private const string ACH_ID = "NEW_ACHIEVEMENT_1_0";

    public int id;

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

        if (IsInTimeRange(11, 28, 11, 29))
        {
            SteamUserStats.SetAchievement("NEW_ACHIEVEMENT_17_0");
            SteamUserStats.StoreStats();

            Debug.Log($"Achievement Unlocked: {"NEW_ACHIEVEMENT_17_0"}");
        }
    }

    private void OnDestroy()
    {
        SteamAPI.Shutdown();
    }

    // ✅ 업적 달성
    [Button("업적 달성")]
    public void UnlockAchievement()
    {
        if (id < 0) return;

        string Key = "NEW_ACHIEVEMENT_" + id.ToString() + "_0";

        SteamUserStats.SetAchievement(Key);
        SteamUserStats.StoreStats();

        Debug.Log($"Achievement Unlocked: {Key}");
    }

    // ❌ 업적 초기화 (전체 초기화)
    [Button("업적 초기화")]
    public void ResetAchievements()
    {
        SteamUserStats.ResetAllStats(true);
        SteamUserStats.StoreStats();

        Debug.Log("All achievements reset");
    }

    bool IsInTimeRange(int startHour, int startMinute, int endHour, int endMinute)
    {
        DateTime now = DateTime.Now;

        TimeSpan nowTime = now.TimeOfDay;
        TimeSpan start = new TimeSpan(startHour, startMinute, 0);
        TimeSpan end = new TimeSpan(endHour, endMinute, 0);

        // 같은 날 범위 (예: 09:00 ~ 18:00)
        if (start <= end)
            return nowTime >= start && nowTime <= end;

        // 자정 넘어가는 범위 (예: 22:00 ~ 02:00)
        return nowTime >= start || nowTime <= end;
    }
}

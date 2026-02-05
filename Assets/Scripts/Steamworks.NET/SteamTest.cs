using Sirenix.OdinInspector;
using Steamworks;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SteamAchievementSimpleTest : MonoBehaviour
{
    private CSteamID _mySteamId;
    private const string ACH_ID = "NEW_ACHIEVEMENT_1_0";

    public int id;

    private bool _done;

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

    void Start()
    {
        InvokeRepeating(nameof(CheckTimeAchievement), 0f, 1f); // 1초마다
    }


    private void Update()
    {
        //SteamAPI.RunCallbacks();
    }

    private void OnDestroy()
    {
        //SteamAPI.Shutdown();
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
        // 글로벌 시간 기준
        //DateTime now = DateTime.UtcNow;
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

    void CheckTimeAchievement()
    {
        if (_done) return;

        if (IsInTimeRange(14,00,14,01))
        {
            SteamUserStats.SetAchievement("NEW_ACHIEVEMENT_17_0");
            SteamUserStats.StoreStats();
            _done = true;

            Debug.Log($"Achievement Unlocked: NEW_ACHIEVEMENT_17_0");
            CancelInvoke(nameof(CheckTimeAchievement));
        }
    }
}

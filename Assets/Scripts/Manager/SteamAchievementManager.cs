using UnityEngine;
using Steamworks;

public static class SteamAchievementManager
{
    public static void Unlock(string apiName)
    {
        if (!SteamManager.Initialized) return;
        bool success = SteamUserStats.SetAchievement(apiName);
        if (success)
        {
            SteamUserStats.StoreStats();
            Debug.Log($"[Steam] 도전과제 해금 성공: {apiName}");
        }
        else
        {
            Debug.LogError($"[Steam] 도전과제 해금 실패 (Key를 찾을 수 없음): {apiName}");
        }
    }
}

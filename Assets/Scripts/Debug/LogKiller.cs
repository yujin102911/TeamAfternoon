using UnityEngine;

public static class LogKiller
{
    // 게임이 시작되기 "직전"에 실행됨
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void DisableLogs()
    {
#if !UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
        Debug.unityLogger.filterLogType = LogType.Exception;
#endif
    }
}

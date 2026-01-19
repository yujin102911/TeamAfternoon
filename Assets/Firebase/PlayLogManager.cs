using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class PlayLogManager : MonoBehaviour
{
    FirebaseFirestore db;
    private void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            db = FirebaseFirestore.DefaultInstance;
        });
    }

    [Button("로그 테스트용", ButtonSizes.Medium)]
    public void SendPlayLog(float playTimeSeconds)
    {
        DocumentReference docRef = db.Collection("PlayLogs").Document();

        Dictionary<string, object> log = new Dictionary<string, object>
        {
            {"play_time", playTimeSeconds},
            {"version", Application.version},
            {"timestamp", FieldValue.ServerTimestamp}
        };

        docRef.SetAsync(log);
    }

    public void SendStageLog(int stageId, int turns, float stagePlayTime, string status, float totalSessionTime)
    {
        DocumentReference docRef = db.Collection("StageLogs").Document();
        Dictionary<string, object> log = new Dictionary<string, object>
    {
        { "stage_id", stageId },
        { "turns_taken", turns },         
        { "stage_play_time", stagePlayTime },
        { "clear_status", status },         
        { "total_session_time", totalSessionTime }, 
        { "version", Application.version },
        { "timestamp", FieldValue.ServerTimestamp }
    };
        docRef.SetAsync(log).ContinueWithOnMainThread(task => {
            if (task.IsCompleted) Debug.Log($"[Firestore] 스테이지 {stageId} 로그 전송 완료!");
        });
    }


}

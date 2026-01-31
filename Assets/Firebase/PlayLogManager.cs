using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Analytics; // Analytics도 여기서 같이 처리
using Firebase.Extensions;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class PlayLogManager : MonoBehaviour
{
    public static PlayLogManager Instance;

    private FirebaseFirestore db;
    private bool isFirebaseReady = false; // 초기화 완료 여부 체크

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase(); // Awake에서 초기화 시작
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                FirebaseAnalytics.SetSessionTimeoutDuration(new System.TimeSpan(0, 30, 0));

                db = FirebaseFirestore.DefaultInstance;

                isFirebaseReady = true;
                Debug.Log("Firebase (Analytics + Firestore) 초기화 완료!");

                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventAppOpen);
            }
            else
            {
                Debug.LogError($"Firebase 초기화 실패: {dependencyStatus}");
            }
        });
    }

    [Button("로그 테스트용", ButtonSizes.Medium)]
    public void SendPlayLog(float playTimeSeconds)
    {
        if (!isFirebaseReady) return; // 초기화 안됐으면 무시

        DocumentReference docRef = db.Collection("PlayLogs").Document();
        Dictionary<string, object> log = new Dictionary<string, object>
        {
            {"play_time", playTimeSeconds},
            {"version", Application.version},
            {"timestamp", FieldValue.ServerTimestamp}
        };

        docRef.SetAsync(log);
    }

    public void SendStageLog(int stageId, string diff, int turns, float stagePlayTime, string status, float totalSessionTime)
    {
        if (!isFirebaseReady) return;

        DocumentReference docRef = db.Collection("StageLogs").Document();
        Dictionary<string, object> log = new Dictionary<string, object>
        {
            { "stage_id", stageId },
            {"stage_difficulty", diff },
            { "turns_taken", turns },
            { "stage_play_time", stagePlayTime },
            { "clear_status", status },
            { "total_session_time", totalSessionTime },
            { "version", Application.version },
            { "timestamp", FieldValue.ServerTimestamp }
        };

        docRef.SetAsync(log);
    }
}
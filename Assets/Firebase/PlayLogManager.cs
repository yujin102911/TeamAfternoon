using UnityEngine;
using Firebase;
using Firebase.Firestore;
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

    private void OnDestroy()
    {
        db = null;
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;

                isFirebaseReady = true;
                Debug.Log("Firebase Firestore 초기화 완료!");

            }
            else
            {
                Debug.LogError($"Firebase 초기화 실패: {dependencyStatus}");
            }
        });
    }

    public void SendStageLog(int stageId, string diff, int turns, float stagePlayTime, string status, float totalSessionTime)
    {
        if (!isFirebaseReady || db == null) return;

        try
        {
            DocumentReference docRef = db.Collection("StageLogs").Document();
            Dictionary<string, object> log = new Dictionary<string, object>
            {
                { "stage_id", stageId },
                { "stage_difficulty", diff },
                { "turns_taken", turns },
                { "stage_play_time", stagePlayTime },
                { "clear_status", status },
                { "total_session_time", totalSessionTime },
                { "version", Application.version },
                { "timestamp", FieldValue.ServerTimestamp }
            };

            docRef.SetAsync(log).ContinueWithOnMainThread(task => {
                if (task.IsFaulted)
                {
                    Debug.LogWarning("Firestore 기록 실패 (네트워크 혹은 권한 문제)");
                }
            });
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Firestore 호출 중 시스템 오류: {e.Message}");
        }
    }
}
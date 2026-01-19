using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseInitializer : MonoBehaviour
{
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            DependencyStatus dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Debug.Log("Firebase Analytics is ready!");
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

                LogTestEvent();
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    void LogTestEvent()
    {
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventAppOpen);
        Debug.Log("Test event 'app_open' sent.");
    }
}
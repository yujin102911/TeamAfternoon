using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneService
{
    private Stack<string> sceneHistory = new Stack<string>();

    /// <summary> 씬 로드 (동기) </summary>
    public void Load(string sceneName)
    {
        string current = SceneManager.GetActiveScene().name;
        sceneHistory.Push(current);
        SceneManager.LoadScene(sceneName);
        Debug.Log($"[SceneService] Load -> {sceneName} (prev: {current})");
    }

    /// <summary> 이전 씬으로 되돌아가기 </summary>
    public void Back()
    {
        if (sceneHistory.Count == 0)
        {
            Debug.LogWarning("[SceneService] 돌아갈 씬 없음");
            return;
        }

        string prevScene = sceneHistory.Pop();
        SceneManager.LoadScene(prevScene);
        Debug.Log($"[SceneService] Back -> {prevScene}");
    }

    /// <summary> 현재 씬 이름 반환 </summary>
    public string CurrentScene => SceneManager.GetActiveScene().name;

    /// <summary> 씬 히스토리 초기화 </summary>
    public void ClearHistory()
    {
        sceneHistory.Clear();
        Debug.Log("[SceneService] History cleared.");
    }
}

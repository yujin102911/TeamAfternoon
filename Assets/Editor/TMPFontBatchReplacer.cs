// Assets/Editor/TMPFontBatchReplacer.cs
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TMPFontBatchReplacer : EditorWindow
{
    [Header("Replace Rule")]
    [SerializeField] private TMP_FontAsset fromFont; // 이 폰트를 쓰는 TMP만 대상
    [SerializeField] private TMP_FontAsset toFont;   // 이 폰트로 교체
    [SerializeField] private bool setBold = true;    // Bold 적용 여부

    [Header("Scope")]
    [SerializeField] private bool includeOpenScenes = true;
    [SerializeField] private bool includeAllPrefabsInProject = true;

    private Vector2 _scroll;

    [MenuItem("Tools/TMP/Batch Replace Font (From -> To)")]
    public static void Open()
    {
        var w = GetWindow<TMPFontBatchReplacer>("TMP Font Batch Replacer");
        w.minSize = new Vector2(420, 300);
        w.Show();
    }

    private void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Replace Rule", EditorStyles.boldLabel);
        fromFont = (TMP_FontAsset)EditorGUILayout.ObjectField("From Font (Filter)", fromFont, typeof(TMP_FontAsset), false);
        toFont = (TMP_FontAsset)EditorGUILayout.ObjectField("To Font", toFont, typeof(TMP_FontAsset), false);
        setBold = EditorGUILayout.ToggleLeft("Also enable Bold style", setBold);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Scope", EditorStyles.boldLabel);
        includeOpenScenes = EditorGUILayout.ToggleLeft("Process OPEN Scenes", includeOpenScenes);
        includeAllPrefabsInProject = EditorGUILayout.ToggleLeft("Process ALL Prefabs in Project", includeAllPrefabsInProject);

        EditorGUILayout.Space(12);

        using (new EditorGUI.DisabledScope(toFont == null))
        {
            if (GUILayout.Button("RUN (Replace + Save)"))
            {
                Run();
            }
        }

        EditorGUILayout.HelpBox(
            "• From Font is optional. If you leave it empty, ALL TMP texts will be replaced.\n" +
            "• This will modify scenes/prefabs and save them.\n" +
            "• Uses Undo for open scenes; prefab assets are edited via AssetDatabase.\n",
            MessageType.Info);

        EditorGUILayout.EndScrollView();
    }

    private void Run()
    {
        if (toFont == null)
        {
            Debug.LogError("[TMPFontBatchReplacer] To Font is null.");
            return;
        }

        int changedCount = 0;

        try
        {
            AssetDatabase.StartAssetEditing();

            if (includeOpenScenes)
                changedCount += ProcessOpenScenes();

            if (includeAllPrefabsInProject)
                changedCount += ProcessAllPrefabs();

        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"[TMPFontBatchReplacer] Done. Changed TMP components: {changedCount}");
        EditorUtility.DisplayDialog("TMP Font Batch Replacer", $"Done.\nChanged: {changedCount}", "OK");
    }

    // -------------------------
    // Open Scenes
    // -------------------------
    private int ProcessOpenScenes()
    {
        int changed = 0;

        // 현재 열린 씬들 순회
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            bool sceneDirty = false;

            foreach (var root in scene.GetRootGameObjects())
            {
                // TextMeshProUGUI
                var uguis = root.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in uguis)
                    if (TryReplace(t)) { changed++; sceneDirty = true; }

                // TextMeshPro (World)
                var worlds = root.GetComponentsInChildren<TextMeshPro>(true);
                foreach (var t in worlds)
                    if (TryReplace(t)) { changed++; sceneDirty = true; }
            }

            if (sceneDirty)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        return changed;
    }

    // -------------------------
    // Prefab Assets
    // -------------------------
    private int ProcessAllPrefabs()
    {
        int changed = 0;

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (var guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
            if (prefabRoot == null) continue;

            bool prefabDirty = false;

            var uguis = prefabRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in uguis)
                if (TryReplace(t, recordUndo: false)) { changed++; prefabDirty = true; }

            var worlds = prefabRoot.GetComponentsInChildren<TextMeshPro>(true);
            foreach (var t in worlds)
                if (TryReplace(t, recordUndo: false)) { changed++; prefabDirty = true; }

            if (prefabDirty)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        return changed;
    }

    // -------------------------
    // Core Replace Logic
    // -------------------------
    private bool TryReplace(TMP_Text text, bool recordUndo = true)
    {
        if (text == null) return false;

        // fromFont 지정되어 있으면 "그 폰트인 경우만"
        if (fromFont != null && text.font != fromFont)
            return false;

        bool changed = false;

        if (recordUndo)
            Undo.RecordObject(text, "TMP Font Replace");

        if (text.font != toFont)
        {
            text.font = toFont;
            changed = true;
        }

        if (setBold)
        {
            // Bold 플래그 켜기 (기존 스타일 유지하면서 Bold만 추가)
            // TMP FontStyles는 비트플래그 느낌으로 사용 가능
            var before = text.fontStyle;
            var after = before | FontStyles.Bold;
            if (before != after)
            {
                text.fontStyle = after;
                changed = true;
            }
        }

        if (changed)
        {
            EditorUtility.SetDirty(text);
        }

        return changed;
    }
}

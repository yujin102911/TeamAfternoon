using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// BlockData에서 부분 블록들을 자동 생성하고 Full 블록에 자동 연결
/// </summary>
public class PartialBlockGenerator : Editor
{
    [MenuItem("Tools/Block/Generate Partial Blocks from Selected")]
    static void GeneratePartialBlocksFromSelected()
    {
        // 선택된 BlockData 가져오기
        BlockData original = Selection.activeObject as BlockData;
        
        if (original == null)
        {
            EditorUtility.DisplayDialog("오류", "BlockData를 선택해주세요!", "확인");
            return;
        }

        // 확인 대화상자
        bool confirm = EditorUtility.DisplayDialog(
            "부분 블록 생성",
            $"'{original.blockName}'의 부분 블록 {original.blockLength}개를 생성하시겠습니까?\n\n생성 후 Full 블록에 자동으로 연결됩니다.",
            "생성",
            "취소"
        );

        if (!confirm) return;

        // 저장 경로 설정 (원본과 같은 폴더)
        string originalPath = AssetDatabase.GetAssetPath(original);
        string directory = Path.GetDirectoryName(originalPath);

        List<BlockData> createdBlocks = new List<BlockData>();
        int createdCount = 0;

        // 각 부분 블록 생성
        for (int startIndex = 0; startIndex < original.blockLength; startIndex++)
        {
            // 부분 블록 생성
            BlockData partial = ScriptableObject.CreateInstance<BlockData>();
            
            // 이름 설정
            if (startIndex == 0)
                partial.blockName = $"{original.blockName}_Full";
            else
                partial.blockName = $"{original.blockName}_Part{startIndex + 1}";

            // 길이 계산
            int partialLength = original.blockLength - startIndex;
            partial.blockLength = partialLength;

            // BlockID (원본 + 인덱스 오프셋)
            partial.blockID = original.blockID + startIndex;

            // 액션 타입 복사
            partial.actionTypes = new ActionType[partialLength];
            for (int i = 0; i < partialLength; i++)
            {
                partial.actionTypes[i] = original.actionTypes[startIndex + i];
            }

            // 이동 방향 복사
            partial.moveDirections = new MoveDirection[partialLength];
            for (int i = 0; i < partialLength; i++)
            {
                partial.moveDirections[i] = original.moveDirections[startIndex + i];
            }

            // 공격 데미지 복사
            partial.attackDamage = original.attackDamage;

            // 파일 저장
            string fileName = $"{partial.blockName}.asset";
            string savePath = Path.Combine(directory, fileName);

            // 중복 파일 확인
            if (File.Exists(savePath))
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "파일 존재",
                    $"'{fileName}' 파일이 이미 존재합니다. 덮어쓰시겠습니까?",
                    "덮어쓰기",
                    "건너뛰기"
                );

                if (!overwrite)
                {
                    Debug.Log($"[PartialBlockGenerator] {fileName} 생성 건너뜀");
                    continue;
                }

                // 기존 에셋 삭제
                AssetDatabase.DeleteAsset(savePath);
            }

            // 에셋 생성
            AssetDatabase.CreateAsset(partial, savePath);
            createdBlocks.Add(partial);
            createdCount++;

            Debug.Log($"[PartialBlockGenerator] {partial.blockName} 생성 완료 ({partialLength}칸)");
        }

        // 저장 및 새로고침
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ⭐ Full 블록에 부분 블록 배열 자동 연결
        if (createdBlocks.Count > 0)
        {
            BlockData fullBlock = createdBlocks[0]; // 첫 번째가 Full
            fullBlock.partialBlockDatas = createdBlocks.ToArray();
            EditorUtility.SetDirty(fullBlock);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"[PartialBlockGenerator] {fullBlock.blockName}에 부분 블록 {createdBlocks.Count}개 자동 연결 완료!");
        }

        // 결과 메시지
        EditorUtility.DisplayDialog(
            "생성 완료",
            $"'{original.blockName}'의 부분 블록 {createdCount}개가 생성되었습니다!\n\n저장 위치: {directory}\n\n✅ Full 블록에 자동으로 연결되었습니다.",
            "확인"
        );

        Debug.Log($"[PartialBlockGenerator] 총 {createdCount}개의 부분 블록 생성 완료!");
    }

    /// <summary>
    /// 선택된 여러 BlockData에서 일괄 생성
    /// </summary>
    [MenuItem("Tools/Block/Generate Partial Blocks (Batch)")]
    static void GeneratePartialBlocksBatch()
    {
        // 선택된 모든 BlockData 가져오기
        Object[] selectedObjects = Selection.objects;
        
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("오류", "BlockData를 하나 이상 선택해주세요!", "확인");
            return;
        }

        // 확인 대화상자
        bool confirm = EditorUtility.DisplayDialog(
            "일괄 부분 블록 생성",
            $"선택된 {selectedObjects.Length}개의 BlockData에서 부분 블록을 생성하시겠습니까?\n\n각 Full 블록에 자동으로 연결됩니다.",
            "생성",
            "취소"
        );

        if (!confirm) return;

        int totalCreated = 0;
        int processedCount = 0;

        foreach (Object obj in selectedObjects)
        {
            BlockData blockData = obj as BlockData;
            if (blockData == null) continue;

            processedCount++;

            // 프로그레스 바 표시
            float progress = (float)processedCount / selectedObjects.Length;
            EditorUtility.DisplayProgressBar(
                "부분 블록 생성 중...",
                $"{blockData.blockName} 처리 중... ({processedCount}/{selectedObjects.Length})",
                progress
            );

            // 저장 경로 설정
            string originalPath = AssetDatabase.GetAssetPath(blockData);
            string directory = Path.GetDirectoryName(originalPath);

            List<BlockData> createdBlocks = new List<BlockData>();

            // 각 부분 블록 생성
            for (int startIndex = 0; startIndex < blockData.blockLength; startIndex++)
            {
                BlockData partial = ScriptableObject.CreateInstance<BlockData>();
                
                if (startIndex == 0)
                    partial.blockName = $"{blockData.blockName}_Full";
                else
                    partial.blockName = $"{blockData.blockName}_Part{startIndex + 1}";

                int partialLength = blockData.blockLength - startIndex;
                partial.blockLength = partialLength;
                partial.blockID = blockData.blockID + startIndex;

                partial.actionTypes = new ActionType[partialLength];
                for (int i = 0; i < partialLength; i++)
                    partial.actionTypes[i] = blockData.actionTypes[startIndex + i];

                partial.moveDirections = new MoveDirection[partialLength];
                for (int i = 0; i < partialLength; i++)
                    partial.moveDirections[i] = blockData.moveDirections[startIndex + i];

                partial.attackDamage = blockData.attackDamage;

                string fileName = $"{partial.blockName}.asset";
                string savePath = Path.Combine(directory, fileName);

                // 기존 파일 있으면 덮어쓰기
                if (File.Exists(savePath))
                    AssetDatabase.DeleteAsset(savePath);

                AssetDatabase.CreateAsset(partial, savePath);
                createdBlocks.Add(partial);
                totalCreated++;
            }

            // ⭐ Full 블록에 부분 블록 배열 자동 연결
            if (createdBlocks.Count > 0)
            {
                BlockData fullBlock = createdBlocks[0];
                fullBlock.partialBlockDatas = createdBlocks.ToArray();
                EditorUtility.SetDirty(fullBlock);
            }
        }

        // 프로그레스 바 닫기
        EditorUtility.ClearProgressBar();

        // 저장 및 새로고침
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 결과 메시지
        EditorUtility.DisplayDialog(
            "일괄 생성 완료",
            $"총 {processedCount}개의 BlockData에서\n{totalCreated}개의 부분 블록이 생성되었습니다!\n\n✅ 각 Full 블록에 자동으로 연결되었습니다.",
            "확인"
        );

        Debug.Log($"[PartialBlockGenerator] 일괄 생성 완료: {processedCount}개 처리, {totalCreated}개 생성");
    }

    /// <summary>
    /// 메뉴 활성화 조건 (BlockData 선택 시에만)
    /// </summary>
    [MenuItem("Tools/Block/Generate Partial Blocks from Selected", true)]
    [MenuItem("Tools/Block/Generate Partial Blocks (Batch)", true)]
    static bool ValidateSelection()
    {
        return Selection.activeObject is BlockData || 
               (Selection.objects.Length > 0 && Selection.objects[0] is BlockData);
    }
}

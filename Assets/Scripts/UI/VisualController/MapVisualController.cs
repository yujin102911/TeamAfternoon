using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 맵 비주얼을 담당하는 스크립트
/// GameManager가 가지고있음
/// </summary>
public class MapVisualController : MonoBehaviour
{
    private MapSystem _mapSystem;

    /// <summary>
    /// 외부에서 MapSystem을 꽂아주는 함수
    /// GameManager가 호출
    /// </summary>
    public void Initialize(MapSystem mapSystem)
    {
        _mapSystem = mapSystem;
    }

    /// <summary>
    /// 적 공격 시 비주얼
    /// </summary>
    public void OnEnemyAttackVisual(List<int> sectors)
    {
        StartCoroutine(FlashAttackSectorsRoutine(sectors));
    }

    /// <summary>
    /// 마우스 오버 시 공격 위치 비주얼
    /// </summary>
    public void OnRequestHighlight(List<int> sectors)
    {
        if (_mapSystem != null)
        {
            _mapSystem.HighlightAttackSectors(sectors);
        }
    }

    /// <summary>
    /// 마우스 Exit 시 공격 비주얼 있던거 클리어
    /// </summary>
    public void OnRequestClearHighlight()
    {
        if (_mapSystem != null)
        {
            _mapSystem.ResetHighlight();
        }
    }

    private IEnumerator FlashAttackSectorsRoutine(List<int> sectors)
    {
        if (_mapSystem == null) yield break;

        for (int i = 0; i < 2; i++)
        {
            // 1. 빨간색 켜기
            _mapSystem.HighlightAttackSectors(sectors);
            yield return new WaitForSeconds(0.1f); // 0.1초 동안 켜짐

            // 2. 끄기 (원래 색으로)
            _mapSystem.ResetHighlight();
            yield return new WaitForSeconds(0.1f);  // 0.1초 동안 꺼짐 (간격)
        }
    }

}

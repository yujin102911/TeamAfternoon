using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// BattleSystem 관찰
/// MapSystem에게 명령
/// </summary>
public class MapVisualController : MonoBehaviour
{
    [SerializeField] private UIEffectPool effectPool;

    private MapSystem _mapSystem;
    private BattleSystem _battleSystem;
    private MapConfiguration _mapConfig;

    #region Initialize Methods
    /// <summary>
    /// 외부에서 MapSystem을 꽂아주는 함수
    /// GameManager가 호출
    /// </summary>
    public void Initialize(MapSystem mapSystem, BattleSystem battleSystem, MapConfiguration mapConfig)
    {
        _mapSystem = mapSystem;
        _battleSystem = battleSystem;
        _mapConfig = mapConfig;
        _battleSystem.OnObjectiveUpdated += HandleObjectiveUpdated;
    }
    void OnDestroy()
    {
        if (_battleSystem != null)
            _battleSystem.OnObjectiveUpdated -= HandleObjectiveUpdated;
    }

    /// <summary>
    /// 현재 전투 상황에 맞게 섹터 기본 색상 갱신
    /// 전투 시작, 턴 종료, 적 사망 시 호출
    /// </summary>
    public void RefreshMapOwnershipVisuals()
    {
        if (_battleSystem == null || _mapSystem == null) return;
        for (int i = 1; i <= _mapSystem.TotalSectors; i++)
        {
            RuntimeEnemy owner = _battleSystem.GetEnemyAtSector(i);
            Color targetColor = (owner != null) ? owner.Data.AssignedColor : GetNormalColor();

            // 목표 횟수 가져오기
            int remaining = _battleSystem.GetRemainingPassCount(i);

            if (remaining > 0)
            {
                // [수정] 목표가 남아있으면 색상 변경 + 숫자 표시
                targetColor = Color.yellow; // 노란색으로 표시
                _mapSystem.SetSectorText(i, remaining.ToString()); // 숫자 표시!
            }
            else
            {
                // [수정] 목표가 없거나 달성했으면 텍스트 지우기
                _mapSystem.SetSectorText(i, "");
            }

            _mapSystem.SetSectorBaseColor(i, targetColor);

            // 정화 모드 로직
            if (TimelineManager.Instance.Is_Cure)
            {
                targetColor = _battleSystem.AbleCureSectors.Contains(i) ? _mapConfig.cureColor : GetNormalColor();
                _mapSystem.SetSectorBaseColor(i, targetColor);
            }
        }
    }
    #endregion

    #region Mouse hover Visual Methods

    /// <summary>
    /// 마우스 오버 시 공격 위치 비주얼
    /// </summary>
    public void OnRequestHighlight(List<int> sectors)
    {
        if (_mapSystem == null) return;
        Color hightlightColor = GetAttackColor();
        foreach (int index in sectors)
        {
            _mapSystem.SetSectorTempColor(index, hightlightColor);
        }
    }

    /// <summary>
    /// 마우스 Exit 시 공격 비주얼 있던거 클리어
    /// </summary>
    public void OnRequestClearHighlight()
    {
        if (_mapSystem == null) return;
        for (int i = 1; i <= _mapSystem.TotalSectors; i++)
        {
            _mapSystem.ResetSectorColor(i);
        }
    }
    #endregion

    #region Enemy Attack Visual Methods
    /// <summary>
    /// 적 공격 시 비주얼
    /// </summary>
    public void OnEnemyAttackVisual(List<int> sectors)
    {
        StartCoroutine(FlashAttackRoutine(sectors));
    }
    private IEnumerator FlashAttackRoutine(List<int> sectors)
    {
        if (_mapSystem == null) yield break;
        if (SoundManager.Instance != null)
            SoundManager.Instance.Play(SoundID.SFX_Lightning);

        Color flashColor = GetAttackColor();
        for (int i = 0; i < 2; i++)
        {
            // 빨간색 켜기
            foreach (int index in sectors)
            {
                _mapSystem.SetSectorTempColor(index, flashColor);
                if (i == 0)
                {
                    _mapSystem.PlaySectorParticle(index);

                    Vector2 spawn_pos = _mapSystem.GetSector_Pos_ToScreen(index);
                    Play_LightingEffect(spawn_pos);
                }
            }
            yield return new WaitForSeconds(0.1f);
            // 빨간색 끄기
            foreach (int index in sectors)
                _mapSystem.ResetSectorColor(index);
            yield return new WaitForSeconds(0.1f);
        }

        
    }

    public void Play_LightingEffect(Vector2 uiPosition)
    {
        UIPooledEffect effect = effectPool.Get();
        effect.Play(uiPosition);
    }
    #endregion

    #region Objective Methods
    private void HandleObjectiveUpdated(int sector, int count)
    {
        RefreshMapOwnershipVisuals();
    }
    #endregion

    #region Helper Methods
    private Color GetAttackColor()
    {
        return _mapConfig != null ? _mapConfig.attackColor : Color.red;
    }
    private Color GetNormalColor()
    {
        return _mapConfig != null ? _mapConfig.normalColor : Color.white;
    }
    #endregion

}

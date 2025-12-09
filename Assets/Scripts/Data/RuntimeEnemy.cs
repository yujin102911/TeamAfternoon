using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 전투 중 실제 존재하는 적 객체
/// </summary>
[System.Serializable]
public class RuntimeEnemy
{
    public EnemyData Data { get; private set; }

    // 전투 상태
    public int CurrentHP { get; private set; }
    public int MaxHP { get; private set; }
    public bool IsDead => CurrentHP <= 0;

    public List<int> AttackableSectors { get; private set; }

    public EnemyPattern CurrentPattern { get; private set; }

    private int _currentPhaseIndex = 0;      // (_currentPhaseIndex + 1) 페이즈
    private int _patternSequenceIndex = 0;   // 현재 리스트의 몇 번째 패턴인지

    public RuntimeEnemy(EnemyData data, List<int> attackableSectors)
    {
        Data = data;
        MaxHP = data.Max_EnemyHp;
        CurrentHP = MaxHP;

        AttackableSectors = attackableSectors;

        _currentPhaseIndex = 0;
        _patternSequenceIndex = 0;
    }

    /// <summary>
    /// 데미지 처리 함수
    /// </summary>
    public void TakeDamage(int damage)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - damage);
    }

    /// <summary>
    /// 공격 판정 확인
    /// </summary>
    public bool IsHitByAttackFrom (int playerSector)
    {
        if (IsDead) return false;
        return AttackableSectors.Contains(playerSector);
    }

    /// <summary>
    /// 이번 라운드 패턴 할당
    /// </summary>
    public void SetPattern(EnemyPattern pattern)
    {
        CurrentPattern = pattern;
    }

    /// <summary>
    /// 외부에서 페이즈를 강제로 변경할 때 호출
    /// </summary>
    public void ForceChangePhase(int newPhaseIndex)
    {
        if (newPhaseIndex <= _currentPhaseIndex) return;
        if (newPhaseIndex >= Data.PhaseGroups.Count) return;

        _currentPhaseIndex = newPhaseIndex;
        _patternSequenceIndex = 0;
        Debug.Log($"[{Data.Enemy_Name}] 페이즈 {_currentPhaseIndex + 1}로 전환!");
    }

    public EnemyPattern GetNextPattern()
    {
        if (Data.PhaseGroups.Count == 0) return null;

        EnemyPhaseGroup currentPhaseGroup = Data.PhaseGroups[_currentPhaseIndex];

        if (currentPhaseGroup.Patterns.Count == 0) return null;

        if (_patternSequenceIndex >= currentPhaseGroup.Patterns.Count)
        {
            if (currentPhaseGroup.IsLoop)
                _patternSequenceIndex = 0;
            else
            {
                Debug.Log($"[{Data.Enemy_Name}] 현재 페이즈 패턴 고갈! 3페이즈(마지막)로 강제 진입");
                _currentPhaseIndex = Data.PhaseGroups.Count - 1;
                _patternSequenceIndex = 0;

                return GetNextPattern();
            }
        }
        EnemyPattern pattern = currentPhaseGroup.Patterns[_patternSequenceIndex];
        _patternSequenceIndex++;
        return pattern;
    }

}

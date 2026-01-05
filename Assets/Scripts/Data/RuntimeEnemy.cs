using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 전투 중 실제 존재하는 적 객체
/// </summary>
[System.Serializable]
public class RuntimeEnemy
{
    public EnemyData Data { get; private set; }

    public bool IsLeft { get; private set; } = false; // 기본값 : 오른쪽

    public List<int> AttackableSectors { get; private set; }

    public EnemyPattern CurrentPattern { get; private set; }

    public int PatternSequenceIndex => _patternSequenceIndex;

    private int _patternSequenceIndex = 0;   // 현재 리스트의 몇 번째 패턴인지

    public RuntimeEnemy(EnemyData data, List<int> attackableSectors)
    {
        Data = data;

        AttackableSectors = attackableSectors;
        _patternSequenceIndex = 0;
    }

    /// <summary>
    /// 공격 판정 확인 (적의 위치에 따라 플레이어의 열 확인_)
    /// </summary>
    public bool IsHitByAttackFrom (int playerSector, int columns)
    {
        int playerCol = (playerSector - 1) % columns;

        if (IsLeft)
        {
            return playerCol == 0;
        }
        else
        {
            return playerCol == columns - 1;
        }
    }

    public void ToggleSide()
    {
        IsLeft = !IsLeft;
        Debug.Log($"[{Data.Enemy_Name}] 위치 변경: {(IsLeft ? "왼쪽" : "오른쪽")}");
    }

    /// <summary>
    /// 이번 라운드 패턴 할당
    /// </summary>
    public void SetPattern(EnemyPattern pattern)
    {
        CurrentPattern = pattern;
        CurrentPattern.Set_Is_left(IsLeft);
    }

    public void AddHitSectors(List<int> newSectors)
    {
        if (newSectors == null) return;
        foreach (int sector in newSectors)
        {
            if (!AttackableSectors.Contains(sector))
            {
                AttackableSectors.Add(sector);
            }
        }
        AttackableSectors.Sort();
        Debug.Log($"[{Data.Enemy_Name}] 피격 범위 확장됨! 현재 범위: {string.Join(", ", AttackableSectors)}");
    }

    public EnemyPattern GetNextPattern()
    {
        if (Data.Patterns == null || Data.Patterns.Count == 0) return null;
        if (_patternSequenceIndex >= Data.Patterns.Count)
            _patternSequenceIndex = 0;
        EnemyPattern pattern = Data.Patterns[ _patternSequenceIndex ];
        _patternSequenceIndex++;

        return pattern;
    }

    public EnemyPattern GetRandomPattern()
    {
        if (Data.Patterns == null || Data.Patterns.Count == 0) return null;
        if (Data.Patterns.Count == 1)
        {
            _patternSequenceIndex = 0;
            CurrentPattern = Data.Patterns[0];
            return CurrentPattern;
        }

        int nextIndex = _patternSequenceIndex;
        while (nextIndex == _patternSequenceIndex)
        {
            nextIndex = Random.Range(0, Data.Patterns.Count);
        }

        _patternSequenceIndex = nextIndex;
        CurrentPattern = Data.Patterns[_patternSequenceIndex];

        Debug.Log($"{Data.Patterns.Count}개의 패턴 중 {_patternSequenceIndex}번째 패턴");
        return CurrentPattern;
    }

}

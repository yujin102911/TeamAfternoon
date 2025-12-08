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

    // 이 적이 점유할 Tick (startTick: 1, endTick:7 -> 1~7틱까지 내꺼)
    public int StartTick { get; private set; }
    public int EndTick { get; private set; }

    public List<int> AttackableSectors { get; private set; }

    public EnemyPattern CurrentPattern { get; private set; }

    public RuntimeEnemy(EnemyData data, List<int> attackableSectors, int startTick, int endTick)
    {
        Data = data;
        MaxHP = data.Max_EnemyHp;
        CurrentHP = MaxHP;

        AttackableSectors = attackableSectors;
        StartTick = startTick;
        EndTick = endTick;
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

}

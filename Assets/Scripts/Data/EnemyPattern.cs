using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적의 한 틱 공격 정보
/// </summary>
[System.Serializable]
public class EnemyAttack
{
    public int tick;                  // 공격 틱 (1~18)
    public List<int> targetSectors;   // 공격 대상 섹터들
    public int damage;                  // 피해 주사위

    public EnemyAttack(int t, List<int> sectors, int dmg)
    {
        tick = t;
        targetSectors = new List<int>(sectors);
        damage = dmg;
    }
}

[System.Serializable]
public class EnemyParrying
{
    public int tick;
    // public float probability = 1f;  // 패링 발동 확률 - 지금은 사실상 사용하는 부분 없음. 추후 확장 가능성을 위해 일단 만들어둠
    public float damageMultiplier = 1f; // 패링 시 데미지

    public EnemyParrying(int t, float mul)
    {
        tick = t;
        // probability = pro;
        damageMultiplier = mul;
    }
}

[System.Serializable]
public class EnemyStone
{
    public int tick;

    public EnemyStone(int t)
    {
        tick = t;
    }

}

// ========================================
// 적 시퀀스 (한 라운드 8틱 패턴)
// ========================================
[CreateAssetMenu(fileName = "New Enemy Pattern", menuName = "Data/Enemy/Enemy Pattern")]
public class EnemyPattern : ScriptableObject
{
    public string Pattern_Name;

    [Header("패턴 대사")]
    [TextArea(2, 5)]
    public string Sentence;


    public List<EnemyAttack> attacks = new List<EnemyAttack>();
    public List<EnemyStone> stones = new List<EnemyStone>();
    public List<EnemyParrying> parryings = new List<EnemyParrying>();

    /// <summary>
    /// 특정 틱의 적 공격 가져오기
    /// </summary>
    public EnemyAttack GetAttackAt(int tick)
    {
        return attacks.Find(a => a.tick == tick);
    }

    /// <summary>
    /// 특정 틱의 적 패링 가져오기
    /// </summary>
    public EnemyParrying GetParryingAt(int tick)
    {
        return parryings.Find(a => a.tick == tick);
    }

    /// <summary>
    /// 특정 틱의 바위 던지기 행동 가져오기
    /// </summary>
    public EnemyStone GetStoneAt(int tick)
    {
        return stones.Find(a => a.tick == tick);
    }
}

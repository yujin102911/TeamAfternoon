using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;

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
    public int tick = 8;
    public int count = 1;

}


public enum WindDirection
{
    Up,
    Down,
    Left,
    Right,
}
public enum WindType
{
    Forward,    // 바람으로 밀기
    BackWard,   // 바람으로 당기기
}
[System.Serializable]
public class EnemyWind
{
    public int tick;
    public WindType windType;

    public EnemyWind(int t, WindType type)
    {
        tick = t;
        windType = type;
    }

    public WindDirection GetDynamicDirection(bool isEnemyLeft)
    {
        switch (windType)
        {
            case WindType.Forward:
            default:
                return isEnemyLeft ? WindDirection.Right : WindDirection.Left;
            case WindType.BackWard:
                return isEnemyLeft ? WindDirection.Left : WindDirection.Right;
        }
    }
}

[Serializable]
public class EnemyDash
{
    public int tick;
    public List<int> targetRows;
    public int damage;

    public List<int> Convert_9sector()
    {
        List<int> sectors = new List<int>();

        for(int i = 0; i < targetRows.Count; i++)
        {
            int row = targetRows[i];
            switch(row)
            {
                case 0:
                    sectors.Add(1);
                    sectors.Add(2);
                    sectors.Add(3);
                    break;
                case 1:
                    sectors.Add(4);
                    sectors.Add(5);
                    sectors.Add(6);
                    break;
                case 2:
                    sectors.Add(7);
                    sectors.Add(8);
                    sectors.Add(9);
                    break;
            }
        }

        return sectors;
    }
}

// ========================================
// 적 시퀀스 (한 라운드 8틱 패턴)
// ========================================
[CreateAssetMenu(fileName = "New Enemy Pattern", menuName = "Data/Enemy/Enemy Pattern")]
[InfoBox("중복된 틱이 감지되었습니다! 한 틱에는 하나의 행동만 설정할 수 있습니다.\n중복된 틱: @$value.DuplicateTicksString",
    InfoMessageType.Error, "@$value.HasOverlappingTicks()")]
public class EnemyPattern : ScriptableObject
{
    public string Pattern_Name;

    public List<EnemyAttack> attacks = new List<EnemyAttack>();
    public List<EnemyStone> stones = new List<EnemyStone>();
    public List<EnemyWind> winds = new List<EnemyWind>();
    public List<EnemyDash> dashes = new List<EnemyDash>();
    public List<EnemyParrying> parryings = new List<EnemyParrying>();

    private bool Is_left = false;

    public void Set_Is_left(bool left)
    {
        Is_left = left;
    }

    public bool Get_Is_left()
    {
        return Is_left;
    }

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

    /// <summary>
    /// 특정 틱의 바람 풍 가져오기
    /// </summary>
    public EnemyWind GetWindAt(int tick)
    {
        return winds.Find(a => a.tick == tick);
    }

    /// <summary>
    /// 특정 틱의 돌진 공격 가져오기
    /// </summary>
    public EnemyDash GetDashAt(int tick)
    {
        return dashes.Find(a => a.tick == tick);
    }

    #region Odin 유효성 검사 로직
    /// <summary>
    /// 중복된 틱이 있는지 확인하는 함수 (InfoBox 표시 조건)
    /// </summary>
    private bool HasOverlappingTicks()
    {
        return GetDuplicateTicks().Any();
    }

    /// <summary>
    /// 중복된 틱 번호들을 문자열로 반환 (InfoBox 메시지용)
    /// </summary>
    private string DuplicateTicksString => string.Join(", ", GetDuplicateTicks());

    /// <summary>
    /// 모든 리스트의 틱을 수집하여 중복된 번호 리스트를 반환
    /// </summary>
    private List<int> GetDuplicateTicks()
    {
        List<int> allTicks = new List<int>();

        // 모든 리스트의 틱 정보를 수집
        if (attacks != null) allTicks.AddRange(attacks.Select(a => a.tick));
        if (stones != null) allTicks.AddRange(stones.Select(s => s.tick));
        if (winds != null) allTicks.AddRange(winds.Select(w => w.tick));
        if (dashes != null) allTicks.AddRange(dashes.Select(d => d.tick));
        if (parryings != null) allTicks.AddRange(parryings.Select(p => p.tick));

        // 1개 이상 존재하는 틱 번호만 추출
        return allTicks.GroupBy(t => t)
                       .Where(g => g.Count() > 1)
                       .Select(g => g.Key)
                       .ToList();
    }
    #endregion
}

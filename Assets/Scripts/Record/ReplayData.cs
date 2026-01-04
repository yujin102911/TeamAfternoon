using System;
using System.Collections.Generic;

[Serializable]
public class ReplayData
{
    public int stageNumber;
    public List<RoundSnapshot> rounds = new List<RoundSnapshot>();
}

[Serializable]
public class RoundSnapshot
{
    public int roundNumber;
    public string enemyPatternName;
    public List<TickSnapshot> ticks = new List<TickSnapshot>();
}

[Serializable]
public class TickSnapshot
{
    public int tick;

    // --- 플레이어 상태 및 결과 ---
    public int playerHP;
    public int playerSector;      // 최종 위치
    public ActionType playerAction;
    public MoveDirection playerMoveDir;
    public int damageToEnemy;     // 실제 준 데미지
    public bool isCrit;           // 크리티컬 여부
    public bool isInterrupted;    // 차징 취소 여부
    public bool isGuardSuccess;   // 가드 성공 여부

    // --- 적 상태 및 결과 ---
    public int enemyHP;
    public bool isLeft;           // 적의 현재 위치 (Dash 시 변경)
    public List<int> enemyAttackSectors; // 공격 비주얼용
    public int damageToPlayer;    // 플레이어가 입은 데미지
    public List<int> stonePositions;    // 필드 위의 모든 돌 위치
    public WindDirection? windDir;      // 발생한 바람 방향
}
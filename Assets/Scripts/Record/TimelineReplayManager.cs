using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimelineReplayManager : MonoBehaviour
{
    [SerializeField] private float _tickInterval = 0.4f;
    private BattleSystem _battleSystem;

    public IEnumerator PlayReplayCoroutine(RoundSnapshot roundData, BattleSystem targetSystem)
    {
        _battleSystem = targetSystem;
        Debug.Log($"[Replay] 라운드 {roundData.roundNumber} 재생 시작");

        foreach (var tickSnapshot in roundData.ticks)
        {
            // 1. 플레이어 상태 및 액션 재현
            _battleSystem.ForceSetPlayerState(tickSnapshot.playerSector, tickSnapshot.playerHP);
            if (tickSnapshot.playerAction != ActionType.None)
            {
                _battleSystem.ForceTriggerPlayerAction(
                    tickSnapshot.playerAction,
                    tickSnapshot.damageToEnemy,
                    tickSnapshot.isCrit,
                    tickSnapshot.playerMoveDir,
                    tickSnapshot.isInterrupted
                );
            }

            yield return new WaitForSeconds(_tickInterval);

            // 2. 적 액션 및 필드 환경(돌 등) 재현
            _battleSystem.ForceSetEnemyState(tickSnapshot.enemyHP, tickSnapshot.isLeft, tickSnapshot.stonePositions);
            if (tickSnapshot.enemyAttackSectors != null)
            {
                _battleSystem.ForceShowEnemyAttack(tickSnapshot.enemyAttackSectors, tickSnapshot.damageToPlayer);
            }

            yield return new WaitForSeconds(_tickInterval);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class BattleSystem 
{

    #region Replay Recording Snapshot Data
    // 이번 틱에서 발생한 결과를 임시 보관하는 프로퍼티들
    public ActionType LastAction { get; set; }
    public int LastDealtDamage { get; set; }
    public bool LastWasCrit { get; set; }
    public int LastTakenDamage { get; set; }
    public bool LastInterrupted { get; set; }
    public MoveDirection LastMoveDir { get; set; }
    public List<int> CurrentAttackSectors { get; set; } = new List<int>();
    #endregion
    public List<int> GetStoneSectors() => _stoneSectors;

    #region Replay Force Methods (재생용)
    public void ForceSetPlayerState(int sector, int hp)
    {
        _playerCurrentSector = sector;
        _playerHP = hp;
        OnPlayerHPChanged?.Invoke(_playerHP, _playerMaxHP);
        OnPlayerMoved?.Invoke(_playerCurrentSector, MoveDirection.None);
    }

    public void ForceTriggerPlayerAction(ActionType action, int damage, bool isCrit, MoveDirection dir, bool interrupted)
    {
        if (interrupted)
        {
            OnChangePlayerAnim?.Invoke("4_Hurt");
            return;
        }

        switch (action)
        {
            case ActionType.Move:
            case ActionType.Jump:
                OnPlayerMoved?.Invoke(_playerCurrentSector, dir);
                break;
            case ActionType.Sword_end:
                if (damage > 0)
                {
                    _enemyHP = Mathf.Max(0, _enemyHP - damage);
                    OnEnemyHPChanged?.Invoke(_enemyHP, _enemyMaxHP);
                    OnEnemyHit?.Invoke(damage, isCrit);
                    OnPlayerAttackSuccess?.Invoke();
                }
                OnChangePlayerAnim?.Invoke("6_2_SwordEnd");
                break;
        }
    }

    public void ForceSetEnemyState(int hp, bool isLeft, List<int> stones)
    {
        _enemyHP = hp;
        OnEnemyHPChanged?.Invoke(_enemyHP, _enemyMaxHP);

        // RuntimeEnemy의 IsLeft를 시각적으로 반영 (이벤트 발행)
        OnEnemySideChanged?.Invoke(isLeft);

        _stoneSectors = new List<int>(stones);
        OnStoneUpdated?.Invoke(_stoneSectors, true);
    }

    public void ForceShowEnemyAttack(List<int> sectors, int damage)
    {
        if (sectors != null && sectors.Count > 0)
            OnEnemyAttackSuccess?.Invoke(sectors);

        if (damage > 0)
            OnPlayerHit?.Invoke();
    }
    #endregion
}
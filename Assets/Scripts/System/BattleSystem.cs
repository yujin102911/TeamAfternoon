using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 전투 로직을 처리하는 System
/// HP, 데미지 등 전투 관련 계산 담당
/// </summary>
public class BattleSystem 
{
    private int _playerHP;
    private int _playerMaxHP;
    private int _playerCurrentSector; // 현재 위치 (1~8)

    private int _totalSectors;

    private int _enemyHP;
    private int _enemyMaxHP;

    private Dictionary<string, int> _playerBuffs = new Dictionary<string, int>();
    private Dictionary<string, int> _enemyBuffs = new Dictionary<string, int>();

    public event Action<int, int> OnPlayerHPChanged;
    public event Action<int, int> OnEnemyHPChanged;
    public event Action<int> OnPlayerMoved;
    public event Action<int, int> OnDamageDealt; // (target, damage) 0: player 1: enemy
    public event Action<string, int, bool> OnBuffChanged;

    public int PlayerHP => _playerHP;
    public int PlayerMaxHP => _playerMaxHP;
    public int PlayerCurrentSector => _playerCurrentSector;
    public int EnemyHP => _enemyHP; 
    public int EnemyMaxHP => _enemyMaxHP;

    public void InitializeBattle(int playerMaxHP, int enemyMaxHP, int totalSectors, int startSector = 1)
    {
        _playerHP = playerMaxHP;
        _playerMaxHP = playerMaxHP;
        _playerCurrentSector = startSector;

        _totalSectors = totalSectors;

        _enemyHP = enemyMaxHP;
        _enemyMaxHP = enemyMaxHP;

        _playerBuffs.Clear();
        _enemyBuffs.Clear();

        Debug.Log($"[BattleSystem] 전투 초기화 - 플레이어 HP: {_playerHP}/{playerMaxHP}, 적 HP: {_enemyHP}/{_enemyMaxHP}");

        OnPlayerHPChanged?.Invoke(_playerHP, _playerMaxHP);
        OnEnemyHPChanged?.Invoke(_enemyHP, _enemyMaxHP);
        OnPlayerMoved?.Invoke(_playerCurrentSector);

    }

    public void DealDamageToEnemy(int damage)
    {
        if (damage <= 0) return;

        int finalDamage = CalculateDamage(damage, true);

        _enemyHP = Mathf.Max(0, _enemyHP - finalDamage);
        Debug.Log($"[BattleSystem] 적에게 {finalDamage} 데미지! 남은 HP: {_enemyHP}/{_enemyMaxHP}");

        OnDamageDealt?.Invoke(1, finalDamage);
        OnEnemyHPChanged?.Invoke(_enemyHP, _enemyMaxHP);

        if (_enemyHP <= 0)
        {
            OnEnemyDefeated();
        }
    }

    public void DealDamageToPlayer(int damage)
    {
        if (damage <= 0) return;

        int finalDamage = CalculateDamage(damage, false);

        _playerHP = Mathf.Max(0, _playerHP - finalDamage);
        Debug.Log($"[BattleSystem] 플레이어가 {finalDamage} 데미지 받음! 남은 HP: {_playerHP}/{_playerMaxHP}");

        OnDamageDealt?.Invoke(0, finalDamage);
        OnPlayerHPChanged?.Invoke(_playerHP, _playerMaxHP);

        if (_playerHP <= 0)
        {
            OnPlayerDefeated();
        }
    }

    private int CalculateDamage(int baseDamage, bool isPlayerAttack)
    {
        int finalDamage = baseDamage;
        if (isPlayerAttack)
        {
            int power = GetBuffValue("Power", true);
            finalDamage += power;

            int enemyWeaken = GetBuffValue("Weaken", false);
            if (enemyWeaken > 0)
            {
                finalDamage += enemyWeaken;
                Debug.Log($"[BattleSystem] 적 약화({enemyWeaken})로 추가 피해 적용!");
            }

        }
        else
        {
            int playerWeaken = GetBuffValue("Weaken", true);
            if (playerWeaken > 0) finalDamage += playerWeaken;
        }

        return Mathf.Max(0, finalDamage);   
    }

    public void SetPlayerStartPosition(int sector)
    {
        int targetSector = sector;
        while (targetSector > _totalSectors)
        {
            targetSector -= _totalSectors;
        }
        while (targetSector < 1)
        {
            targetSector += _totalSectors;
        }
        _playerCurrentSector = targetSector;
        Debug.Log($"[BattleSystem] 플레이어 시작 위치 갱신됨: {_playerCurrentSector}");
        OnPlayerMoved?.Invoke(_playerCurrentSector);
    }

    public void MovePlayer(MoveDirection moveDirection) 
    {
        if (moveDirection == MoveDirection.None) return;

        int speedBonus = GetBuffValue("Speed", true);
        int moveAmount = 1 + speedBonus;

        int direction = (moveDirection == MoveDirection.Right) ? 1 : -1;
        int targetSector = _playerCurrentSector + (direction * moveAmount);

        while (targetSector > _totalSectors)
        {
            targetSector -= _totalSectors;
        }
        while (targetSector < 1)
        {
            targetSector += _totalSectors;
        }

        int prevSector = _playerCurrentSector;
        _playerCurrentSector = Mathf.Clamp(targetSector, 1, 8);

        if (prevSector != _playerCurrentSector)
        {
            Debug.Log($"[BattleSystem] 이동: {prevSector} -> {_playerCurrentSector} (Speed보너스: {speedBonus})");
            OnPlayerMoved?.Invoke(_playerCurrentSector);
        }
    }

    public bool IsPlayerHitByAttack(EnemyAttack attack)
    {
        if (attack == null || attack.targetSectors == null) return false;
        return attack.targetSectors.Contains(_playerCurrentSector);
    }

    public void ProcessEnemyAttack(EnemyAttack attack)
    {
        if (attack == null) return;
        Debug.Log($"[BattleSystem] 적 공격! 대상 섹터: [{string.Join(", ", attack.targetSectors)}]");
        
        if (IsPlayerHitByAttack(attack))
        {
            DealDamageToPlayer(attack.damage);
        }
        else
        {
            Debug.Log($"[BattleSystem] 회피 성공! (플레이어 위치: 섹터 {_playerCurrentSector})");
        }
    }

    public void HealPlayer(int amount)
    {
        if (amount <= 0) return;

        _playerHP = Mathf.Min(_playerMaxHP, _playerHP +  amount);

        Debug.Log($"[BattleSystem] 플레이어 {amount} 회복! (현재 HP: {_playerHP}/{_playerMaxHP})");

        OnPlayerHPChanged?.Invoke(_playerHP, _playerMaxHP);
    }

    private void OnEnemyDefeated()
    {
        Debug.Log("[BattleSystem] 적 처치!");
        GameManager.Instance?.EndBattle(true);
    }
    private void OnPlayerDefeated()
    {
        Debug.Log("[BattleSystem] 플레이어 사망...");
        GameManager.Instance?.EndBattle(false);
    }
    /// <summary>
    /// 디버그용: 현재 전투 상태 출력
    /// </summary>
    public void PrintBattleStatus()
    {
        Debug.Log($"=== 전투 상태 ===");
        Debug.Log($"플레이어: HP {_playerHP}/{_playerMaxHP}, 위치 섹터 {_playerCurrentSector}");
        Debug.Log($"적: HP {_enemyHP}/{_enemyMaxHP}");
    }

    public void SetBuff(string buffName, int amount, bool isPlayer)
    {
        Dictionary<string, int> buffs = isPlayer ? _playerBuffs : _enemyBuffs;
        buffs[buffName] = amount;

        string target = isPlayer ? "플레이어" : "적";
        Debug.Log($"[BattleSystem] {target}의 {buffName}을 {amount}로 설정");

        OnBuffChanged?.Invoke(buffName, amount, isPlayer);
    }

    public void RemoveBuff(string buffName, bool isPlayer)
    {
        Dictionary<string, int> buffs = isPlayer ? _playerBuffs : _enemyBuffs;

        if (buffs.Remove(buffName))
        {
            string target = isPlayer ? "플레이어" : "적";
            Debug.Log($"[BattleSystem] {target}의 {buffName} 제거");
            OnBuffChanged?.Invoke(buffName, 0, isPlayer);
        }
    }

    public int GetBuffValue(string buffName, bool isPlayer)
    {
        Dictionary<string, int> buffs = isPlayer ? _playerBuffs : _enemyBuffs;
        return buffs.ContainsKey(buffName) ? buffs[buffName] : 0;
    }

    public void DecayBuffs()
    {
        DecayBuffsForTarget(_playerBuffs, true);
        DecayBuffsForTarget(_enemyBuffs, false);
    }

    private void DecayBuffsForTarget(Dictionary<string, int> buffs, bool isPlayer)
    {
        List<string> toRemove = new List<string>();

        foreach (var pair in buffs)
        {
            if (pair.Value > 0)
            {
                buffs[pair.Key]--;
                OnBuffChanged?.Invoke(pair.Key, buffs[pair.Key], isPlayer);

                if (buffs[pair.Key] <= 0)
                    toRemove.Add(pair.Key);
            }
        }

        foreach (string key in toRemove)
        {
            buffs.Remove(key);
        }
    }
}

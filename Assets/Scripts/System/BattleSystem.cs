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

    private float _vulnerableAmount = 1.5f;

    private int _totalSectors;

    private List<RuntimeEnemy> _enemies = new List<RuntimeEnemy>();

    private Dictionary<string, int> _playerBuffs = new Dictionary<string, int>();
    private Dictionary<string, int> _enemyBuffs = new Dictionary<string, int>();

    public event Action<int, int> OnPlayerHPChanged;
    public event Action<RuntimeEnemy> OnEnemyHPChanged;
    public event Action<RuntimeEnemy> OnEnemyDied;
    public event Action<int> OnPlayerMoved;
    public event Action<string, int, bool> OnBuffChanged;
    public event Action OnBattleInitialized;
    public event Action<List<int>> OnEnemyAttackExecute;

    public int PlayerHP => _playerHP;
    public int PlayerMaxHP => _playerMaxHP;
    public int PlayerCurrentSector => _playerCurrentSector;
    public IReadOnlyList<RuntimeEnemy> Enemies => _enemies;

    public void InitializeBattle(List<RuntimeEnemy> enemies, int playerMaxHP, int totalSectors, int startSector = 1)
    {
        _playerHP = playerMaxHP;
        _playerMaxHP = playerMaxHP;
        _playerCurrentSector = startSector;

        _totalSectors = totalSectors;

        _enemies = enemies;

        _playerBuffs.Clear();
        _enemyBuffs.Clear();

        OnBattleInitialized?.Invoke();

        Debug.Log($"[BattleSystem] 전투 초기화 - 플레이어 HP: {_playerHP}/{playerMaxHP}");
        Debug.Log($"[BattleSystem] 전투 초기화 - 적 {_enemies.Count} 마리 배치됨");
        OnPlayerHPChanged?.Invoke(_playerHP, _playerMaxHP);
        foreach (RuntimeEnemy enemy in _enemies)
        {
            OnEnemyHPChanged?.Invoke(enemy);
        }

        OnPlayerMoved?.Invoke(_playerCurrentSector);
    }

    public void DealDamageToCurrentSector(int damage, int currentTick)
    {
        if (damage <= 0) return;

        int attackPos = PlayerCurrentSector;

        RuntimeEnemy target = null;

        // 공격 위치에 있는 적 찾기
        foreach (RuntimeEnemy enemy in _enemies)
        {
            if (enemy.IsHitByAttackFrom(attackPos))
            {
                target = enemy;
                break; // 한명만
            }
        }

        if (target != null)
        {

            if (target.CurrentPattern != null)
            {
                // 적이 현재 틱에 패링 중인지 확인
                EnemyParrying parry = target.CurrentPattern.GetParryingAt(currentTick);

                if (parry != null)
                {
                    // 패링 성공! (적은 데미지 안 입고, 플레이어가 데미지 입음)
                    Debug.Log($"[BattleSystem] 패링 발생: {target.Data.Enemy_Name}가 공격을 튕겨냈습니다");
                    int reflectDamage = Mathf.CeilToInt(damage * parry.damageMultiplier);

                    DealDamageToPlayer(reflectDamage);

                    return;
                }
            }
            int finalDamage = CalculateDamage(damage, true);

            target.TakeDamage(finalDamage);
            Debug.Log($"[BattleSystem] {target.Data.Enemy_Name} 피격! ({finalDamage} 피해)");

            OnEnemyHPChanged?.Invoke(target);

            if (target.IsDead)
            {
                Debug.Log($"[BattleSystem] {target.Data.Enemy_Name} 사망");
                OnEnemyDied?.Invoke(target);
            }
        }
        else
        {
            Debug.Log($"[BattleSystem] 공격 빗나감 (섹터 {attackPos}에 적 없음");
        }
    }

    public void DealDamageToPlayer(int damage)
    {
        if (damage <= 0) return;

        int finalDamage = CalculateDamage(damage, false);

        _playerHP = Mathf.Max(0, _playerHP - finalDamage);
        Debug.Log($"[BattleSystem] 플레이어가 {finalDamage} 데미지 받음! 남은 HP: {_playerHP}/{_playerMaxHP}");

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

            if (GetBuffValue("Vulnerable", false) > 0)
            {
                finalDamage = Mathf.FloorToInt(finalDamage * _vulnerableAmount);
                Debug.Log($"[BattleSystem] 적 취약 상태! 데미지 {_vulnerableAmount}배 적용");
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
        if (_totalSectors <= 0)
        {
            _playerCurrentSector = sector;
            Debug.LogWarning("[BattleSystem] 맵 크기(_totalSectors)가 0입니다! 초기화 순서를 확인하세요.");
            OnPlayerMoved?.Invoke(_playerCurrentSector);
            return;
        }   
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
        if (attack.targetSectors != null && attack.targetSectors.Count > 0)
        {
            OnEnemyAttackExecute?.Invoke(attack.targetSectors);
        }
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

    /// <summary>
    /// 
    /// </summary>
    private void DecayBuffsForTarget(Dictionary<string, int> buffs, bool isPlayer)
    {
        List<string> keys = new List<string>(buffs.Keys);
        List<string> toRemove = new List<string>();

        foreach (string key in keys)
        {
            if (!buffs.ContainsKey(key)) continue;
            if (buffs[key] > 0)
            {
                buffs[key]--;
                OnBuffChanged?.Invoke(key, buffs[key], isPlayer);
                if (buffs[key] <= 0)
                    toRemove.Add(key);
            }
        }
        foreach (string key in toRemove)
        {
            if (buffs.ContainsKey(key))
            {
                buffs.Remove(key);
                OnBuffChanged?.Invoke(key, 0, isPlayer); 
            }
        }
    }
}

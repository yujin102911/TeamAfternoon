using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 전투 로직을 처리하는 System
/// HP, 데미지 등 전투 관련 계산 담당
/// </summary>
public class BattleSystem 
{
    #region Private Fields
    private int _playerHP;
    private int _playerMaxHP;
    private int _playerCurrentSector; // 현재 위치 (1~8)

    private float _vulnerableAmount = 1.5f;

    private int _totalSectors;

    private List<RuntimeEnemy> _enemies = new List<RuntimeEnemy>(); // 현재 싸우고 있는 적

    private Dictionary<string, int> _playerBuffs = new Dictionary<string, int>();
    private Dictionary<string, int> _enemyBuffs = new Dictionary<string, int>();

    private List<int> _ableCureSectors = new List<int>();
    private int _maxCure = 20;
    private int _currentCure = 0;
    private int _curePower = 1;
    private int _recoverCycle = 3;

    private int _battleTurnCount = 0;

    #endregion

    #region Events
    public event Action<int, int> OnPlayerHPChanged;          // 플레이어 HP 변화 시 발행되는 이벤트(UI용)
    public event Action<int, int> UpdateCureGauage;             // 정화 게이지 UI업데이트

    public event Action OnEnemyPurified; // 적 정화 완료시 발행

    public event Action<RuntimeEnemy> OnEnemyHit;       // 적 맞았을 때 발행 (일단 안씀)
    public event Action OnPlayerAttack;                       // 때릴 때 발행되는 이벤트
    public event Action OnPlayerHit;                          // 맞을 때 발행되는 이벤트
    public event Action<int> OnPlayerMoved;
    public event Action<List<int>> OnEnemyAttack;             // 적이 공격할 때 발행되는 이벤트(섹터반짝용)
    public event Action<string, int, bool> OnBuffChanged;

    public event Action OnBattleInitialized;
    #endregion

    #region Properties
    public int PlayerHP => _playerHP;
    public int PlayerMaxHP => _playerMaxHP;
    public int PlayerCurrentSector => _playerCurrentSector;

    public int TotalDamage;
    public IReadOnlyList<RuntimeEnemy> Enemies => _enemies;

    public List<int> AbleCureSectors => _ableCureSectors;
    public int RecoverCycle => _recoverCycle;
    #endregion

    /// <summary>
    /// 체력, 적, 버프 초기화 미리 설정
    /// </summary>
    public void InitializeBattle(List<RuntimeEnemy> enemies, int playerMaxHP, int totalSectors, bool keepPlayerHP = false)
    {
        _totalSectors = totalSectors;
        _enemies = enemies;

        if (!keepPlayerHP) _playerHP = playerMaxHP;
        _playerMaxHP = playerMaxHP;
        if (_playerHP <= 0) _playerHP = playerMaxHP;

        _playerBuffs.Clear();
        _enemyBuffs.Clear();
        ChooseCureSector();

        _currentCure = 0;
        if (_enemies.Count > 0 && _enemies[0].Data != null)
            _maxCure = _enemies[0].Data.MaxCureValue;
        else
            _maxCure = 50;

            OnBattleInitialized?.Invoke();

        OnPlayerHPChanged?.Invoke(_playerHP, _playerMaxHP);
        UpdateCureGauage?.Invoke(_currentCure, _maxCure);

        _battleTurnCount = 0;
        Debug.Log($"[BattleSystem] 전투 시작! 목표 정화량: {_maxCure}");

    }

    

    public void DealDamageToCurrentSector(int damage, int currentTick)
    {
        OnPlayerAttack?.Invoke(); // 플레이어 공격 모션

        // 공격 위치에 적이 있는지 확인
        foreach (RuntimeEnemy enemy in _enemies)
        {
            if (enemy.IsHitByAttackFrom(_playerCurrentSector))
            {
                // 패링 체크 등은 여기서 수행
                if (enemy.CurrentPattern != null)
                {
                    EnemyParrying parry = enemy.CurrentPattern.GetParryingAt(currentTick);
                    if (parry != null)
                    {
                        DealDamageToPlayer((int)(damage * parry.damageMultiplier));
                        return;
                    }
                }

                // 적 피격 연출
                OnEnemyHit?.Invoke(enemy);
                Debug.Log($"[BattleSystem] 적({enemy.Data.Enemy_Name}) 타격! (물리 데미지는 0)");
            }
        }
    }

    public void DealDamageToPlayer(int damage)
    {
        if (damage <= 0) return;

        int finalDamage = CalculateDamage(damage, false);

        _playerHP = Mathf.Max(0, _playerHP - finalDamage);
        Debug.Log($"[BattleSystem] 플레이어가 {finalDamage} 데미지 받음! 남은 HP: {_playerHP}/{_playerMaxHP}");

        OnPlayerHPChanged?.Invoke(_playerHP, _playerMaxHP);
        OnPlayerHit?.Invoke();
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
            OnEnemyAttack?.Invoke(attack.targetSectors);
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

    public RuntimeEnemy GetEnemyAtSector(int sectorIndex)
    {
        if (_enemies == null) return null;
        foreach (RuntimeEnemy enemy in _enemies)
        {
            if (enemy.AttackableSectors.Contains(sectorIndex))
                return enemy;
        }
        return null;
    }

    // 정화 가능한 섹터 뽑기
    public void ChooseCureSector()
    {
        if(_ableCureSectors == null) return;

        _ableCureSectors.Clear();

        int a = Random.Range(1, _totalSectors + 1);
        int b;

        do
        {
            b = Random.Range(1, _totalSectors + 1);
        }
        while (b == a);

        _ableCureSectors.Add(a);
        _ableCureSectors.Add(b);
    }

    // 정화 시도
    public bool TryCurePage(int tick, bool is_cure)
    {
        Debug.Log($"페이지 정화 시도");
        if (_ableCureSectors.Contains(_playerCurrentSector) && is_cure)
        {
            //정화 연산
            Cure(_curePower);
            Debug.Log($"정화 성공");
            return true;
        }

        Debug.Log($"정화 실패");
        return false;
    }

    public void Cure(int damage) 
    {
        if (damage <= 0) return;

        OnPlayerAttack?.Invoke();

        IncreaseCureGauge(damage);

        // 정화 되었는지 체크
        if (_currentCure >= _maxCure)
        {
            Debug.Log($"[BattleSystem] 정화 완료!");
            OnEnemyPurified?.Invoke();
        }
    }

    // 정화 수치 증가
    public void IncreaseCureGauge(int amount)
    {
        if (amount <= 0) return;

        _currentCure = Mathf.Min(_maxCure, _currentCure + amount);
        Debug.Log($"[BattleSystem] 정화 발동! ({amount} 수치 정화)");

        UpdateCureGauage?.Invoke(_currentCure, _maxCure);
    }

    // 정화 수치 감소
    public void DecreaseCureGauge(int amount)
    {
        if (amount <= 0) return;

        _currentCure = Mathf.Max(0, _currentCure - amount);

        UpdateCureGauage?.Invoke(_currentCure, _maxCure);
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

    public void OnRoundEnded()
    {
        _battleTurnCount++;
        if (_battleTurnCount > 0 && _battleTurnCount % _recoverCycle == 0)
        {
            DecreaseCureGauge(5);
            Debug.Log($"[BattleSystem] {_recoverCycle}턴 경과! 정화 수치 자연 감소 (-5)");
        }
    }
}

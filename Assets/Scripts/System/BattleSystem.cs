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

    private int _totalSectors;
    private int _columns;

    private List<RuntimeEnemy> _enemies = new List<RuntimeEnemy>(); // 현재 싸우고 있는 적

    private int _enemyHP;
    private int _enemyMaxHP;

    private int _battleTurnCount = 0;

    #endregion

    #region Events
    public event Action<int, int> OnPlayerHPChanged;          // 플레이어 HP 변화 시 발행되는 이벤트(UI용)
    public event Action<int, int> OnEnemyHPChanged;

    public event Action OnPlayerMeleeAttack; // 근접 공격시 발행되는 이벤트
    public event Action OnPlayerLongRangeAttack; // 원거리 공격시 발행되는 이벤트
    public event Action OnPlayerLongRangeStart; // 원거리 공격시 발행되는 이벤트
    public event Action OnPlayerLongRangeMiddle; // 원거리 공격시 발행되는 이벤트

    public event Action OnEnemyDied; // 적 사망시 발행되는 이벤트
    public event Action OnPlayerDied; // 플레이어 사망 시 발행되는 이벤트

    public event Action<int> OnEnemyHit; // 적이 맞을 때 발행되는 이벤트
    public event Action OnPlayerHit; // 플레이어가 맞을 때 발행되는 이벤트
    public event Action OnPlayerAttackSuccess; // 성공적으로 때렸을 때 발행되는 이벤트
    public event Action<int, MoveDirection> OnPlayerMoved; // 플레이어가 움직였을 때 발행되는 이벤트
    public event Action<List<int>> OnEnemyAttackSuccess; // 적이 공격할 때 발행되는 이벤트(섹터반짝용)

    public event Action OnBattleInitialized;
    #endregion

    #region Properties
    public int PlayerHP => _playerHP;
    public int PlayerMaxHP => _playerMaxHP;
    public int EnemyHP => _enemyHP;
    public int EnemyMaxHP => _enemyMaxHP;
    public int PlayerCurrentSector => _playerCurrentSector;

    public int TotalDamage;
    public IReadOnlyList<RuntimeEnemy> Enemies => _enemies;
    #endregion

    /// <summary>
    /// 체력, 적, 버프 초기화 미리 설정
    /// </summary>
    public void InitializeBattle(List<RuntimeEnemy> enemies, int playerMaxHP, int totalSectors, int columns, bool keepPlayerHP = false)
    {
        _totalSectors = totalSectors;
        _columns = columns;
        _enemies = enemies;

        if (!keepPlayerHP) _playerHP = playerMaxHP;
        _playerMaxHP = playerMaxHP;
        if (_playerHP <= 0) _playerHP = playerMaxHP;

        if (_enemies.Count > 0 && _enemies[0].Data != null)
            _enemyMaxHP = _enemies[0].Data.MaxHP;
        else
            _enemyMaxHP = 100;

        _enemyHP = _enemyMaxHP;

        OnBattleInitialized?.Invoke();
        OnPlayerHPChanged?.Invoke(_playerHP, _playerMaxHP);
        OnEnemyHPChanged?.Invoke(_enemyHP, _enemyMaxHP);    

        _battleTurnCount = 0;

    }

    /// <summary>
    /// 근거리 공격
    /// </summary>
    public void MeleeAttack(int damage)
    {
        OnPlayerMeleeAttack?.Invoke(); // 근거리 공격 애니메이션 이벤트

        // 공격 위치에 적이 있는지 확인
        foreach (RuntimeEnemy enemy in _enemies)
        {
            if (enemy.IsHitByAttackFrom(_playerCurrentSector))
            {
                //타격 범위인지 확인
                if(_playerCurrentSector % _columns == 0)
                {
                    DamageEnemy(damage);
                    OnPlayerAttackSuccess?.Invoke(); // 플레이어 공격 성공 모션

                    // 적 피격 연출
                    OnEnemyHit?.Invoke(damage);
                    Debug.Log($"[BattleSystem] 적({enemy.Data.Enemy_Name}) 타격! (데미지는 {damage})");
                }
            }
        }
    }
    /// <summary>
    /// 원거리 공격
    /// </summary>
    public void LongRangeAttack(int damage)
    {
        OnPlayerLongRangeAttack?.Invoke();
        DamageEnemy(damage );
        OnEnemyHit?.Invoke(damage);
    }

    public void LongRangeAttack_Start()
    {
        OnPlayerLongRangeStart?.Invoke();
    }

    public void LongRangeAttack_Middle()
    {
        OnPlayerLongRangeMiddle?.Invoke();
    }

    private void DamageEnemy(int amount)
    {
        _enemyHP = Mathf.Max(0, _enemyHP - amount);
        Debug.Log($"[BattleSystem] 적에게 {amount} 데미지! 남은 HP: {_enemyHP}");

        OnEnemyHPChanged?.Invoke(_enemyHP, _enemyMaxHP);
        if (_enemyHP <= 0)
        {
            Debug.Log("적 처치 완료");
            OnEnemyDied?.Invoke();
            OnEnemyDefeated();
        }
    }

    public void DealDamageToPlayer(int damage)
    {
        if (damage <= 0) return;

        _playerHP = Mathf.Max(0, _playerHP - damage);
        Debug.Log($"[BattleSystem] 플레이어가 {damage} 데미지 받음! 남은 HP: {_playerHP}/{_playerMaxHP}");

        OnPlayerHPChanged?.Invoke(_playerHP, _playerMaxHP);
        OnPlayerHit?.Invoke();
        if (_playerHP <= 0)
        {
            OnPlayerDied?.Invoke();
            OnPlayerDefeated();
        }
    }

    public void SetPlayerStartPosition(int sector)
    {
        if (_totalSectors <= 0)
        {
            _playerCurrentSector = sector;
            Debug.LogWarning("[BattleSystem] 맵 크기(_totalSectors)가 0입니다! 초기화 순서를 확인하세요.");
            OnPlayerMoved?.Invoke(_playerCurrentSector, MoveDirection.None);
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
        //OnPlayerMoved?.Invoke(_playerCurrentSector);
    }

    public void MovePlayer(MoveDirection moveDirection) 
    {
        if (moveDirection == MoveDirection.None || _columns <= 0) return;

        int moveAmount = 1;

        int currentIndex = _playerCurrentSector - 1;
        int curRow = currentIndex / _columns;
        int curCol = currentIndex % _columns;

        int targetRow = curRow;
        int targetCol = curCol;

        switch (moveDirection)
        {
            case MoveDirection.Front:
                targetCol += moveAmount;
                break;
            case MoveDirection.Back:
                targetCol -= moveAmount;
                break;
            case MoveDirection.Left:
                targetRow -= moveAmount;
                break;
            case MoveDirection.Right:
                targetRow += moveAmount;
                break;
            case MoveDirection.DiagonalLu:
                targetRow -= moveAmount;
                targetCol += moveAmount;
                break;
            case MoveDirection.DiagonalRu:
                targetRow += moveAmount;
                targetCol += moveAmount;
                break;
            case MoveDirection.DiagonalLd:
                targetRow -= moveAmount;
                targetCol -= moveAmount;
                break;
            case MoveDirection.DiagonalRd:
                targetRow += moveAmount;
                targetCol -= moveAmount;
                break;
        }
        int rows = _totalSectors / _columns;
        if (targetRow >= 0 && targetRow < rows && targetCol >= 0 && targetCol < _columns)
        {
            int prevSector = _playerCurrentSector;
            _playerCurrentSector = (targetRow *  _columns) + targetCol + 1;
            if (prevSector != _playerCurrentSector)
            {
                Debug.Log($"[BattleSystem] 이동 성공 {prevSector} -> {_playerCurrentSector}");
                OnPlayerMoved?.Invoke(_playerCurrentSector, moveDirection);
            }
        }
        else
        {
            Debug.Log("[BattleSystem]이동 불가");
        }
    }

    bool IsAdjacent(int from, int to)
    {
        int fromIndex = from - 1; // 1부터 시작하니까
        int toIndex = to - 1;

        int fromRow = fromIndex / _columns;
        int fromCol = fromIndex % _columns;

        int toRow = toIndex / _columns;
        int toCol = toIndex % _columns;

        int rowDiff = Mathf.Abs(fromRow - toRow);
        int colDiff = Mathf.Abs(fromCol - toCol);

        // 상하좌우만 허용
        return (rowDiff == 1 && colDiff == 0) ||
               (rowDiff == 0 && colDiff == 1);
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
            OnEnemyAttackSuccess?.Invoke(attack.targetSectors);
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

    public void OnRoundEnded()
    {
        _battleTurnCount++;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// 크리티컬을 위한 구조체
public struct DamageResult
{
    public int damage;
    public bool isCritical;
}



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
    private List<int> _stoneSectors = new List<int>();


    private int _enemyHP;
    private int _enemyMaxHP;

    private bool _isGuarding = false; // 방어 플래그
    private bool _isBowCharging = false; // 활 플래그
    private bool _isSwordCharging = false; // 검 플래그
    private bool _isPlayerHitThisTurn = false; // 이번 턴에 플레이어가 맞았는지 여부

    private int _meleeAttackStack = 0;              // 라운드 내 누적 스택
    private float _defualtCriticalChance = 0.3f;    //기본 치명타 확률
    private float _criticMulti = 0.02f;             //치명타 증가량

    private int _battleTurnCount = 0;

    private int _damageBuffer = 0;

    #endregion

    #region Events
    public event Action<int, int> OnPlayerHPChanged;          // 플레이어 HP 변화 시 발행되는 이벤트(UI용)
    public event Action<int, int> OnEnemyHPChanged;

    public event Action<float> OnCriticalChanceChanged;

    public event Action OnPlayerMeleeAttack; // 근접 공격시 발행되는 이벤트
    public event Action OnPlayerLongRangeAttack; // 원거리 공격시 발행되는 이벤트
    public event Action OnPlayerLongRangeStart; // 원거리 공격시 발행되는 이벤트
    public event Action OnPlayerLongRangeMiddle; // 원거리 공격시 발행되는 이벤트
    public event Action OnPlayerGuard;
    public event Action OnPlayerIdle;

    public event Action<string> OnChangePlayerAnim; // 플레이어 트리거 변경시 발행되는 이벤트
    public event Action<string> OnChangeEnemyAnim; // 적 트리거 변경시 발행되는 이벤트

    public event Action OnStartMelee; // 근접 차징 시작
    public event Action OnMiddleMelee; // 근접 차징 시작
    public event Action OnEndMelee; // 근접 차징 시작

    public event Action OnEnemyDied; // 적 사망시 발행되는 이벤트
    public event Action OnPlayerDied; // 플레이어 사망 시 발행되는 이벤트

    public event Action<int, bool> OnEnemyHit; // 적이 맞을 때 발행되는 이벤트
    public event Action OnPlayerHit; // 플레이어가 맞을 때 발행되는 이벤트
    public event Action OnPlayerAttackSuccess; // 성공적으로 때렸을 때 발행되는 이벤트
    public event Action<int, MoveDirection> OnPlayerMoved; // 플레이어가 움직였을 때 발행되는 이벤트
    public event Action<List<int>> OnEnemyAttackSuccess; // 적이 공격할 때 발행되는 이벤트(섹터반짝용)

    public event Action<List<int>, bool> OnStoneUpdated; // 돌 던질때, 혹은 사라질때 발행되는 이벤트 (사라질때 false, 생길때 true)

    public event Action<bool> OnEnemySideChanged; // 적 위치 변경 이벤트
    public event Action<List<int>, bool> OnEnemyDash;          // 적 돌진 이벤트

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
    public int MeleeAttackStack => _meleeAttackStack;
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

        ResetMeleeStack();
    }

    /// <summary>
    /// 근거리 공격
    /// </summary>
    public bool MeleeAttack(int damage, bool isChargeRequired)
    {
        if (isChargeRequired && !_isSwordCharging)
        {
            Debug.Log("<color=red>[BattleSystem] 검 차징이 끊겨 공격에 실패했습니다!</color>");
            return false;
        }

        _damageBuffer = damage;

        

        // 공격 위치에 적이 있는지 확인
        foreach (RuntimeEnemy enemy in _enemies)
        {
            if (enemy.IsHitByAttackFrom(_playerCurrentSector, _columns))
            {
                OnChangePlayerAnim?.Invoke(isChargeRequired ? "6_2_SwordEnd" : "2_2_SwordAttack");

                return true;
            }
        }
        _isSwordCharging = false;
        return false;
    }
    /// <summary>
    /// 원거리 공격
    /// </summary>
    public void LongRangeAttack(int damage, bool isChargeRequired)
    {
        if (isChargeRequired && !_isBowCharging)
        {
            Debug.Log("[BattleSystem] 차징이 취소되어 공격에 실패했습니다.");
            return;
        }

        _damageBuffer = damage;

        OnChangePlayerAnim?.Invoke("2_3_BowShoot");

        _isBowCharging = false;
    }
    /// <summary>
    /// 적 데미지 연출 실행
    /// </summary>
    public void EnemyTakeDamage()
    {
        float critChance = _defualtCriticalChance + (_criticMulti * MeleeAttackStack);

        if (critChance > 1)
        {
            critChance = 1;
        }

        DamageResult result = CalculateDamage(_damageBuffer, critChance);

        int final_dam = result.damage;

        DamageEnemy(final_dam);
        OnEnemyHit?.Invoke(final_dam, result.isCritical);
    }

    // 데미지 결과 계산
    public DamageResult CalculateDamage(int baseDamage, float critChance)
    {
        bool isCritical = UnityEngine.Random.value < critChance;

        int finalDamage = isCritical
            ? baseDamage * 2
            : baseDamage;

        return new DamageResult
        {
            damage = finalDamage,
            isCritical = isCritical
        };
    }


    public void LongRangeAttack_Start()
    {
        _isBowCharging = true;
        Debug.Log("[BattleSystem] 활 차징 시작");

        OnChangePlayerAnim?.Invoke("2_1_BowAttack");
    }

    public void LongRangeAttack_Middle()
    {
        OnChangePlayerAnim?.Invoke("2_2_BowMiddle");
    }

    public void MeleeAttack_Start()
    {
        _isSwordCharging = true;
        Debug.Log("[BattleSystem] 검 차징 시작");

        OnChangePlayerAnim?.Invoke("6_SwordCharge");
    }

    public void MeleeAttack_Middle()
    {
        OnChangePlayerAnim?.Invoke("6_1_SwordMiddle");
    }

    /// <summary>
    /// 치명타 확률 증가
    /// </summary>
    public void IncreaseMeleeStack()
    {
        _meleeAttackStack++;
        Debug.Log($"[BattleSystme] 근거리 공격 스택 증가. 현재 스택: {_meleeAttackStack}");

        float critChance = _defualtCriticalChance + (_criticMulti * MeleeAttackStack);

        if (critChance > 1)
        {
            critChance = 1;
        }

        OnCriticalChanceChanged?.Invoke(critChance);
    }

    /// <summary>
    /// 치명타 확률 초기화
    /// </summary>
    public void ResetMeleeStack()
    {
        _meleeAttackStack = 0;
        float critChance = _defualtCriticalChance + (_criticMulti * MeleeAttackStack);

        if (critChance > 1)
        {
            critChance = 1;
        }

        OnCriticalChanceChanged?.Invoke(critChance);
    }

    private void DamageEnemy(int amount)
    {
        _enemyHP = Mathf.Max(0, _enemyHP - amount);
        Debug.Log($"[BattleSystem] 적에게 {amount} 데미지! 남은 HP: {_enemyHP}");

        OnEnemyHPChanged?.Invoke(_enemyHP, _enemyMaxHP);
        if (_enemyHP <= 0)
        {
            Debug.Log("적 처치 완료");
            OnChangeEnemyAnim?.Invoke("Die");
            OnEnemyDefeated();
        }
    }

    public void DealDamageToPlayer(int damage)
    {
        if (damage <= 0) return;

        int finalDamage = _isGuarding ? 0 : damage;

        if (_isGuarding)
        {
            return;
        }
        if (_isBowCharging)
        {
            _isBowCharging = false;
            Debug.Log("[BattleSystem] 피격으로 인해 활 차징이 취소되었습니다!");
        }
        if (_isSwordCharging)
        {
            _isSwordCharging = false;
            Debug.Log("[BattleSystem] 피격으로 인해 검 차징이ㅣ 취소되었습니다.");
        }
        _playerHP = Mathf.Max(0, _playerHP - damage);
        Debug.Log($"[BattleSystem] 플레이어가 {damage} 데미지 받음! 남은 HP: {_playerHP}/{_playerMaxHP}");

        //아드레날린 조건 파괴
        ResetMeleeStack();
    }

    public void PlayerTakeDamage()
    {
        if (!_isPlayerHitThisTurn)
        {
            return;
        }
        else
        {
            _isPlayerHitThisTurn = false;

            if (_isGuarding)
            {
                OnChangePlayerAnim?.Invoke("7_1_GuardSuccess");
                Debug.Log("<color=blue>[BattleSystem] 방어 성공! 데미지 0</color>");
                _isGuarding = false;
            }
            else
            {
                OnPlayerHPChanged?.Invoke(_playerHP, _playerMaxHP);
                OnChangePlayerAnim?.Invoke("4_Hurt");
                Debug.Log("<color=red>[BattleSystem] 플레이어 아야!!!</color>");

                if (_playerHP <= 0)
                {
                    OnPlayerDied?.Invoke();
                    OnPlayerDefeated();
                }
            }
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
            int targetSector = (targetRow * _columns) + targetCol + 1;
            if (IsSectorBlocked(targetSector))
            {
                Debug.Log($"[BattleSystem] {targetSector}번 섹터는 돌에 막혀 이동할 수 없습니다");
                return;
            }
            int prevSector = _playerCurrentSector;
            _playerCurrentSector = targetSector;
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

        _isPlayerHitThisTurn = false;

        if (attack.targetSectors != null && attack.targetSectors.Count > 0)
        {
            OnEnemyAttackSuccess?.Invoke(attack.targetSectors);
        }

        _isPlayerHitThisTurn = IsPlayerHitByAttack(attack);

        if (_isPlayerHitThisTurn)
        {
            
            DealDamageToPlayer(attack.damage);
        }
        else
        {
            Debug.Log($"[BattleSystem] 회피 성공! (플레이어 위치: 섹터 {_playerCurrentSector})");
        }
    }

    public void ProcessEnemyStone(int count = 1)
    {
        List<int> validSectors = new List<int>();
        for (int i = 1; i <= _totalSectors; i++)
        {
            if (i != _playerCurrentSector && !_stoneSectors.Contains(i))
            {
                validSectors.Add(i);
            }
        }
        int stonesToSpawn = Mathf.Min(count, validSectors.Count);
        for (int j = 0; j < stonesToSpawn; j++)
        {
            int randomIndex = Random.Range(0, validSectors.Count);
            int targetSector = validSectors[randomIndex];

            _stoneSectors.Add(targetSector);

            validSectors.RemoveAt(randomIndex);

            Debug.Log($"[BattleSystem] 적이 {targetSector}번 섹터에 돌을 던졌습니다");
        }
        if (count > 0)
        {
            OnStoneUpdated?.Invoke(_stoneSectors, true);
        }
    }

    public void ProcessEnemyWind(WindDirection actualDirection)
    {
        Debug.Log($"[BattleSystem] 바람 발생! 실제 방향: {actualDirection}");
        MoveDirection moveDir = ConvertWindToMoveDirection(actualDirection);
        int targetSector = GetWindTargetSector(_playerCurrentSector, actualDirection);
        if (targetSector != -1 && !IsSectorBlocked(targetSector))
        {
            int prevSector = _playerCurrentSector;
            _playerCurrentSector = targetSector;

            Debug.Log($"[BattleSystem] 바람에 의해 밀려남: Sector {prevSector} -> {_playerCurrentSector}");

            OnPlayerMoved?.Invoke(_playerCurrentSector, moveDir);
        }
        else
        {
            Debug.Log("[BattleSystem] 바람이 불었으나 장애물이나 벽에 막혀 이동하지 못했습니다.");
        }
    }


    // 적 돌진 공격 처리
    public void ProcessEnemyDash(EnemyDash dash)
    {
        if (dash == null) return;

        List<int> targetSectors = new List<int>();

        for (int col = 0; col < _columns; col++)
        {
            foreach (int row in dash.targetRows)
            {
                targetSectors.Add((row * _columns) + col + 1);
            }
        }

        // 돌진 공격 처리
        Debug.Log($"[BattleSystem] 적 돌진 공격! 대상 섹터: [{string.Join(", ", targetSectors)}]");
        //OnEnemyAttackSuccess?.Invoke(targetSectors);

        _isPlayerHitThisTurn = false;

        foreach (var enemy in _enemies)
        {
            OnEnemyDash?.Invoke(targetSectors, enemy.IsLeft);
        }

        _isPlayerHitThisTurn = targetSectors.Contains(_playerCurrentSector);

        if (_isPlayerHitThisTurn)
        {
            
            DealDamageToPlayer(dash.damage);
        }

        foreach (var enemy in _enemies)
        {
            enemy.ToggleSide();
            //OnEnemySideChanged?.Invoke(enemy.IsLeft);
        }
    }

    public List<int> GetMovableWindSectors(WindDirection direction)
    {
        List<int> movableSectors = new List<int>();
        for (int i = 1; i <= _totalSectors; i++)
        {
            int target = GetWindTargetSector(i, direction);
            if (target != -1 && !IsSectorBlocked(target))
            {
                movableSectors.Add(i);
            }
        }
        return movableSectors;
    }

    public int GetWindTargetSector(int fromSector, WindDirection direction)
    {
        int currentIndex = fromSector - 1;
        int curRow = currentIndex / _columns;
        int curCol = currentIndex % _columns;

        int targetRow = curRow;
        int targetCol = curCol;

        switch (direction)
        {
            case WindDirection.Up: targetRow -= 1; break;
            case WindDirection.Down: targetRow += 1; break;
            case WindDirection.Left: targetCol -= 1; break;
            case WindDirection.Right: targetCol += 1; break;
        }
        int rows = _totalSectors / _columns;
        if (targetRow >= 0 && targetRow < rows && targetCol >= 0 && targetCol < _columns)
        {
            return (targetRow * _columns) + targetCol + 1;
        }
        return -1;
    }

    private MoveDirection ConvertWindToMoveDirection(WindDirection windDir)
    {
        return windDir switch
        {
            WindDirection.Up => MoveDirection.Left,
            WindDirection.Down => MoveDirection.Right,
            WindDirection.Left => MoveDirection.Back,
            WindDirection.Right => MoveDirection.Front,
            _ => MoveDirection.None,
        };
    }

    public bool IsSectorBlocked(int sectorIndex)
    {
        return _stoneSectors.Contains(sectorIndex);
    }
    public void ClearStones()
    {
        if (_stoneSectors.Count > 0)
        {
            _stoneSectors.Clear();
            Debug.Log("모든 돌 소멸");
            OnStoneUpdated?.Invoke(_stoneSectors, false);
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

    public void SetGuard(bool state)
    {
        if(_isGuarding && !state)
        {
            Debug.Log("<color=blue>[BattleSystem] 플레이어 방어 해제!</color>");
            //
            OnChangePlayerAnim?.Invoke("7_2_ReleaseGuard");
        }
        else if (state)
        {
            Debug.Log("<color=blue>[BattleSystem] 플레이어 방어 태세!</color>");
        }

        _isGuarding = state;
        
    }

    public void Guard()
    {
        OnChangePlayerAnim?.Invoke("7_Guard");
    }

    public void Release_Guard()
    {
        //OnChangePlayerAnim?.Invoke("1_Idle");
    }
}

using UnityEngine;
using System.Collections.Generic;

public class BattleUIManager : MonoBehaviour
{
    [Header("플레이어 UI")]
    [SerializeField] private UnitStatusUI _playerStatusUI;

    [Header("적 UI 설정")]
    [SerializeField] private GameObject _enemyStatusPrefab;
    [SerializeField] private Transform _enemyUIContainer;

    private Dictionary<RuntimeEnemy, UnitStatusUI> _enemyUIMap = new Dictionary<RuntimeEnemy, UnitStatusUI>();

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.BattleSystem != null)
        {
            BattleSystem battle = GameManager.Instance.BattleSystem;

            battle.OnPlayerHPChanged += HandlePlayerHPChanged;
            battle.OnEnemyHPChanged += HandleEnemyHPChanged;
            // TODO: Enemy죽었을 때 UI처리 로직

            battle.OnBattleInitialized += HandleBattleInitialized;

        }
    }

    private void HandleBattleInitialized()
    {
        if (GameManager.Instance != null && GameManager.Instance.BattleSystem != null)
            InitializeUI(GameManager.Instance.BattleSystem);
    }

    public void InitializeUI(BattleSystem battle)
    {
        // 플레이어 UI초기화
        if (_playerStatusUI != null)
        {
            _playerStatusUI.Init("Player", battle.PlayerHP, battle.PlayerMaxHP);
            _playerStatusUI.UpdateHP(battle.PlayerHP, battle.PlayerMaxHP);
        }
        foreach (Transform child in _enemyUIContainer) Destroy(child.gameObject);
        _enemyUIMap.Clear();

        foreach (RuntimeEnemy enemy in battle.Enemies)
        {
            GameObject go = Instantiate(_enemyStatusPrefab, _enemyUIContainer);
            UnitStatusUI ui = go.GetComponent<UnitStatusUI>();

            if (ui != null)
            {
                ui.Init(enemy.Data.Enemy_Name, enemy.CurrentHP, enemy.MaxHP);
                _enemyUIMap.Add(enemy, ui);
            }
        }
    }

    private void HandlePlayerHPChanged(int current, int max)
    {
        if (_playerStatusUI != null)
        {
            _playerStatusUI.UpdateHP(current, max);
        }
    }

    private void HandleEnemyHPChanged(RuntimeEnemy enemy)
    {
        if (_enemyUIMap.TryGetValue(enemy, out UnitStatusUI ui))
        {
            ui.UpdateHP(enemy.CurrentHP, enemy.MaxHP);
        }
    }

}

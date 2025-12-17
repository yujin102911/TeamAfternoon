using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class BattleUIManager : MonoBehaviour
{
    [Header("스테이지 UI")]
    [SerializeField] private Button _startButton;
    [SerializeField] private GameObject _sectorSelectionPanel;
    [SerializeField] private TextMeshProUGUI _pageText;
    [SerializeField] private TextMeshProUGUI _totalPageText;
    [SerializeField] private TextMeshProUGUI _chapterText;

    [Header("정화 게이지 UI")]
    [SerializeField]
    private UnitStatusUI _cureUI;

    [Header("플레이어 UI")]
    [SerializeField] private PlayerHeartUI _playerStatusUI;

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
            battle.OnEnemyDied += HandleEnemyDied;
            battle.OnBattleInitialized += HandleBattleInitialized;
            battle.UpdateCureGauage += HandleCureChanged;

            GameManager.Instance.OnGameStateChanged += RefreshStartButtonState;
            GameManager.Instance.OnRoundChanged += HandleRoundChanged;
            GameManager.Instance.OnBattleEnded += RefreshStartButtonState;
            GameManager.Instance.OnGameStateChanged += TryBindPlayerUI;
        }
        RefreshStartButtonState();
        //RefreshSectorSelectionPanel();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.BattleSystem != null)
        {
            BattleSystem battle = GameManager.Instance.BattleSystem;
            GameManager.Instance.OnGameStateChanged -= RefreshStartButtonState;
            GameManager.Instance.OnRoundChanged -= HandleRoundChanged;
            GameManager.Instance.OnGameStateChanged -= TryBindPlayerUI;
            battle.OnPlayerHPChanged -= HandlePlayerHPChanged;
            battle.OnEnemyHPChanged -= HandleEnemyHPChanged;
            battle.OnEnemyDied -= HandleEnemyDied;
            battle.OnBattleInitialized -= HandleBattleInitialized;
            battle.UpdateCureGauage -= HandleCureChanged;
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
            _playerStatusUI.Init(battle.PlayerHP, battle.PlayerMaxHP);
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

    private void HandleCureChanged(int current, int max)
    {
        if (_cureUI != null)
        {
            _cureUI.UpdateHP(current, max);
        }
    }

    private void HandlePlayerHPChanged(int current, int max)
    {
        if (_playerStatusUI != null)
        {
            _playerStatusUI.UpdateHearts(current, max);
        }
    }

    private void HandleEnemyHPChanged(RuntimeEnemy enemy)
    {
        if (_enemyUIMap.TryGetValue(enemy, out UnitStatusUI ui))
        {
            ui.UpdateHP(enemy.CurrentHP, enemy.MaxHP);
        }
    }

    private void HandleEnemyDied(RuntimeEnemy enemy)
    {
        if (_enemyUIMap.TryGetValue(enemy, out UnitStatusUI ui))
        {
            if (ui != null) 
                Destroy(ui.gameObject);
            _enemyUIMap.Remove(enemy);
        }
    }

    private void HandleRoundChanged(int chapter, int page, int totalPage)
    {
        _pageText.text = $"Page\n<size=56pt>{0+page.ToString()}";
        _totalPageText.text = $"/{totalPage}";
        _chapterText.text = $"Chapter {chapter.ToString()}.";

    }
    private void RefreshStartButtonState()
    {
        if (_startButton == null || GameManager.Instance == null) return;
        bool isRoundRunning = GameManager.Instance.IsExecutingRound;
        bool isSectorSelected = GameManager.Instance.IsSectorSelected;
        bool isGameOver = GameManager.Instance.IsBattleEnded;

        bool interactable = !isRoundRunning && isSectorSelected && !isGameOver;

        _startButton.interactable = interactable;

        //RefreshSectorSelectionPanel();
    }

    private void RefreshStartButtonState(bool isVictory, int a, int b, int c, int d)
    {
        _startButton.interactable = false;
    }

    private void TryBindPlayerUI()
    {
        var visualController = FindAnyObjectByType<PlayerVisualController>();

        if (visualController != null && _playerStatusUI != null)
        {
            Transform playerTr = visualController.CurrentPlayerTransform;
            if (playerTr != null)
            {
                _playerStatusUI.SetFollowTarget(playerTr);
            }
        } 
    }

}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// 체력 관련 UI를 관리하는 스크립트
/// </summary>
public class BattleHUDController : MonoBehaviour
{
    [Header("플레이어 체력 (하트)")]
    [SerializeField] private GameObject _fullHeartPrefab;
    [SerializeField] private GameObject _emptyHeartPrefab;
    [SerializeField] private Transform _heartContainer;

    [Header("적 체력 (슬라이더/텍스트)")]
    [SerializeField] private Slider _enemySlider;
    [SerializeField] private TextMeshProUGUI _enemyHPText;

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.BattleSystem != null)
        {
            BattleSystem bs = GameManager.Instance.BattleSystem;
            bs.OnBattleInitialized += InitializeHUD;
            bs.OnPlayerHPChanged += UpdatePlayerHearts;
            bs.OnEnemyHPChanged += UpdateEnemyHPSlider;

            InitializeHUD();
        }
    }
    private void InitializeHUD()
    {
        BattleSystem bs = GameManager.Instance.BattleSystem;
        UpdatePlayerHearts(bs.PlayerHP, bs.PlayerMaxHP);
        UpdateEnemyHPSlider(bs.EnemyHP, bs.EnemyMaxHP);
    }
    private void UpdatePlayerHearts(int currentHP, int maxHP)
    {
        foreach (Transform child in _heartContainer)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < currentHP; i++)
            Instantiate(_fullHeartPrefab, _heartContainer);
        int emptyCount = maxHP - currentHP;
        for (int i = 0; i < emptyCount; i++)
            Instantiate(_emptyHeartPrefab, _heartContainer);
    }
    private void UpdateEnemyHPSlider(int currentHP, int maxHP)
    {
        if ( _enemySlider != null)
        {
            _enemySlider.maxValue = maxHP;
            _enemySlider.value = currentHP;
        }
        if (_enemyHPText != null)
        {
            _enemyHPText.text = $"{currentHP}/{maxHP}";
        }
    }

}

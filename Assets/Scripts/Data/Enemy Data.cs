using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New EnemyData", menuName = "Data/Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("적 최대 체력")]
    [SerializeField]
    private int _maxEnemyHp = 100;

    [Header("적 이름")]
    [SerializeField]
    private string _enemyName = "Enemy";

    [Header("적 이미지")]
    [SerializeField]
    private Sprite _enemySprite;

    [Header("일반 패턴")]
    [SerializeField]
    private List<EnemyPattern> _nomalPatterns = new List<EnemyPattern>();
    [Header("특수 패턴")]
    [SerializeField]
    private List<EnemyPattern> _specialPatterns = new List<EnemyPattern>();
    [Tooltip("특수패턴 발동 hp")]
    [SerializeField]
    private int[] _specialPatternThreshold;
    public int SpecialPatternIndex = 0;

    [Header("적 능력")]
    [SerializeField]
    private List<EnemyAbility> _enemyAbilities = new List<EnemyAbility>();

    public int Max_EnemyHp => _maxEnemyHp;
    public string Enemy_Name => _enemyName;
    public List<EnemyPattern> Nomal_Patterns => _nomalPatterns;
    public List<EnemyPattern> Special_Patterns => _specialPatterns;
    public Sprite Enemy_Sprite => _enemySprite;
    public List<EnemyAbility> Enemy_Abilities => _enemyAbilities;

    /// <summary>
    /// 현재 체력에 따른 패턴 반환
    /// </summary>
    public EnemyPattern GetPattern(int current_hp)
    {
        Debug.Log($"Current HP: {current_hp}, Threshold: {_specialPatternThreshold}, Special Pattern Index: {SpecialPatternIndex}");

        if (SpecialPatternIndex < _specialPatterns.Count)
        {
            // 특수 패턴 발동 조건 충족 시 특수 패턴 반환
            if (current_hp > 0
            && current_hp < _specialPatternThreshold[SpecialPatternIndex])
            {
                // 추후 그로기나 이미지 속도 조절 필요
                Debug.Log("Special Pattern Activated");

                if (SpecialPatternIndex == 0)
                {
                    Debug.Log("First Special Pattern Activated");
                }
                else if (SpecialPatternIndex == 1)
                {
                    Debug.Log("Second Special Pattern Activated");
                }

                return _specialPatterns[SpecialPatternIndex++ % _specialPatterns.Count];
            }
            else
            {
                Debug.Log("Nomal Pattern Activated");
                return _nomalPatterns[Random.Range(0, _nomalPatterns.Count)];
            }

        }
        else
        {
            Debug.Log("Nomal Pattern Activated");
            return _nomalPatterns[Random.Range(0, _nomalPatterns.Count)];
        }
    }
}

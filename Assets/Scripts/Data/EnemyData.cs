using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyPhaseGroup
{
    public string PhaseName;
    public List<EnemyPattern> Patterns = new List<EnemyPattern>();
    public bool IsLoop = false;
}

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

    [Header("페이즈 별 패턴 구성")]
    [Tooltip("Index0 : 1페이즈, Index1: 2페이즈 ...")]
    public List<EnemyPhaseGroup> PhaseGroups = new List<EnemyPhaseGroup>();

    [Header("비주얼 설정")]
    [SerializeField]
    private Color _assignedColor = Color.white; // 기본값 흰색

    [Header("적 능력")]
    [SerializeField]
    private List<EnemyAbility> _enemyAbilities = new List<EnemyAbility>();

    public int Max_EnemyHp => _maxEnemyHp;
    public string Enemy_Name => _enemyName;
    public Sprite Enemy_Sprite => _enemySprite;
    public List<EnemyAbility> Enemy_Abilities => _enemyAbilities;
    public Color AssignedColor => _assignedColor;

}

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
    [Header("적 정보")]
    [SerializeField]
    private string _enemyName = "Enemy";
    [SerializeField]
    private int _maxCureValue = 50;

    [Header("적 이미지")]
    [Tooltip("왼쪽 적 스프라이트")]
    [SerializeField]
    private List<Sprite> _leftEnemySprites = new List<Sprite>();
    [Tooltip("오른쪽 적 스프라이트")]
    [SerializeField]
    private List<Sprite> _rightEnemySprites = new List<Sprite>();

    [Header("적 이미지")]
    [SerializeField]
    private Sprite _bgSprite;

    [Header("패턴 리스트 (순서대로 반복)")]
    [SerializeField]
    private List<EnemyPattern> _patterns = new List<EnemyPattern>();

    [Header("비주얼 설정")]
    [SerializeField]
    private Color _assignedColor = Color.white; // 기본값 흰색

    [Header("적 능력")]
    [SerializeField]
    private List<EnemyAbility> _enemyAbilities = new List<EnemyAbility>();

    public string Enemy_Name => _enemyName;
    public List<EnemyPattern> Patterns => _patterns;
    public int MaxCureValue => _maxCureValue;
    public List<Sprite> LeftEnemySprites => _leftEnemySprites;
    public List<Sprite > RightEnemySprites => _rightEnemySprites;
    public Sprite BackGroundSprite => _bgSprite;
    public List<EnemyAbility> Enemy_Abilities => _enemyAbilities;
    public Color AssignedColor => _assignedColor;

}

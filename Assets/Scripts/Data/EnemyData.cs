using System.Collections.Generic;
using UnityEngine;

public enum PatternSelectionType
{
    Sequential,      // 순차
    Random,          // 랜덤
}

[CreateAssetMenu(fileName = "New EnemyData", menuName = "Data/Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("적 정보")]
    [SerializeField]
    private string _enemyName = "Enemy";
    [SerializeField]
    private int _maxHP = 50;

    [Header("HP바 오프셋")]
    [SerializeField]
    private Vector2 _hpBarOffset = new Vector2(0, -150.0f);

    [Header("적 프리팹")]
    [SerializeField]
    private GameObject _enemyPrefab;

    [Header("적 배경 프리펩")]
    [SerializeField]
    private GameObject _enemyBackPrefab;

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

    [Header("패턴")]
    [SerializeField]
    private PatternSelectionType _selectionType = PatternSelectionType.Random;
    [SerializeField]
    private List<EnemyPattern> _patterns = new List<EnemyPattern>();

    [Header("비주얼 설정")]
    [SerializeField]
    private Color _assignedColor = new Color(1, 1, 1, 0); // 기본값 투명

    [Header("적 능력")]
    [SerializeField]
    private List<EnemyAbility> _enemyAbilities = new List<EnemyAbility>();

    public GameObject EnemyPrefab => _enemyPrefab;
    public GameObject EnemyBackPrefab => _enemyBackPrefab;
    public string Enemy_Name => _enemyName;
    public List<EnemyPattern> Patterns => _patterns;
    public int MaxHP => _maxHP;
    public List<Sprite> LeftEnemySprites => _leftEnemySprites;
    public List<Sprite > RightEnemySprites => _rightEnemySprites;
    public Sprite BackGroundSprite => _bgSprite;
    public List<EnemyAbility> Enemy_Abilities => _enemyAbilities;
    public Color AssignedColor => _assignedColor;
    public PatternSelectionType SelectionType => _selectionType;

    public Vector2 HPBarOffset => _hpBarOffset;

}

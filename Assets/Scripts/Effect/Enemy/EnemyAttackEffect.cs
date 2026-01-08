using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackEffect : MonoBehaviour, IEffectPoolOwner
{
    [Header("Effect")]
    public GameObject attackFxPrefab;
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField]
    private Vector3 _offset = Vector3.zero;

    private readonly Queue<GameObject> _effectPool = new Queue<GameObject>();

    private List<Vector3> _worldSectorPos = new List<Vector3>();
    private List<int> _targetSectors = new List<int>();

    private Transform effectRoot;

    private void Awake()
    {
        //InitializePool();
    }

    private void Start()
    {
        if(EffectContainer.Instance != null)
        {
            effectRoot = EffectContainer.Instance._enemyEffectArea;
        }

        InitializePool();
    }

    #region Data Setters
    public void Set_worldSectorPos(List<Vector3> vector3s)
    {
        _worldSectorPos.Clear();
        _worldSectorPos.AddRange(vector3s);
    }

    public void Set_targetSectors(List<int> ints)
    {
        _targetSectors.Clear();
        _targetSectors.AddRange(ints);
    }
    #endregion

    public void SpawnAttackEffect()
    {
        Debug.Log($"[EnemyAttackEffect] 적 공격! 대상 섹터: [{string.Join(", ", _targetSectors)}]");

        foreach (var index in _targetSectors)
        {
            if (index - 1 < 0 || index - 1 >= _worldSectorPos.Count)
                continue;

            GameObject fx = GetEffect();
            fx.transform.SetParent(effectRoot, true); // ⭐ 월드 기준 유지
            fx.transform.position = _worldSectorPos[index - 1] + _offset;
            fx.SetActive(true);
        }

        if (GameManager.Instance != null)
            GameManager.Instance.BattleSystem.PlayerTakeDamage();
    }

    public void SpawnStoneAnim()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.BattleSystem.SpawnStone();
    }

    #region Pool
    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject fx = CreateNewEffect();
            fx.SetActive(false);
            _effectPool.Enqueue(fx);
        }
    }

    private GameObject CreateNewEffect()
    {
        GameObject fx = Instantiate(attackFxPrefab, effectRoot);
        //Vector3 parentScale = transform.lossyScale;
        //fx.transform.localScale = new Vector3(
        //    fx.transform.localScale.x / parentScale.x,
        //    fx.transform.localScale.y / parentScale.y,
        //    fx.transform.localScale.z / parentScale.z
        //);
        var autoReturn = fx.GetComponent<EffectAutoReturn>();
        autoReturn.Init(this);
        return fx;
    }

    private GameObject GetEffect()
    {
        if (_effectPool.Count > 0)
            return _effectPool.Dequeue();

        // 부족하면 확장
        return CreateNewEffect();
    }

    public void ReturnEffect(GameObject fx)
    {
        fx.SetActive(false);
        fx.transform.SetParent(effectRoot, true);
        _effectPool.Enqueue(fx);
    }
    #endregion
}

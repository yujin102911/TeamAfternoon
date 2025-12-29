using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackEffect : MonoBehaviour, IEffectPoolOwner
{
    [Header("Effect")]
    public GameObject attackFxPrefab;
    [SerializeField] private int initialPoolSize = 10;

    private readonly Queue<GameObject> _effectPool = new Queue<GameObject>();

    private List<Vector3> _worldSectorPos = new List<Vector3>();
    private List<int> _targetSectors = new List<int>();

    private void Awake()
    {
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
            fx.transform.position = _worldSectorPos[index - 1];
            fx.SetActive(true);
        }
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
        GameObject fx = Instantiate(attackFxPrefab, transform);
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
        fx.transform.SetParent(transform);
        _effectPool.Enqueue(fx);
    }
    #endregion
}

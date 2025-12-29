using System.Collections.Generic;
using UnityEngine;

public class EffectPool : MonoBehaviour, IEffectPoolOwner
{
    [SerializeField] private GameObject prefab;
    private Queue<GameObject> _effectPool = new();
    [SerializeField] 
    private int initialPoolSize = 10;

    private void Awake()
    {
        InitializePool();
    }

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
        GameObject fx = Instantiate(prefab, transform);
        
        fx.tag = "Effect";

        Vector3 parentScale = transform.lossyScale;
        fx.transform.localScale = new Vector3(
            fx.transform.localScale.x / parentScale.x,
            fx.transform.localScale.y / parentScale.y,
            fx.transform.localScale.z / parentScale.z
        );
        var autoReturn = fx.GetComponent<EffectAutoReturn>();
        autoReturn.Init(this);
        return fx;
    }

    public GameObject GetEffect()
    {
        if (_effectPool.Count > 0)
            return _effectPool.Dequeue();

        // 부족하면 확장
        return CreateNewEffect();
    }

    public void ReturnEffect(GameObject fx)
    {
        fx.SetActive(false);
        fx.transform.SetParent(transform, true);
        _effectPool.Enqueue(fx);
    }
}

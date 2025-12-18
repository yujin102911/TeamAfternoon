using System.Collections.Generic;
using UnityEngine;

public class UIEffectPool : MonoBehaviour
{
    [SerializeField] private UIPooledEffect prefab;
    [SerializeField] private int initialCount = 10;
    [SerializeField] private RectTransform parent;

    private Queue<UIPooledEffect> _pool = new Queue<UIPooledEffect>();

    private void Awake()
    {
        for (int i = 0; i < initialCount; i++)
        {
            CreateNew();
        }
    }

    private UIPooledEffect CreateNew()
    {
        UIPooledEffect effect = Instantiate(prefab, parent);
        effect.gameObject.SetActive(false);
        effect.Init(this);
        _pool.Enqueue(effect);
        return effect;
    }

    public UIPooledEffect Get()
    {
        if (_pool.Count == 0)
            CreateNew();

        return _pool.Dequeue();
    }

    public void Return(UIPooledEffect effect)
    {
        _pool.Enqueue(effect);
    }
}

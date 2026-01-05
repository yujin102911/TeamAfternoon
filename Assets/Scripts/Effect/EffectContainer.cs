using UnityEngine;

public class EffectContainer : MonoBehaviour
{
    public static EffectContainer Instance { get; private set; }

    public Transform _enemyEffectArea;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
}

using UnityEngine;

public class PlayerAttectEffect : MonoBehaviour
{
    [SerializeField] private EffectPool swordAttackFX_pool;
    [SerializeField] private EffectPool bowAttackFX_pool;
    [SerializeField] private EffectPool bowChargingFX_pool;

    [SerializeField] private Vector3 attackOffset;
    [SerializeField] private Vector3 chargingOffset;


    public void SpawnSwordAttackEffect()
    {
        var fx = swordAttackFX_pool.GetEffect();
        fx.transform.position = attackOffset;
        fx.SetActive(true);
    }

    public void SpawnBowAttackEffect()
    {
        var fx = bowAttackFX_pool.GetEffect();
        fx.transform.position = attackOffset;
        fx.SetActive(true);
    }

    public void SpawnBowChargingEffect()
    {
        var fx = bowChargingFX_pool.GetEffect();
        fx.transform.position = transform.position + chargingOffset;
        fx.SetActive(true);
    }
}

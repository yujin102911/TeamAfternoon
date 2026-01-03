using UnityEngine;

public class PlayerAttectEffect : MonoBehaviour
{
    [SerializeField] private EffectPool swordAttackFX_pool;
    [SerializeField] private EffectPool big_swordAttackFX_pool;
    [SerializeField] private EffectPool bowAttackFX_pool;
    [SerializeField] private EffectPool bowChargingFX_pool;

    [SerializeField] private Vector3 attackOffset;
    [SerializeField] private Vector3 chargingOffset;

    private Animator _currentAnimtor;

    public void SpawnSwordAttackEffect()
    {
        var fx = swordAttackFX_pool.GetEffect();
        fx.transform.position = attackOffset;
        fx.SetActive(true);
    }

    public void Spawn_BigSwordAttackEffect()
    {
        var fx = big_swordAttackFX_pool.GetEffect();
        fx.transform.position = attackOffset;
        fx.SetActive(true);
    }

    public void SpawnBowAttackEffect()
    {
        var fx = bowAttackFX_pool.GetEffect();
        fx.transform.position = attackOffset;
        fx.SetActive(true);

        if (_currentAnimtor != null)
        {
            _currentAnimtor.SetTrigger("Shoot");
            _currentAnimtor = null;
        }
    }

    public void SpawnBowChargingEffect()
    {
        if(_currentAnimtor != null)
        {
            _currentAnimtor.SetTrigger("Charging_Lv3");

        }
        else
        {
            var fx = bowChargingFX_pool.GetEffect();
            //Vector3 worldPos = transform.TransformPoint(transform.position);
            fx.transform.position = transform.position + chargingOffset;
            fx.SetActive(true);
            _currentAnimtor = fx.GetComponent<Animator>();
        }

            
    }
}

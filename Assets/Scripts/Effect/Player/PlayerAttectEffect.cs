using UnityEngine;

public class PlayerAttectEffect : MonoBehaviour
{
    [SerializeField] private EffectPool swordAttackFX_pool;
    [SerializeField] private EffectPool big_swordAttackFX_pool;
    [SerializeField] private EffectPool bowAttackFX_pool;
    [SerializeField] private EffectPool bowChargingFX_pool;

    [SerializeField] private Vector3 attackOffset;
    [SerializeField] private Vector3 _leftOffset;
    [SerializeField]
    private Vector3 _rightOffset;
    
    [SerializeField] private Vector3 charging_rightOffset;
    [SerializeField] private Vector3 charging_leftOffset;

    private Animator _currentAnimtor;
    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    private void Flip_setPos(GameObject fx)
    {
        // true면 좌측 방향
        if(_sr.flipX)
        {
            fx.transform.position = _leftOffset;
            fx.GetComponent<EffectAutoReturn>().Flip();
        }
        else
        {
            fx.transform.position = _rightOffset;
        }

        fx.SetActive(true);
    }

    public void SpawnSwordAttackEffect()
    {
        var fx = swordAttackFX_pool.GetEffect();
        Flip_setPos(fx);
        //fx.transform.position = attackOffset;
        //fx.SetActive(true);

        if (GameManager.Instance != null)
            GameManager.Instance.BattleSystem.EnemyTakeDamage();
    }

    public void Spawn_BigSwordAttackEffect()
    {
        var fx = big_swordAttackFX_pool.GetEffect();
        Flip_setPos(fx);
        //fx.transform.position = attackOffset;
        //fx.SetActive(true);

        if (GameManager.Instance != null)
            GameManager.Instance.BattleSystem.EnemyTakeDamage();
    }

    public void SpawnBowAttackEffect()
    {
        var fx = bowAttackFX_pool.GetEffect();
        Flip_setPos(fx);

        if (_currentAnimtor != null)
        {
            _currentAnimtor.SetTrigger("Shoot");
            _currentAnimtor = null;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.BattleSystem.EnemyTakeDamage();
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
            // true면 좌측 방향
            if (_sr.flipX)
            {
                fx.transform.position = transform.position + charging_leftOffset;
                fx.GetComponent<EffectAutoReturn>().Flip();
            }
            else
            {
                fx.transform.position = transform.position + charging_rightOffset;
            }

            fx.SetActive(true);

            //fx.transform.position = 
            //fx.SetActive(true);
            _currentAnimtor = fx.GetComponent<Animator>();
        } 
    }

    public void ReturnEffects()
    {
        swordAttackFX_pool.ReturnAllEffects();
        big_swordAttackFX_pool.ReturnAllEffects();
        bowAttackFX_pool.ReturnAllEffects();
        bowChargingFX_pool.ReturnAllEffects();
    }

    public void Flip_currentCharge()
    {
        if (_currentAnimtor == null) return;

        // true면 좌측 방향
        if (_sr.flipX)
        {
            _currentAnimtor.transform.position = transform.position + charging_leftOffset;
        }
        else
        {
            _currentAnimtor.transform.position = transform.position + charging_rightOffset;
        }
    }
}

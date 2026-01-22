using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class PlayerAttectEffect : MonoBehaviour
{
    [Header("그림자")]
    [SerializeField]
    private GameObject _leftShadow;
    [SerializeField]
    private GameObject _rightShadow;

    [Header("이펙트 위치")]
    [SerializeField] 
    private Vector3 attackOffset;
    [SerializeField] 
    private Vector3 _leftOffset;
    [SerializeField]
    private Vector3 _rightOffset;

    [TabGroup("Sword")]
    [SerializeField] private EffectPool swordAttackFX_pool;
    [TabGroup("Sword")]
    [SerializeField] private EffectPool swordEnfoceFX_pool;
    [TabGroup("Sword")]
    [SerializeField] private EffectPool big_swordAttackFX_pool;
    [TabGroup("Sword")]
    [SerializeField] private EffectPool Enfocebig_swordAttackFX_pool;

    [TabGroup("Bow")]
    [SerializeField] private EffectPool bowAttackFX_pool;
    [TabGroup("Bow")]
    [SerializeField] private EffectPool Enforce_bowAttackFX_pool;

    [TabGroup("Bow_charging")]
    [SerializeField] private EffectPool bowChargingFX_pool;
    [TabGroup("Bow_charging")]
    [SerializeField] private Vector3 charging_rightOffset;
    [TabGroup("Bow_charging")]
    [SerializeField] private Vector3 charging_leftOffset;

    [TabGroup("Guard")]
    [SerializeField] private EffectPool guard_pool;
    [TabGroup("Guard")]
    [SerializeField] private Vector3 guard_rightOffset;
    [TabGroup("Guard")]
    [SerializeField] private Vector3 guard_leftOffset;

    private Animator _currentAnimtor;
    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();

        _leftShadow.SetActive(true);
        _rightShadow.SetActive(false);
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
    public void SpawnSwordEffect(int Pool_ID)
    {
        GameObject fx = null;

        switch (Pool_ID)
        {
            //일반 공격
            case 0:
                fx = swordAttackFX_pool.GetEffect();

                if (SoundManager.Instance != null)
                    SoundManager.Instance.Play(SoundID.Player_Sword);
                break;
            //일반 공격 + 단데증
            case 1:
                fx = swordEnfoceFX_pool.GetEffect();

                if (SoundManager.Instance != null)
                    SoundManager.Instance.Play(SoundID.Player_Sword);
                break;
            //차징
            case 2:
                fx = big_swordAttackFX_pool.GetEffect();

                if (SoundManager.Instance != null)
                    SoundManager.Instance.Play(SoundID.Player_Sword2);
                break;
            //차징 + 단데증
            case 3:
                fx = Enfocebig_swordAttackFX_pool.GetEffect();

                if (SoundManager.Instance != null)
                    SoundManager.Instance.Play(SoundID.Player_Sword2);

                break;
            default:
                break;
        }

        if(fx != null)
            Flip_setPos(fx);

        //타격 연출
        //StartCoroutine(HitStop(0.05f));
        //CameraShake.Instance.Shake(0.05f, 0.12f);

        if (GameManager.Instance != null)
            GameManager.Instance.BattleSystem.EnemyTakeDamage();
    }

    public void SpawnBowEffect(int Pool_ID)
    {
        GameObject fx = null;

        switch (Pool_ID)
        {
            case 0:
                fx = bowAttackFX_pool.GetEffect();

                if (SoundManager.Instance != null)
                    SoundManager.Instance.Play(SoundID.Player_Bow);

                break;
            case 1:
                fx = Enforce_bowAttackFX_pool.GetEffect();

                if (SoundManager.Instance != null)
                    SoundManager.Instance.Play(SoundID.Player_Bow);
                //SoundManager.Instance.Play(SoundID.Player_Bow2);

                break;
            default:
                break;
        }

        if (fx != null)
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

        if (SoundManager.Instance != null)
            SoundManager.Instance.Play(SoundID.Player_Bow3);
    }

    // 가드 이펙트
    public void GuardEffect()
    {
        var fx = guard_pool.GetEffect();

        // true면 좌측 방향
        if (_sr.flipX)
        {
            fx.transform.position = transform.position + guard_leftOffset;
            fx.GetComponent<EffectAutoReturn>().Flip();
        }
        else
        {
            fx.transform.position = transform.position + guard_rightOffset;
        }

        fx.SetActive(true);

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

    public void Flip_shadow()
    {
        _leftShadow.SetActive(!_sr.flipX);
        _rightShadow.SetActive(_sr.flipX);
    }
}

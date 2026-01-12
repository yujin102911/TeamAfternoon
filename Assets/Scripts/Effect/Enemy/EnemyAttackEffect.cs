using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackEffect : MonoBehaviour, IEffectPoolOwner
{
    [Header("Sturn")]
    public GameObject SturnFx_Prefab;
    public GameObject Second_StarPrefab;

    [Header("Effect")]
    public GameObject attackFxPrefab;
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField]
    private Vector3 _offset = Vector3.zero;

    private readonly Queue<GameObject> _effectPool = new Queue<GameObject>();

    private List<Vector3> _worldSectorPos = new List<Vector3>();
    private List<int> _targetSectors = new List<int>();
    private int _currentStage = 0;

    private Transform effectRoot;
    private SpriteRenderer _sr;

    private void Awake()
    {
        //InitializePool();
        _sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if(EffectContainer.Instance != null)
        {
            effectRoot = EffectContainer.Instance._enemyEffectArea;
        }

        Hide_Star();

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

    public void Set_StageNum(int num)
    {
        _currentStage = num;
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
            
            if (_sr.flipX)
            {
                fx.GetComponent<EffectAutoReturn>().Flip();
            }

            fx.SetActive(true);
        }

        Choose_EffectSound(_currentStage);

        if (GameManager.Instance != null)
            GameManager.Instance.BattleSystem.PlayerTakeDamage();
    }

    public void SpawnStoneAnim()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.BattleSystem.SpawnStone();

        if (SoundManager.Instance != null)
            SoundManager.Instance.Play(SoundID.Stone);
    }

    public void Show_Star()
    {
        SturnFx_Prefab.SetActive(true);
    }

    public void Show_Star_Flip()
    {
        if(_sr.flipX)
        {
            SturnFx_Prefab.SetActive(true);
        }
        else
        {
            Second_StarPrefab.SetActive(true);
        }
    }

    public void Hide_Star()
    {
        if (SturnFx_Prefab != null)
            SturnFx_Prefab.SetActive(false);

        if(Second_StarPrefab != null)
            Second_StarPrefab.SetActive(false);
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
        if(_effectPool.Count > 0)
        {
            GameObject fx = _effectPool.Dequeue();
            fx.GetComponent<EffectAutoReturn>().ResetState();
            return fx;
        }

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

    private void Choose_EffectSound(int stage_num)
    {
        if (SoundManager.Instance == null) return;

        switch (stage_num - 1)
        {
            case 0:
                SoundManager.Instance.Play(SoundID.EnemyEffect_0);
                break;
            case 1:
                SoundManager.Instance.Play(SoundID.EnemyEffect_1);
                break;
            case 2:
                SoundManager.Instance.Play(SoundID.EnemyEffect_2);
                break;
            case 3:
                SoundManager.Instance.Play(SoundID.EnemyEffect_3);
                break;
            case 4:
                SoundManager.Instance.Play(SoundID.EnemyEffect_4);
                break;
            case 5:
                SoundManager.Instance.Play(SoundID.EnemyEffect_5);
                break;
        }
    }
}

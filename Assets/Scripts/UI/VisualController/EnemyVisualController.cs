using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyVisualController : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private Transform _enemyContainer;

    [Header("정렬 설정")]
    [SerializeField] private float _spacing = 2.5f;

    public GameObject textPrefab; // 생성될 프리팹
    public GameObject CritTextPrefab; // 크리티컬 생성될 프리팹
    public Vector3 Offset;

    [Header("점멸 설정")]
    public float blinkDuration = 0.3f;
    public Color EnemyHitColor = Color.red;

    [Header("텍스트 설정")]
    public float textLifetime = 1.0f;
    public float floatUpSpeed = 1.0f;

    // 적 애니메이터
    private Animator _enemyAnimator;
    private EnemyAttackEffect _enemyAttackEffect;
    private EnemyVisual visual;

    private string _currentAnimation = "";

    private Dictionary<RuntimeEnemy, EnemyVisual> _visualMap = new Dictionary<RuntimeEnemy, EnemyVisual>();
    private List<RuntimeEnemy> _currentEnemies = new List<RuntimeEnemy>();
    private BattleSystem _battleSystem;

    private void Awake()
    {

    }

    public void Initialize(BattleSystem battleSystem)
    {
        _battleSystem = battleSystem;

        _battleSystem.OnBattleInitialized += CreateEnemies;
    }

    private void OnDestroy()
    {
        if (_battleSystem != null)
        {
            _battleSystem.OnBattleInitialized -= CreateEnemies;
        }
    }

    private void CreateEnemies()
    {
        // 기존 적 제거
        //foreach (EnemyVisual visual  in _visualMap.Values)
        //{
        //    if (visual != null) Destroy(visual.gameObject);
        //}
        //_visualMap.Clear();

        //foreach (RuntimeEnemy enemy in _battleSystem.Enemies)
        //{
        //    GameObject obj = Instantiate(_enemyPrefab, _enemyContainer);
        //    EnemyVisual visual = obj.GetComponent<EnemyVisual>();

        //    if (visual != null)
        //    {
        //        visual.Initialize(enemy);
        //        _visualMap.Add(enemy, visual);
        //    }
        //}

        for (int i = _enemyContainer.childCount - 1; i >= 0; i--)
        {
            if (_enemyContainer.GetChild(i) != null) Destroy(_enemyContainer.GetChild(i).gameObject);
        }

        foreach (RuntimeEnemy enemy in _battleSystem.Enemies)
        {
            GameObject obj = Instantiate(enemy.Data.EnemyPrefab, _enemyContainer);
            _enemyAnimator = obj.GetComponent<Animator>();
            _enemyAttackEffect = obj.GetComponent<EnemyAttackEffect>();

            visual = obj.GetComponent<EnemyVisual>();

            // 맵 전체 좌표정보 전달
            if (GameManager.Instance.MapSystem != null)
                _enemyAttackEffect.Set_worldSectorPos(GameManager.Instance.MapSystem.GetSectorsPosition());
        }

        //AlignEnemies();

    }

    //private void AlignEnemies()
    //{
    //    int activeCount = 0;
    //    foreach (RuntimeEnemy enemy in _battleSystem.Enemies)
    //    {
    //        if (!enemy.IsDead) activeCount++;
    //    }

    //    if (activeCount == 0) return;

    //    float totalWidth = (activeCount - 1) * _spacing;

    //    float startX = -totalWidth / 2f;

    //    int currentIndex = 0;
    //    foreach (RuntimeEnemy enemy in _battleSystem.Enemies)
    //    {
    //        if (enemy.IsDead) continue;

    //        if (_visualMap.TryGetValue(enemy, out EnemyVisual visual))
    //        {
    //            // 현재 위치 = 시작점 + (순서 * 간격)
    //            float xPos = startX + (currentIndex * _spacing);

    //            visual.transform.localPosition = new Vector3(xPos, 0, 0);
    //            currentIndex++;
    //        }
    //    }
    //}

    //private void HandleEnemyHit(RuntimeEnemy enemy)
    //{
    //    if (_visualMap.TryGetValue(enemy, out EnemyVisual visual))
    //        visual.PlayerHitAnimation();

    //    StartCoroutine(ShowEnemyDamage(_battleSystem.TotalDamage));
    //}

    private void HandleEnemyDead(RuntimeEnemy enemy)
    {
        if (_visualMap.TryGetValue(enemy, out EnemyVisual visual))
        {
            visual.EnemyDeadColor(enemy);
        }
    }
    public void ChangeAnim(string animation)
    {
        if (_enemyAnimator != null)
        {
            _currentAnimation = animation;
            _enemyAnimator.CrossFade(animation, 0.2f);
        }
    }

    public void Play_EnemyIdle()
    {
        if (_enemyAnimator != null)
            _enemyAnimator.enabled = true;
    }

    public void Stop_EnemyIdle()
    {
        if (_enemyAnimator != null)
            _enemyAnimator.enabled = false;
    }

    public void PlayDamage(int damage, bool is_crit)
    {
        ChangeAnim("Hurt");

        StartCoroutine(ShowEnemyDamage(damage, is_crit));
    }

    public void PlayEnemyAttack(List<int> targets)
    {
        if (_enemyAttackEffect != null)
            _enemyAttackEffect.Set_targetSectors(targets);

        ChangeAnim("Attack");
    }

    public IEnumerator ShowEnemyDamage(int damage, bool is_crit)
    {
        SpawnDamageText(damage, is_crit);

        yield return new WaitForSeconds(blinkDuration);
    }

    private void SpawnDamageText(int damage, bool is_crit)
    {
        if (textPrefab == null) return;
        
        GameObject go = null;

        if (is_crit)
        {
            go = Instantiate(CritTextPrefab, transform.position + Offset, Quaternion.identity);
        }
        else
        {
            go = Instantiate(textPrefab, transform.position + Offset, Quaternion.identity);
        }

            TextMeshProUGUI tmp = go.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = damage.ToString();
        }
        StartCoroutine(DestroyDamageText(go));
    }

    private IEnumerator DestroyDamageText(GameObject go)
    {
        float timer = 0f;
        while (timer < textLifetime)
        {
            timer += Time.deltaTime;
            go.transform.position += Vector3.up * floatUpSpeed * Time.deltaTime;

            yield return null;
        }
        Destroy(go);
    }

}

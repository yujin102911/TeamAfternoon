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
    public Vector3 Offset;

    [Header("점멸 설정")]
    public float blinkDuration = 0.3f;
    public Color EnemyHitColor = Color.red;

    [Header("텍스트 설정")]
    public float textLifetime = 1.0f;
    public float floatUpSpeed = 1.0f;

    private Dictionary<RuntimeEnemy, EnemyVisual> _visualMap = new Dictionary<RuntimeEnemy, EnemyVisual>();
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
        foreach (EnemyVisual visual  in _visualMap.Values)
        {
            if (visual != null) Destroy(visual.gameObject);
        }
        _visualMap.Clear();

        foreach (RuntimeEnemy enemy in _battleSystem.Enemies)
        {
            GameObject obj = Instantiate(_enemyPrefab, _enemyContainer);
            EnemyVisual visual = obj.GetComponent<EnemyVisual>();

            if (visual != null)
            {
                visual.Initialize(enemy);
                _visualMap.Add(enemy, visual);
            }
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

    private void HandleEnemyHit(RuntimeEnemy enemy)
    {
        if (_visualMap.TryGetValue(enemy, out EnemyVisual visual))
            visual.PlayerHitAnimation();

        StartCoroutine(ShowEnemyDamage(_battleSystem.TotalDamage));
    }

    private void HandleEnemyDead(RuntimeEnemy enemy)
    {
        if (_visualMap.TryGetValue(enemy, out EnemyVisual visual))
        {
            visual.EnemyDeadColor(enemy);
        }
    }

    public void PlayDamage(int damage)
    {
        StartCoroutine(ShowEnemyDamage(damage));
    }

    public IEnumerator ShowEnemyDamage(int damage)
    {
        

        SpawnDamageText(damage);

        yield return new WaitForSeconds(blinkDuration);
    }

    private void SpawnDamageText(int damage)
    {
        if (textPrefab == null) return;
        GameObject go = Instantiate(textPrefab, transform.position + Offset, Quaternion.identity);

        TextMeshProUGUI tmp = go.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = "-" + damage.ToString();
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

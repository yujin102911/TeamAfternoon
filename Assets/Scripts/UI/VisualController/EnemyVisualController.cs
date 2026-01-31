using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class EnemyVisualController : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private Transform _enemyRightPos;
    [SerializeField] private Transform _enemyLeftPos;
    [SerializeField] private UIFollowWorldTarget _enemyHealthBar;

    [Header("돌진 연출")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Vector3 _cameraRightPos;
    [SerializeField] private Vector3 _cameraLeftPos;
    [SerializeField] private AnimationCurve dashCurve;
    [SerializeField] private AnimationCurve cameraDashCurve;
    private Coroutine _dashCoroutine;

    [Header("정렬 설정")]
    [SerializeField] private float _spacing = 2.5f;

    public GameObject textPrefab; // 생성될 프리팹
    public GameObject CritTextPrefab; // 크리티컬 생성될 프리팹
    public Vector3 Offset;
    public Vector3 Left_Offset;

    [Header("점멸 설정")]
    public float blinkDuration = 0.3f;
    public Color EnemyHitColor = Color.red;

    [Header("텍스트 설정")]
    public float textLifetime = 1.0f;
    public float floatUpSpeed = 1.0f;

    // 적 애니메이터
    private GameObject _currentEnemy;
    private Animator _enemyAnimator;
    private EnemyAttackEffect _enemyAttackEffect;
    private EnemyVisual visual;

    private RuntimeEnemy _runtimeEnemy;
    private string _currentAnimation = "";

    private Dictionary<RuntimeEnemy, EnemyVisual> _visualMap = new Dictionary<RuntimeEnemy, EnemyVisual>();
    private List<RuntimeEnemy> _currentEnemies = new List<RuntimeEnemy>();
    private BattleSystem _battleSystem;

    private Vector3 _enemyOffset = Vector3.zero;

    public event Action<bool> OnEnemySideChanged; // 적 위치 변경 이벤트

    private void Awake()
    {

    }

    public void Initialize(BattleSystem battleSystem)
    {
        _battleSystem = battleSystem;

        _battleSystem.OnBattleInitialized += CreateEnemies;
        _battleSystem.OnEnemySideChanged += HandleSideChanged;
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

        for (int i = _enemyRightPos.childCount - 1; i >= 0; i--)
        {
            if (_enemyRightPos.GetChild(i) != null) Destroy(_enemyRightPos.GetChild(i).gameObject);
        }

        for (int i = _enemyLeftPos.childCount - 1; i >= 0; i--)
        {
            if (_enemyLeftPos.GetChild(i) != null) Destroy(_enemyLeftPos.GetChild(i).gameObject);
        }

        foreach (RuntimeEnemy enemy in _battleSystem.Enemies)
        {
            _enemyOffset = enemy.Data.EnemyPrefab.transform.position;

            //기본 우측 소환
            GameObject obj = Instantiate(enemy.Data.EnemyPrefab, _enemyRightPos);

            _runtimeEnemy = enemy;

            _currentEnemy = obj;
            _enemyAnimator = obj.GetComponent<Animator>();
            _enemyAttackEffect = obj.GetComponent<EnemyAttackEffect>();

            visual = obj.GetComponent<EnemyVisual>();

            // 맵 전체 좌표정보 전달
            if (GameManager.Instance.MapSystem != null)
                _enemyAttackEffect.Set_worldSectorPos(GameManager.Instance.MapSystem.GetSectorsPosition());

            _enemyAttackEffect.Set_StageNum(GameManager.Instance.CurrentStageData.StageNumber);

            _enemyHealthBar.SetTarget(obj.transform, enemy.Data.HPBarOffset);
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
        if(animation == "Sturn")
        {
            _enemyAttackEffect.Show_Star_Flip();
        }

        // 기절 별 이펙트 숨기기
        if (_currentAnimation == "Sturn" && _currentAnimation != animation)
        {
            _enemyAttackEffect.Hide_Star();
        }

        if (_enemyAnimator != null)
        {
            _currentAnimation = animation;
            _enemyAnimator.CrossFade(animation, 0.2f);
        }
    }

    private void HandleSideChanged(bool isLeft)
    {
        float targetX = isLeft ? -5f : 5f;
        if (visual != null)
        {
            SpriteRenderer sr = visual.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.flipX = !isLeft;
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

    public void PlayEnemyDash(List<int> targets, bool isLeft)
    {
        if (_enemyAttackEffect != null)
            _enemyAttackEffect.Set_targetSectors(targets);

        Debug.Log($"적위치{isLeft}");

        _enemyAttackEffect.SpawnAttackEffect();

        // 기존 코루틴이 실행 중이면 중지
        if (_dashCoroutine != null)
        {
            StopCoroutine(_dashCoroutine);
        }

        _dashCoroutine = StartCoroutine(EnemyDashCoroutine(isLeft));
    }

    // 대쉬 공격 연출 코루틴
    private IEnumerator EnemyDashCoroutine(bool isLeft)
    {
        bool hitTriggered = false;
        float duration = 0.25f;   // 돌진 시간

        if (TimelineManager.Instance != null)
        {
            duration = TimelineManager.Instance.Tick_interval - 0.1f;
        }

        float elapsed = 0f;

        Vector3 camera_start = isLeft ? _cameraLeftPos : _cameraRightPos;
        Vector3 camera_end = isLeft ? _cameraRightPos : _cameraLeftPos;

        targetCamera.transform.position = camera_start;

        Transform start = isLeft ? _enemyLeftPos : _enemyRightPos;
        Transform end = isLeft ? _enemyRightPos : _enemyLeftPos;

        Vector3 startPos = Vector3.zero;
        Vector3 endPos = Vector3.zero;
        
        Vector3 offset = _enemyOffset;

        // 위치 오프셋 반영
        if (isLeft)
        {
            offset.x = -offset.x;
        }

        startPos = start.position + offset;
        endPos = end.position - offset;

        _currentEnemy.transform.position = startPos;

        ChangeAnim("Dash");

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float curveT = dashCurve.Evaluate(t);
            float cameraT = cameraDashCurve.Evaluate(t);

            if (!hitTriggered && t >= duration * 0.7f)
            {
                hitTriggered = true;
                
                OnEnemySideChanged?.Invoke(isLeft);
            }

            targetCamera.transform.position =
    Vector3.Lerp(camera_start, camera_end, cameraT);

            //targetCamera.transform.position =
            //    Vector3.Lerp(camera_start, camera_end, curveT);

            _currentEnemy.transform.position =
                Vector3.Lerp(startPos, endPos, curveT);

            yield return null;
        }

        targetCamera.transform.position = camera_end;
        _currentEnemy.transform.position = endPos;

        CameraShake.Instance.RePosition(camera_end);

        SpriteRenderer sr = _currentEnemy.GetComponent<SpriteRenderer>();
        if (sr != null) sr.flipX = isLeft;

        ChangeAnim("Idle");
    }



    public void PlayDamage(int damage, Critical is_crit, bool is_sturn)
    {
        if(is_sturn)
        {
            ChangeAnim("Sturn");
        }
        else
        {
            //데미지가 0이 아닐때만 피격 모션
            if(damage != 0)
            {
                ChangeAnim("Hurt");
                Choose_EnemyHurt(GameManager.Instance.CurrentStageData.StageNumber);
            }
            
        }

        StartCoroutine(ShowEnemyDamage(damage, is_crit));
    }

    public void PlayEnemyAttack(List<int> targets)
    {
        if (_enemyAttackEffect != null)
            _enemyAttackEffect.Set_targetSectors(targets);

        ChangeAnim("Attack");
    }

    public IEnumerator ShowEnemyDamage(int damage, Critical is_crit)
    {
        SpawnDamageText(damage, is_crit);

        yield return new WaitForSeconds(blinkDuration);
    }

    private void SpawnDamageText(int damage, Critical is_crit)
    {
        if (textPrefab == null) return;
        
        GameObject go = null;

        if (is_crit == Critical.Critical_2 || is_crit == Critical.Critical_3)
        {
            // 왼쪽일 때는 오프셋 조정
            if (_runtimeEnemy.IsLeft)
            {
                go = Instantiate(CritTextPrefab, transform.position + Offset + Left_Offset, Quaternion.identity);
            }
            else
            {
                go = Instantiate(CritTextPrefab, transform.position + Offset, Quaternion.identity);
            }
        }
        else
        {
            // 왼쪽일 때는 오프셋 조정
            if (_runtimeEnemy.IsLeft)
            {
                go = Instantiate(textPrefab, transform.position + Offset + Left_Offset, Quaternion.identity);
            }
            else
            {
                go = Instantiate(textPrefab, transform.position + Offset, Quaternion.identity);
            }
            
        }

        //데미지 텍스트 순서 변경
        go.GetComponent<Canvas>().sortingOrder = 75;

            TextMeshProUGUI tmp = go.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            if(damage == 0)
            {
                tmp.text = "Miss!";
            }
            else
            {
                tmp.text = damage.ToString();
            }   
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

    #region Preview Methods
    public void PreviewFlip(bool isLeft)
    {
        if (_currentEnemy != null)
        {
            SpriteRenderer sr = _currentEnemy.GetComponentInChildren<SpriteRenderer>();
            if (sr != null ) sr.flipX = !isLeft;

            Transform targetTr = isLeft ? _enemyLeftPos : _enemyRightPos;
            
            Vector3 offset = _enemyOffset;

            if (isLeft)
            {
                offset.x = -offset.x;
            }

            _currentEnemy.transform.position = targetTr.position + offset;
        }

        if (targetCamera != null)
        {
            targetCamera.transform.position = isLeft ? _cameraLeftPos : _cameraRightPos;
        }

        Debug.Log($"<color=red>[EV_controller] 카메라 뒤집기 / isLeft: {isLeft}</color>");
    }

    public void RestoreActualSide()
    {
        if (_runtimeEnemy != null && _currentEnemy != null)
        {
            bool actualIsLeft = _runtimeEnemy.IsLeft;

            SpriteRenderer sr = _currentEnemy.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.flipX = !actualIsLeft;

            Transform targetTr = actualIsLeft ? _enemyLeftPos : _enemyRightPos;
            
            Vector3 offset = _enemyOffset;

            if (actualIsLeft)
            {
                offset.x = -offset.x;
            }

            _currentEnemy.transform.position = targetTr.position + offset;

            if (targetCamera != null)
            {
                targetCamera.transform.position = actualIsLeft ? _cameraLeftPos : _cameraRightPos;
            }
        }
    }
    #endregion

    private void Choose_EnemyHurt(int stage_num)
    {
        if (SoundManager.Instance == null) return;

        if (GameManager.Instance != null && GameManager.Instance.UserGameData.Difficulty == Difficulty.Hard)
        {
            SoundManager.Instance.Play(SoundID.EnemyHurt_5);
            return;
        }

        switch (stage_num -1)
        {

            case 0:
                SoundManager.Instance.Play(SoundID.EnemyHurt_0);
                break;
            case 1:
                SoundManager.Instance.Play(SoundID.EnemyHurt_1);
                break;
            case 2:
                SoundManager.Instance.Play(SoundID.EnemyHurt_2);
                break;
            case 3:
                SoundManager.Instance.Play(SoundID.EnemyHurt_3);
                break;
            case 4:
                SoundManager.Instance.Play(SoundID.EnemyHurt_4);
                break;
            case 5:
                SoundManager.Instance.Play(SoundID.EnemyHurt_5);
                break;
        }
    }

}

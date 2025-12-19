using UnityEngine;

/// <summary>
/// 적 스프라이트, 피격 연출
/// </summary>
public class EnemyVisual : MonoBehaviour
{
    [Header("연결 요소")]
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private Coroutine _hitCoroutine;

    public void Initialize(RuntimeEnemy enemy)
    {
        //if (enemy.Data.Enemy_Sprite != null) 
        //    _spriteRenderer.sprite = enemy.Data.Enemy_Sprite;
    }

    public void PlayerHitAnimation()
    {
        if (_hitCoroutine != null) StopCoroutine(_hitCoroutine);

        _hitCoroutine = StartCoroutine(HitRoutine());
    }

    public System.Collections.IEnumerator HitRoutine()
    {
        Color original = _spriteRenderer.color;
        _spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        _spriteRenderer.color = original;
        _hitCoroutine = null; // 코루틴 종료 표시
    }

    public void EnemyDeadColor(RuntimeEnemy enemy)
    {
        //if (enemy.Data.EnemyDeadSprite != null)
        //    _spriteRenderer.sprite = enemy.Data.EnemyDeadSprite;
    }

}

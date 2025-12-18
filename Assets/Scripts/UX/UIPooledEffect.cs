using UnityEngine;

public class UIPooledEffect : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string animationStateName = "Play";

    private UIEffectPool _pool;

    public void Init(UIEffectPool pool)
    {
        _pool = pool;
    }

    /// <summary>
    /// 위치를 받아서 이펙트 재생
    /// </summary>
    public void Play(Vector2 anchoredPosition)
    {
        RectTransform rect = transform as RectTransform;
        rect.anchoredPosition = anchoredPosition;

        gameObject.SetActive(true);
        animator.Play(0, 0, 0f);
    }

    /// <summary>
    /// 애니메이션 이벤트에서 호출
    /// </summary>
    public void OnAnimationEnd()
    {
        gameObject.SetActive(false);
        _pool.Return(this);
    }
}

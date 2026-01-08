using UnityEngine;
using System.Collections;

public class EffectAutoReturn : MonoBehaviour
{
    private IEffectPoolOwner _owner;

    private SpriteRenderer _sr;
    private Vector3 _defaultScale;
    private bool _defaultFlipX;

    public void Init(IEffectPoolOwner owner)
    {
        _owner = owner;

        _sr = GetComponent<SpriteRenderer>();
        _defaultScale = transform.localScale;
        _defaultFlipX = _sr.flipX;
    }

    public void ResetState()
    {
        _sr.flipX = _defaultFlipX;
        transform.localScale = _defaultScale;
        _sr.color = Color.white;
    }

    public void Flip()
    {
        _sr.flipX = !_sr.flipX;
    }

    // 애니메이션 이벤트로 작동
    public void AutoReturn()
    {
        _owner.ReturnEffect(gameObject);
    }
}

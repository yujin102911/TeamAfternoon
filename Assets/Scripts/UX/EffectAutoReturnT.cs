using UnityEngine;
using System.Collections;

public class EffectAutoReturn : MonoBehaviour
{
    private IEffectPoolOwner _owner;
    private float _lifeTime;

    public void Init(IEffectPoolOwner owner)
    {
        _owner = owner;
    }

    // 애니메이션 이벤트로 작동
    public void AutoReturn()
    {
        _owner.ReturnEffect(gameObject);
    }

    private IEnumerator ReturnRoutine()
    {
        yield return new WaitForSeconds(_lifeTime);

        _owner.ReturnEffect(gameObject);
    }
}

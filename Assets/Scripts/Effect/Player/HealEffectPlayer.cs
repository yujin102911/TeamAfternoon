using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class HealEffectPlayer : MonoBehaviour
{
    [SerializeField] private CanvasGroup rootGroup;   // Heal_Effect에 붙이거나 참조
    [SerializeField] private Animator iconsAnimator;  // Icons의 Animator
    [SerializeField] private string playStateName = "Play"; // 아이콘 애니 상태명

    [Button("효과실행")]
    public void PlayAndFade()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        StartCoroutine(CoPlayAndFade());
    }

    private IEnumerator CoPlayAndFade()
    {
        rootGroup.alpha = 0.3f;
        gameObject.SetActive(true);

        if (SoundManager.Instance != null)
            SoundManager.Instance.Play(SoundID.Heal);

        iconsAnimator.Play(playStateName, 0, 0f);

        float fadeStartAt = 0.0f;   // 바로 페이드 시작
        float fadeEndAt = 1.0f;   // 애니 끝날 때 alpha=0

        while (true)
        {
            var st = iconsAnimator.GetCurrentAnimatorStateInfo(0);
            
            float nt = st.normalizedTime; // 0~1 (루프면 1 넘어감)

            float progress01 = st.normalizedTime % 1f;

            Debug.Log($"nt: {progress01}");

            float k = Mathf.InverseLerp(fadeStartAt, fadeEndAt, Mathf.Clamp01(nt));
            rootGroup.alpha = Mathf.Lerp(1f, 0f, k);

            bool finished =
    !iconsAnimator.IsInTransition(0) &&
    st.normalizedTime >= 1f;

            if (finished) break; // 애니 1회 끝
            yield return null;
        }

        rootGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}

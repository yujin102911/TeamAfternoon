using System.Collections;
using UnityEngine;

public class Nail : MonoBehaviour
{
    [Header("행동변경 주기")]
    [SerializeField] private float _changeDuration = 2.0f;

    [Header("애니메이션")]
    [SerializeField] private Animator animator;
    [SerializeField] private string defaultIdle = "Idle_Default";
    [SerializeField] private int maxIdleCount = 3; // Idle_0 ~ Idle_2

    private Coroutine _idleRoutine;

    void Start()
    {
        // 시작은 항상 디폴트
        animator.Play(defaultIdle, 0, 0f);

        _idleRoutine = StartCoroutine(IdleLoop());
    }

    IEnumerator IdleLoop()
    {
        while (true)
        {
            // 1️⃣ 기본 상태로 대기
            yield return new WaitForSeconds(_changeDuration);

            // 2️⃣ 랜덤 Idle 선택
            int index = Random.Range(0, maxIdleCount);
            string randomIdle = $"Idle_{index}";

            // 3️⃣ 랜덤 Idle 재생
            animator.CrossFade(randomIdle, 0.2f);

            // 4️⃣ 해당 애니 길이만큼 대기
            yield return null; // CrossFade 반영 1프레임
            float animLength = animator.GetCurrentAnimatorStateInfo(0).length *2;

            yield return new WaitForSeconds(animLength);

            // 5️⃣ 다시 디폴트로 복귀
            animator.CrossFade(defaultIdle, 0.2f);
        }
    }
}

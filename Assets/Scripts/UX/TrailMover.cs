using UnityEngine;
using System.Collections;

// 이 스크립트는 베기 이펙트 프리팹에 부착됩니다.
public class TrailMover : MonoBehaviour
{
    private TrailRenderer trail;
    private bool isMoving = false;

    // ========================================
    // ⚔️ 인스펙터 설정 필드
    // ========================================

    [Header("트레일 설정")]
    [Tooltip("이 이펙트가 시작될 월드 좌표입니다. 이 위치는 고정됩니다.")]
    [SerializeField] private Vector3 startWorldPosition = Vector3.zero;

    [Tooltip("트레일이 이동할 목표 위치입니다. (월드 좌표)")]
    [SerializeField] private Vector3 targetWorldPosition = new Vector3(3, 0, 0);

    [Tooltip("트레일이 이동할 초당 속도입니다. (단위: World Unit/sec)")]
    [SerializeField] private float moveSpeed = 20f;


    void Awake()
    {
        trail = GetComponent<TrailRenderer>();
        if (trail == null)
        {
            Debug.LogError("TrailRenderer 컴포넌트가 TrailMover에 없습니다! TrailMover를 비활성화합니다.");
            enabled = false;
        }
    }

    private void Start()
    {
        InitializeAndMove();
    }

    // ========================================
    // 🏃 이동 시작 함수 (CombatVisualManager에서 호출됨)
    // ========================================

    /// <summary>
    /// 트레일 이펙트 이동을 시작합니다.
    /// 이 함수는 CombatVisualManager에서 이펙트 생성 후 바로 호출됩니다.
    /// (주: startWorldPosition과 targetWorldPosition은 필드 값을 사용합니다.)
    /// </summary>
    public void InitializeAndMove()
    {
        if (isMoving || moveSpeed <= 0) return;

        // 1. 시작 위치로 즉시 이동 (프리팹이 어디에 생성되든 이 위치로 고정)
        transform.position = startWorldPosition;
        gameObject.SetActive(true);

        // 2. 트레일 렌더링 시작
        trail.Clear(); // 잔상 제거
        trail.emitting = true;

        // 3. 이동 코루틴 시작
        StartCoroutine(MoveCoroutine());
    }

    private IEnumerator MoveCoroutine()
    {
        isMoving = true;
        Vector3 currentPos = transform.position;

        // 이동에 필요한 총 시간 계산 (거리 / 속도)
        float distance = Vector3.Distance(startWorldPosition, targetWorldPosition);
        float duration = distance / moveSpeed;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Lerp를 사용하여 속도 기반으로 이동
            transform.position = Vector3.Lerp(currentPos, targetWorldPosition, t);
            yield return null;
        }

        // 4. 이동 완료 후, 트레일 생성 종료
        transform.position = targetWorldPosition;
        trail.emitting = false;

        
        // 5. 트레일 잔상이 완전히 사라질 때까지 대기
        yield return new WaitForSeconds(trail.time);

        // 6. 오브젝트 소멸
        Destroy(gameObject);
        isMoving = false;
    }

    // ========================================
    // 🎨 Gizmos (에디터 시각화)
    // ========================================

    private void OnDrawGizmos()
    {
        // Gizmos가 Inspector에서 선택되었을 때만 표시됩니다.
        if (Application.isPlaying) return;

        // 시작 위치 표시 (노란색 구)
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(startWorldPosition, 0.15f);

        // 끝 위치 표시 (빨간색 구)
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetWorldPosition, 0.15f);

        // 시작점에서 끝점으로 가는 방향 선 표시 (흰색 선)
        Gizmos.color = Color.white;
        Gizmos.DrawLine(startWorldPosition, targetWorldPosition);

        // 트레일의 이동 방향 표시 (파란색 화살표)
        // (간단한 화살표처럼 보이기 위해 끝점에서 약간 뒤로 물러난 위치에 콘 모양을 그립니다.)
        Vector3 direction = (targetWorldPosition - startWorldPosition).normalized;

        // 끝점에서 거리에 따른 속도 예측선 (주황색 선)
        Gizmos.color = Color.cyan;
        // 텍스트 기반 게임이므로, 단순한 위치 마커만 제공합니다.
    }
}
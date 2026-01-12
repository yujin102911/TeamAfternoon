using UnityEngine;

public class BottomBarController : MonoBehaviour
{
    [Header("위치 설정")]
    public float showPosY = 0f;    // 화면에 보일 때 Y좌표 (보통 0)
    public float hidePosY = -80f;  // 숨겨질 때 Y좌표 (보통 높이만큼 아래로)

    [Header("속도 설정")]
    public float moveSpeed = 400f; // 움직이는 속도

    private RectTransform rect;
    private float targetY; // 이동해야 할 목표 Y좌표

    void Start()
    {
        rect = GetComponent<RectTransform>();
        // 시작할 때 현재 위치를 목표로 설정 (멋대로 움직이지 않게)
        targetY = rect.anchoredPosition.y;
    }

    void Update()
    {
        // 현재 Y위치가 목표 Y위치와 다르면 부드럽게 이동
        if (Mathf.Abs(rect.anchoredPosition.y - targetY) > 0.1f)
        {
            float newY = Mathf.MoveTowards(
                rect.anchoredPosition.y,
                targetY,
                moveSpeed * Time.deltaTime
            );

            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, newY);
        }
    }

    // 외부(Manager)에서 부르는 함수: "상태를 반대로 바꿔라"
    public void ToggleBar()
    {
        // 만약 현재 목표가 '숨김 위치'와 가깝다면 -> '보임 위치'로 목표 변경
        if (Mathf.Abs(targetY - hidePosY) < 1.0f)
        {
            targetY = showPosY;
        }
        // 아니라면(이미 보이고 있거나 보이는 중이라면) -> '숨김 위치'로 목표 변경
        else
        {
            targetY = hidePosY;
        }
    }
}
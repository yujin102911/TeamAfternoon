using UnityEngine;

public class AutoScroll : MonoBehaviour
{
    [Header("설정")]
    public float moveSpeed = 200f; // 이동 속도

    private RectTransform myRect;
    private Vector2 targetPosition; // 이동해야 할 목표 위치

    void Start()
    {
        myRect = GetComponent<RectTransform>();
        targetPosition = myRect.anchoredPosition; // 시작 위치 고정
    }

    void Update()
    {
        // 현재 위치가 목표 위치와 다르면 부드럽게 이동
        if (Vector2.Distance(myRect.anchoredPosition, targetPosition) > 0.1f)
        {
            myRect.anchoredPosition = Vector2.MoveTowards(
                myRect.anchoredPosition,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
        }
    }

    // 외부(Manager)에서 호출하는 함수: "amount만큼 위로 올라가라"
    public void MoveNext(float amount)
    {
        // 현재 목표 위치에서 Y축으로 amount만큼 더함 (위로 이동)
        targetPosition += new Vector2(0f, amount);
    }
}
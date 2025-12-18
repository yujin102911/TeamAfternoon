using UnityEngine;

public class UIRotate : MonoBehaviour
{
    public bool Is_Left = false;

    [SerializeField] private float maxAngle = 10f;
    [SerializeField] private float rotateSpeed = 3f;

    private bool _isArrived = false;
    private float _time;

    /// <summary>
    /// 이동이 끝났을 때 호출
    /// </summary>
    public void OnArrived()
    {
        _isArrived = true;
        _time = 0f;
    }

    /// <summary>
    /// 나갈 때 회전 중지용 (선택)
    /// </summary>
    public void OnExit()
    {
        _isArrived = false;
        transform.localRotation = Quaternion.identity;
    }

    private void Update()
    {
        if (!_isArrived)
            return;

        _time += Time.deltaTime * rotateSpeed;

        float z = Mathf.Lerp(
            -maxAngle,
            maxAngle,
            Mathf.PingPong(_time, 1f)
        );

        // 방향 반전
        if (Is_Left)
            z *= -1f;

        transform.localRotation = Quaternion.Euler(0f, 0f, z);
    }
}

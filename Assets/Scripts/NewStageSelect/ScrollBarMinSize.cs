using UnityEngine;
using UnityEngine.UI;

// [ExecuteInEditMode]를 넣으면 에디터 상태에서도 바로 확인 가능합니다.
[ExecuteInEditMode]
public class ScrollbarMinSize : MonoBehaviour
{
    private Scrollbar _scrollbar;

    [Header("설정")]
    [Range(0.05f, 0.5f)]
    public float minSize = 0.1f; // 원하는 최소 크기 (0.1이면 전체의 10%)

    void OnEnable()
    {
        _scrollbar = GetComponent<Scrollbar>();
    }

    // ScrollRect가 LateUpdate에서 크기를 계산하므로, 
    // 그보다 늦은 시점이나 비슷한 시점에 값을 보정해줍니다.
    void LateUpdate()
    {
        if (_scrollbar != null)
        {
            if (_scrollbar.size < minSize)
            {
                _scrollbar.size = minSize;
            }
        }
    }
}
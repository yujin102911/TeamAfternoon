using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ScrollbarMinSize : MonoBehaviour
{
    private Scrollbar _scrollbar;
    [SerializeField] private ScrollRect scrollRect; // 인스펙터에서 ScrollRect를 연결해주세요!

    [Header("설정")]
    [Range(0.05f, 0.5f)]
    public float minSize = 0.1f; // 원하는 최소 크기 (0.1이면 전체의 10%)

    void OnEnable()
    {
        _scrollbar = GetComponent<Scrollbar>();
        if (scrollRect != null)
        {
            // 값이 바뀔 때마다 실행되도록 리스너 등록
            scrollRect.onValueChanged.AddListener(OnScrollChanged);
        }
    }

    void Start()
    {
        if (scrollRect != null)
        {
            OnScrollChanged(scrollRect.normalizedPosition);
        }
    }

    void OnDisable()
    {
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
        }
    }

    // ScrollRect의 계산이 끝난 후 호출되어 깜빡임을 방지함
    void OnScrollChanged(Vector2 value)
    {
        if (_scrollbar != null && _scrollbar.size < minSize)
        {
            _scrollbar.size = minSize;
        }
    }
    public void ManualRefresh()
    {
        // _scrollbar가 아직 할당 전이라면 여기서 직접 찾아줍니다.
        if (_scrollbar == null) _scrollbar = GetComponent<Scrollbar>();

        if (_scrollbar != null && scrollRect != null)
        {
            if (_scrollbar.size < minSize) _scrollbar.size = minSize;
        }
    }

}
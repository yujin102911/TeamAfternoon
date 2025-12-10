using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 타임라인 각 틱 슬롯에 붙어 마우스 호버 시 프리뷰를 요청하는 핸들러
/// </summary>
public class TimelineTickHoverHandler : MonoBehaviour, 
    IPointerEnterHandler, IPointerExitHandler
{
    public int tickIndex;
    private TimelineUI _timelineUI;

    private void Start()
    {
        _timelineUI = GetComponentInParent<TimelineUI>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_timelineUI != null)
        {
            _timelineUI.OnCursorEnter(tickIndex);
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_timelineUI != null)
            _timelineUI.OnCursorExit();
    }


}

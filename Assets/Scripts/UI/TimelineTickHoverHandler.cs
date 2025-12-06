using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 타임라인 각 틱 슬롯에 붙어 마우스 호버 시 프리뷰를 요청하는 핸들러
/// </summary>
public class TimelineTickHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int tickIndex;

    /// <summary>
    /// 마우스 호버 시, 틱 인덱스랑 같이 전달 *^^*
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        //잔상기능 부활 필요

        //if (TimeLine_Manager.Instance != null && TimeLine_Manager.Instance.IsExecutingRound)
        //    return;

        //TimelinePreviewManager.Instance?.ShowPreviewUntilTick(tickIndex);
    }

    /// <summary>
    /// 마우스 나가면 바로 숨기기
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        //if (TimeLine_Manager.Instance != null)
        //{
        //    TimelinePreviewManager.Instance.HidePreview();
        //}
    }

}

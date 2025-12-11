using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 적 슬롯 마우스 오버 핸들러
/// </summary>
public class EnemySlotHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int tick;
    public TimelineUI timelineUI;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsSectorSelected) return;
        if (timelineUI != null)
        {
            timelineUI.ShowEnemyAttackTooltip(tick, transform.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsSectorSelected) return;
        if (timelineUI != null)
        {
            timelineUI.HideTooltip();
        }
    }
}

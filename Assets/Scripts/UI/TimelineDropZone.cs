using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TimelineDropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int tickIndex;
    public TimelineUI timelineUI;
    private Image image;
    private Color originalColor;

    void Awake()
    {
        image = GetComponent<Image>();
        if (image != null)
        {
            //originalColor = image.color;
        }
    }

    /// <summary>
    /// 마우스가 드롭존 위에 있을 때 (스냅 미리보기)
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        originalColor = image.color;

        // 드래그 중인 카드가 있으면 하이라이트
        if (eventData.pointerDrag != null)
        {
            Draggable_Block draggable = eventData.pointerDrag.GetComponent<Draggable_Block>();
            if (draggable != null && image != null)
            {
                // 배치 가능한지 확인
                if (timelineUI != null &&
                    timelineUI.IsRangeAvailable(tickIndex, draggable.BlockData.blockLength))
                {
                    image.color = new Color(0.5f, 1f, 0.5f); // 초록색 하이라이트
                }
                else
                {
                    image.color = new Color(1f, 0.6f, 0.6f); // 빨간색 하이라이트
                }
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // 색상 원래대로
        if (image != null)
        {
            image.color = originalColor;
        }

        // 드래그 중인 카드 가져오기
        Draggable_Block draggable = eventData.pointerDrag?.GetComponent<Draggable_Block>();

        if (draggable != null)
        {

            // 레거시
            //if (DeckManager.Instance != null)
            //{
            //    bool success = DeckManager.Instance.PlaceCard(draggable.card, tickIndex);

            //    if (success)
            //    {
            //        // 손패 UI 업데이트
            //        if (UIManager.Instance != null)
            //        {
            //            UIManager.Instance.UpdateHandUI(DeckManager.Instance.Hand);
            //        }
            //    }
            //}
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 색상 원래대로
        if (eventData.pointerDrag != null && image != null
            && (IsColorSimilar(image.color, new Color(0.5f, 1f, 0.5f))) || IsColorSimilar(image.color, new Color(1f, 0.6f, 0.6f)))
        {
            image.color = originalColor;
        }
    }

    bool IsColorSimilar(Color a, Color b, float tolerance = 0.01f)
    {
        return
            Mathf.Abs(a.r - b.r) < tolerance &&
            Mathf.Abs(a.g - b.g) < tolerance &&
            Mathf.Abs(a.b - b.b) < tolerance &&
            Mathf.Abs(a.a - b.a) < tolerance;
    }
}

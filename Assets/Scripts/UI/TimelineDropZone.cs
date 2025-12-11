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

    public void OnDrop(PointerEventData eventData)
    {
        // 색상 원래대로
        if (image != null)
        {
            image.color = originalColor;
        }

        // 드래그 중인 카드 가져오기
        RuntimeBlock block_info = eventData.pointerDrag.GetComponent<HandBlock_UI>().runtimeBlock;

        if (block_info != null && TimelineManager.Instance != null)
        {
            // 배치 시도
            bool success = TimelineManager.Instance.TryPlaceBlock(block_info, tickIndex);

            if (success)
            {
                //블록 배치 성공 시 드래그 블록 숨기기
                //Draggable_Block.Instance.Hide();
            }
        }
    }

    /// <summary>
    /// 마우스가 드롭존 위에 있을 때 (스냅 미리보기)
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        originalColor = image.color;
        if (eventData.pointerDrag == null || eventData.pointerDrag.GetComponent<HandBlock_UI>() == null) return;

        RuntimeBlock block_info = eventData.pointerDrag.GetComponent<HandBlock_UI>().runtimeBlock;

        if (block_info != null && TimelineManager.Instance != null)
        {
            // 배치 가능한지 확인
            bool canPlace = TimelineManager.Instance.CanPlaceAt(tickIndex, block_info.BaseData.BlockLength);
            if (image) image.color = canPlace ? new Color(0.5f, 1f, 0.5f) : new Color(1f, 0.6f, 0.6f);
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

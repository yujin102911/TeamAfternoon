using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BarRaycastFilter : MonoBehaviour, IPointerEnterHandler, IEndDragHandler
{
    [SerializeField]
    private Image _image;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null || DragOn_timeline.instance == null) return;

        var draggable = eventData.pointerDrag.GetComponent<IDraggableUI>();
        var block = DragOn_timeline.instance.draggingBlock;
        //if (draggable == null) return;
        //if(block == null) return;

        if(draggable != null || block != null)
            _image.raycastTarget = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag BarRaycastFilter");
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class CursorHoverUI : MonoBehaviour
    ,IPointerEnterHandler, IPointerExitHandler
{
    public Texture2D hoverCursor;
    public Vector2 hotSpot = Vector2.zero;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Cursor.SetCursor(hoverCursor, hotSpot, CursorMode.Auto);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}

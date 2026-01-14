using UnityEngine;
using UnityEngine.EventSystems;

public class CursorDragBridge : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    public void OnBeginDrag(PointerEventData eventData)
    {
        ServiceLocator.Instance.Cursor.OnDragStart();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ServiceLocator.Instance.Cursor.OnDragEnd();
    }

    private void OnDisable()
    {
        if (ServiceLocator.Instance != null && ServiceLocator.Instance.Cursor != null)
        {
            ServiceLocator.Instance.Cursor.OnDragEnd();
        }
    }
}
using UnityEngine;
using UnityEngine.EventSystems;

public class PanelFocusHandler : MonoBehaviour
    , IPointerDownHandler
{
    private void OnEnable()
    {
        FocusPanel();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        FocusPanel();
    }
    private void FocusPanel()
    {
        transform.SetAsLastSibling();
    }
}

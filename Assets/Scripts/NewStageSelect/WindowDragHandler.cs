using UnityEngine;
using UnityEngine.EventSystems;

public class WindowDragHandler : MonoBehaviour, IDragHandler
{
    [SerializeField] private RectTransform _targetPanel;
    private Canvas _canvas;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();

        if (_targetPanel == null )
        {
            _targetPanel = transform.parent.GetComponent<RectTransform>();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_canvas == null || _targetPanel == null) return;
        _targetPanel.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    }

}

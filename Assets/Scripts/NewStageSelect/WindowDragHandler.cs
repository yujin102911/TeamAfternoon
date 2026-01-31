using UnityEngine;
using UnityEngine.EventSystems;

public class WindowDragHandler : MonoBehaviour
    , IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform _targetPanel;
    [SerializeField] private float _minVisibleHeight = 100f;
    [SerializeField] private float _minVisibleWidth = 50f;
    private Canvas _canvas;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();

        if (_targetPanel == null )
        {
            _targetPanel = transform.parent.GetComponent<RectTransform>();
        }
    }
    private void OnDisable()
    {
        ServiceLocator.Instance?.Cursor.OnDragEnd();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        ServiceLocator.Instance?.Cursor.OnDragStart();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_canvas == null || _targetPanel == null) return;
        _targetPanel.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        KeepInsideCanvas();
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        ServiceLocator.Instance.Cursor.OnDragEnd();
    }

    private void KeepInsideCanvas()
    {
        RectTransform canvasRect = _canvas.transform as RectTransform;
        if (canvasRect == null) return;

        Vector3[] panelCorners = new Vector3[4];
        _targetPanel.GetWorldCorners(panelCorners);

        for (int i = 0; i < 4; i++)
        {
            panelCorners[i] = canvasRect.InverseTransformPoint(panelCorners[i]);
        }

        Rect canvasLocalRect = canvasRect.rect;

        float deltaX = 0;
        float deltaY = 0;

        if (panelCorners[2].x < canvasLocalRect.xMin + _minVisibleWidth)
            deltaX = (canvasLocalRect.xMin + _minVisibleWidth) - panelCorners[2].x;
        else if (panelCorners[0].x > canvasLocalRect.xMax - _minVisibleWidth)
            deltaX = (canvasLocalRect.xMax - _minVisibleWidth) - panelCorners[0].x;

        if (panelCorners[2].y < canvasLocalRect.yMin + _minVisibleHeight)
            deltaY = (canvasLocalRect.yMin + _minVisibleHeight) - panelCorners[2].y;
        else if (panelCorners[2].y > canvasLocalRect.yMax)
            deltaY = canvasLocalRect.yMax - panelCorners[2].y;

        if (deltaX != 0 || deltaY != 0)
        {
            Vector3 worldDelta = canvasRect.TransformVector(new Vector3(deltaX, deltaY, 0));
            _targetPanel.position += worldDelta;
        }
    }


}

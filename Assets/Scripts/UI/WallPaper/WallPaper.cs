using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WallPaper : MonoBehaviour, IPointerClickHandler
{
    [Header("Refs")]
    [SerializeField] private RectTransform canvasRect;   // 최상위 Canvas의 RectTransform
    [SerializeField] private RectTransform panelRect;    // 열릴 패널 RectTransform

    [Header("Open Direction")]
    [SerializeField] private Vector2 pivotRightDown = new Vector2(0f, 1f); // top-left pivot => 오른쪽/아래로 펼쳐짐
    [SerializeField] private Vector2 pixelOffset = new Vector2(6f, -6f);   // 커서에서 살짝 띄우기(오른쪽/아래)

    [Header("Close")]
    [SerializeField] private bool closeOnLeftClickOutside = true;

    private Canvas _canvas;
    private Camera _uiCamera;

    private void Awake()
    {
        _canvas = canvasRect.GetComponentInParent<Canvas>();
        _uiCamera = (_canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : _canvas.worldCamera;

        HideImmediate();
    }

    private void Update()
    {
        if (!panelRect.gameObject.activeSelf) return;

        // 바깥 좌클릭하면 닫기 (원하면 우클릭도 닫기 추가 가능)
        if (closeOnLeftClickOutside && Input.GetMouseButtonDown(0))
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(panelRect, Input.mousePosition, _uiCamera))
                HideImmediate();
        }
    }

    public void ShowAtMouse()
    {
        ShowAtScreenPosition(Input.mousePosition);
    }

    public void ShowAtScreenPosition(Vector2 screenPos)
    {
        // 1) 활성화 + 피벗을 top-left로 (오른쪽/아래로 펼쳐지게)
        panelRect.gameObject.SetActive(true);
        panelRect.pivot = pivotRightDown;

        // 레이아웃 갱신(콘텐츠 크기 계산되도록)
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);

        // 2) 스크린 → 캔버스 로컬 좌표 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            _uiCamera,
            out var localPoint
        );

        // 3) 커서 오프셋 적용
        localPoint += pixelOffset;

        // 4) 1차 위치 적용
        panelRect.anchoredPosition = localPoint;

        // 5) 캔버스 밖으로 나가면 안쪽으로 클램프
        ClampToCanvas();
    }

    public void HideImmediate()
    {
        panelRect.gameObject.SetActive(false);
    }

    private void ClampToCanvas()
    {
        Rect canvasR = canvasRect.rect;   // 로컬 좌표에서의 캔버스 영역(xMin~xMax, yMin~yMax)
        Rect panelR = panelRect.rect;     // 로컬 좌표에서의 패널 크기(0~width/height)
        Vector2 pivot = panelRect.pivot;

        Vector2 pos = panelRect.anchoredPosition;
        Vector2 size = panelR.size;

        // 패널의 pivot을 고려해서 "pos"가 어디까지 가야 패널이 완전히 안에 들어오는지 계산
        float minX = canvasR.xMin + size.x * pivot.x;
        float maxX = canvasR.xMax - size.x * (1f - pivot.x);

        float minY = canvasR.yMin + size.y * pivot.y;
        float maxY = canvasR.yMax - size.y * (1f - pivot.y);

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        panelRect.anchoredPosition = pos;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ShowAtMouse();
        }
    }
}

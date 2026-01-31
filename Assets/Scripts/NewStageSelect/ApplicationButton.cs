using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 더블 클릭해서 패널을 여는 버튼
public class ApplicationButton : MonoBehaviour
    , IPointerClickHandler
    , ISelectHandler, IDeselectHandler
    , IPointerEnterHandler, IPointerExitHandler 
    //, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Image _image;
 
    [SerializeField] private Image _selectedImage;
    [SerializeField] private GameObject _applicationPanel;
    [SerializeField] private Vector2 _panelPosition;

    private bool _isHovering = false;
    private bool _isSelected = false;

    private Canvas canvas;

    private void Awake()
    {
        _image = GetComponent<Image>();
        if ( _selectedImage != null)
        {
            _selectedImage.gameObject.SetActive(false);
        }

        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovering = true;
        UpdateVisualState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovering = false;
        UpdateVisualState();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 1)
        {
            OnSingleClick();
        }
        else if(eventData.clickCount == 2)
        {
            OnDoubleClick();
        }
    }
    public void OnSelect(BaseEventData eventData)
    {
        _isSelected = true;
        UpdateVisualState();
    }
    public void OnDeselect(BaseEventData eventData)
    {
        _isSelected = false;
        UpdateVisualState();
    }

    private void OnSingleClick()
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
    private void OnDoubleClick()
    {
        if (_applicationPanel != null)
        {
            RectTransform rt = _applicationPanel.GetComponent<RectTransform>();

            rt.anchorMin = _panelPosition;
            rt.anchorMax = _panelPosition;

            rt.anchoredPosition = Vector2.zero;
            _applicationPanel.SetActive(true);
            _applicationPanel.transform.SetAsLastSibling();
        }
        EventSystem.current.SetSelectedGameObject(null);
        _isSelected = false;
        UpdateVisualState();
    }
    private void UpdateVisualState()
    {
        if (_selectedImage == null) return;

        if (_isHovering)
        {
            _selectedImage.gameObject.SetActive(true );
            SetAlpha(0.3f);
        }
        else if(_isSelected)
        {
             _selectedImage.gameObject.SetActive(true ) ;
            SetAlpha(0.5f);
        }
        else
        {
            _selectedImage.gameObject.SetActive(false);
        }
    }
    private void SetAlpha(float alpha)
    {
        Color color = _selectedImage.color;
        color.a = alpha;
        _selectedImage.color = color;
    }

    // 화면 비율에 맞는 위치 변환
    private Vector2 Convert_position(Vector3 v3)
    {
        Vector2 localPoint;
        Vector2 v2 = new Vector2(v3.x, v3.y);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            v2,
            canvas.worldCamera,   // ⭐ Camera 모드에서는 반드시 필요
            out localPoint
        );

        return localPoint;
    }
}

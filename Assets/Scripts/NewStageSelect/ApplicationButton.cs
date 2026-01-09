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

    private void Awake()
    {
        _image = GetComponent<Image>();
        if ( _selectedImage != null)
        {
            _selectedImage.gameObject.SetActive(false);
        }
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
            _applicationPanel.gameObject.transform.position = _panelPosition;
            _applicationPanel.SetActive(true);
            _applicationPanel.transform.SetAsLastSibling();
        }
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

}

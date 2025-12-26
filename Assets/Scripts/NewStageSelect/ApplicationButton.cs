using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 더블 클릭해서 패널을 여는 버튼
public class ApplicationButton : MonoBehaviour
    , IPointerClickHandler
    , ISelectHandler, IDeselectHandler
{
    private Image _image;
 
    [SerializeField] private Image _selectedImage;
    [SerializeField] private GameObject _applicationPanel;
    [SerializeField] private Vector2 _panelPosition;

    private void Awake()
    {
        _image = GetComponent<Image>();
        if ( _selectedImage != null)
        {
            _selectedImage.gameObject.SetActive(false);
        }
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
        Debug.Log("선택됨: 시각적 효과 시작");
        if (_selectedImage != null)
            _selectedImage.gameObject.SetActive(true);
    }
    public void OnDeselect(BaseEventData eventData)
    {
        Debug.Log("선택 취소됨: 시각적 효과 제거");
        if (_selectedImage != null)
            _selectedImage.gameObject.SetActive(false);
    }

    private void OnSingleClick()
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
        Debug.Log("단일 클릭");
    }
    private void OnDoubleClick()
    {
        if (_applicationPanel != null)
        {
            _applicationPanel.gameObject.transform.position = _panelPosition;
            _applicationPanel.SetActive(true);
        }
        Debug.Log("더블 클릭");
    }


}

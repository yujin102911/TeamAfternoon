using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 더블 클릭해서 패널을 여는 버튼
public class ApplicationButton : MonoBehaviour
    , IPointerClickHandler
    , ISelectHandler, IDeselectHandler
    //, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Image _image;
 
    [SerializeField] private Image _selectedImage;
    [SerializeField] private GameObject _applicationPanel;
    [SerializeField] private Vector2 _panelPosition;

    //[Header("그리드 설정")]
    //[SerializeField] private float _gridSize = 100f;
    //[SerializeField] private float _snapSpeed = 10f;

    //private RectTransform _rectTransform;
    //private Canvas _canvas;
    //private CanvasGroup _canvasGroup;
    //private Vector2 _targetPosition;
    //private bool _isSnapping = false;


    private void Awake()
    {
        //_rectTransform = GetComponent<RectTransform>();
        //_canvas = GetComponentInParent<Canvas>();
        //_canvasGroup = GetComponent<CanvasGroup>();
        //if (_canvasGroup == null ) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        //_targetPosition = _rectTransform.anchoredPosition;

        _image = GetComponent<Image>();
        if ( _selectedImage != null)
        {
            _selectedImage.gameObject.SetActive(false);
        }
    }
    //private void Update()
    //{
    //    if (_isSnapping)
    //    {
    //        _rectTransform.anchoredPosition = Vector2.Lerp(_rectTransform.anchoredPosition, _targetPosition, Time.deltaTime * _snapSpeed);
    //        if (Vector2.Distance(_rectTransform.anchoredPosition, _targetPosition) < 0.1f)
    //        {
    //            _rectTransform.anchoredPosition = _targetPosition;
    //            _isSnapping = false;
    //        }
    //    }
    //}

    //public void OnBeginDrag(PointerEventData eventData)
    //{
    //    _isSnapping = false;
    //    _canvasGroup.blocksRaycasts = false;
    //    _canvasGroup.alpha = 0.6f;
    //}

    //public void OnDrag(PointerEventData eventData)
    //{
    //    _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    //}

    //public void OnEndDrag(PointerEventData eventData)
    //{
    //    _canvasGroup.blocksRaycasts = true;
    //    _canvasGroup.alpha = 1f;

    //    float snappedX = Mathf.Round(_rectTransform.anchoredPosition.x / _gridSize) * _gridSize;
    //    float snappedY = Mathf.Round(_rectTransform.anchoredPosition.y / _gridSize) * _gridSize;

    //    _targetPosition = new Vector2 (snappedX, snappedY);
    //    _isSnapping = true;
    //}

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

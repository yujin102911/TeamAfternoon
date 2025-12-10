using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ReturnKeyword : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, 
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public KeywordData Keyword_Data;

    private Image _image;
    private Color _highlightColor = new Color(1f, 1f, 0f, 0.5f); // 강조 색상 (노란색 반투명)
    private Color _originalColor;

    [Header("드래그 복제본")]
    public GameObject dragGhostPrefab; // UI 프리팹 복제본
    private GameObject ghost;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        _image = GetComponent<Image>();
        if (_image != null)
        {
            _originalColor = _image.color;
        }
    }

    private void OnDisable()
    {
        if (ghost != null)
            Destroy(ghost);
        ghost = null;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }

    // 포인터가 UI 요소 위로 들어올 때 호출
    public void OnPointerEnter(PointerEventData eventData)
    {

        if (eventData.pointerDrag != null) return;

        if (_image != null)
        {
            _image.color = _highlightColor;
        }
    }

    // 포인터가 UI 요소 밖으로 나갈 때 호출
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_image != null)
        {
            _image.color = _originalColor;
        }
    }

    // 드래그 시작 시 호출
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그용 복제 생성
        ghost = Instantiate(dragGhostPrefab, canvas.transform);
        ghost.transform.position = transform.position;

        // 드래그 복제본 초기화 세팅
        ghost.GetComponent<Keyword_UI>().SetAndShow(Keyword_Data.KeywordName);


        // 원본은 숨기기 or 투명화
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    // 드래그 중 호출
    public void OnDrag(PointerEventData eventData)
    {
        if (ghost != null)
            ghost.transform.position = eventData.position;
    }

    // 드래그 종료 시 호출
    public void OnEndDrag(PointerEventData eventData)
    {
        Destroy(ghost);
        ghost = null;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Additional_effect_UI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler,
     IDragHandler, IEndDragHandler
{
    public Additional_Effect Additional_Effect;

    [SerializeField]
    private Additional_EffectCell cell;

    [Header("드래그 복제본")]
    public GameObject dragGhostPrefab; // UI 프리팹 복제본
    private GameObject ghost;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    [Header("마우스 호버 설정")]
    [SerializeField]
    private Image _image;
    private Color _originColor;
    private bool _isPointerOver = false;

    

    private void Awake()
    {
        if (_image == null)
            _image = GetComponent<Image>();

        _originColor = _image.color;

        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnDisable()
    {
        Destroy(ghost);
        ghost = null;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }

    public void Init(Additional_Effect _Effect)
    {
        Additional_Effect = _Effect;
        cell.Update_CellVisual(Additional_Effect);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Additional_Effect == null) return;
        if (eventData.pointerDrag != null) return;

        _isPointerOver = true;

        // 색상 변경
        _image.color = new Color(0.9f, 0.9f, 0.9f, 1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerOver = false;
        _image.color = _originColor;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (GameManager.Instance.IsExecutingRound) return;

        // 드래그용 복제 생성
        ghost = Instantiate(dragGhostPrefab, canvas.transform);
        ghost.transform.position = transform.position;

        // 드래그 복제본 초기화 세팅
        ghost.GetComponent<Additional_EffectCell>().Update_CellVisual(Additional_Effect);


        // 원본은 숨기기 or 투명화
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghost != null)
            ghost.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그 종료
        Destroy(ghost);
        ghost = null;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (BattleUIManager.Instance != null)
            BattleUIManager.Instance.HandleBar_raycastOn();
    }
}

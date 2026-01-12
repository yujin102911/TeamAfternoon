using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
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
    private RectTransform canvasRect;
    private RectTransform ghostRect;

    [Header("마우스 호버 설정")]
    [SerializeField]
    private Image _image;
    private Color _originColor;
    private bool _isPointerOver = false;

    [Header("텍스트 설정")]
    [SerializeField]
    private TextMeshProUGUI costText;
    [SerializeField]
    private TextMeshProUGUI nameText;
    [SerializeField]
    private LocalizedString _nameText;

    [Header("자물쇠")]
    [SerializeField]
    private GameObject _lock;
    private bool _isLocked = false;

    private RectTransform rectTransform;

    private void Awake()
    {
        if (_image == null)
            _image = GetComponent<Image>();

        _originColor = _image.color;

        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvasRect = canvas.GetComponent<RectTransform>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (TimelineManager.Instance != null)
        {
            TimelineManager.Instance.OnTextMemoryChanged += SetAlpha;
        }
    }

    private void OnDestroy()
    {
        if (TimelineManager.Instance != null)
        {
            TimelineManager.Instance.OnTextMemoryChanged -= SetAlpha;
        }
    }

    private void OnEnable()
    {
        _nameText.StringChanged += OnNameChanged;
        _nameText.RefreshString();
    }

    private void OnDisable()
    {
        Destroy(ghost);
        ghost = null;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        _nameText.StringChanged -= OnNameChanged;
    }

    private void OnNameChanged(string value) => nameText.text = value;

    public void Init(Additional_Effect _Effect)
    {
        Additional_Effect = _Effect;
        cell.Update_CellVisual(Additional_Effect);

        //nameText.text = Additional_Effect.effectName + ".mfx";
        _nameText.TableEntryReference = Additional_Effect.effectName;
        costText.text = $"{Additional_Effect.cost} <size=15>mb</size>";
    }

    public void SetLock(bool isLocked)
    {
        _isLocked = isLocked;
        _lock.SetActive(_isLocked);

        if (isLocked)
        {
            cell.GetComponent<CanvasGroup>().alpha = 0.5f;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            cell.GetComponent<CanvasGroup>().alpha = 1.0f;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Additional_Effect == null) return;
        if (eventData.pointerDrag != null) return;

        _isPointerOver = true;

        // 색상 변경
        _image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Set_Descript_Text();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerOver = false;
        _image.color = _originColor;

        if (CardTooltip.Instance != null)
        {
            CardTooltip.Instance.Hide();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (GameManager.Instance.IsExecutingRound) return;

        // 드래그용 복제 생성
        ghost = Instantiate(dragGhostPrefab, canvas.transform);
        ghostRect = ghost.GetComponent<RectTransform>();

        // 드래그 복제본 초기화 세팅
        ghost.GetComponent<Additional_EffectCell>().Update_CellVisual(Additional_Effect);

        UpdateGhostPosition(eventData);

        // 원본은 숨기기 or 투명화
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghost != null)
            UpdateGhostPosition(eventData);
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

    // 위치변환 함수
    private void UpdateGhostPosition(PointerEventData eventData)
    {
        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            canvas.worldCamera,   // ⭐ Camera 모드에서는 반드시 필요
            out localPoint
        );

        ghostRect.localPosition = localPoint;
    }

    private void Set_Descript_Text()
    {
        if (CardTooltip.Instance != null)
        {
            var canvas = GetComponentInParent<Canvas>();
            Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                ? canvas.worldCamera
                : null;

            RectTransform rt = transform as RectTransform;
            if (rt == null) return;

            // ✅ Rect 내부의 (0.9, 0.9) 지점(정규화) -> 로컬 좌표로 변환
            Vector2 rectSize = rt.rect.size;
            Vector2 localPoint = new Vector2(
                (0.9f - rt.pivot.x) * rectSize.x,
                (0.9f - rt.pivot.y) * rectSize.y
            );

            // ✅ 로컬 -> 월드 -> 스크린
            Vector3 worldPoint = rt.TransformPoint(localPoint);
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldPoint);

            CardTooltip.Instance.Show_EffectDesc(Additional_Effect, screenPos, cam);
        }
    }

    private void SetAlpha(int current, int max)
    {
        if(_isLocked) return;

        if (current + Additional_Effect.cost > max)
        {
            canvasGroup.alpha = 0.5f;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
    }
}

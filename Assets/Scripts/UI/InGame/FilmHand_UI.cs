using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class FilmHand_UI : Film_UI, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler,
     IDragHandler, IEndDragHandler, IDraggableUI, IPointerClickHandler
{

    public RuntimeBlock RuntimeBlock => base.runtimeBlock;

    [Header("드래그 복제본")]
    public GameObject dragGhostPrefab; // UI 프리팹 복제본
    private GameObject ghost;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private RectTransform canvasRect;
    private RectTransform ghostRect;

    private RectTransform rectTransform;

    [Header("마우스 호버 설정")]
    [SerializeField]
    private Image _image;
    private Color _originColor;
    private bool _isPointerOver = false;

    [Header("튜토리얼 여부")]
    [SerializeField] private bool isTutorial = false;



    private void Awake()
    {
        if (_image == null)
            _image = GetComponent<Image>();

        _originColor = _image.color;

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        isTutorial = GameManager.Instance.IsTutorial;
    }

    private void Update()
    {
        if (!isTutorial)
        {
            int num = GetNumberKeyDown();
            if (num >= 1 && num <= 9 && _isPointerOver && TimelineManager.Instance != null)
            {
                bool tryPlace = TimelineManager.Instance.TryPlaceBlock(RuntimeBlock, num);
            }
        }
        
    }
    int GetNumberKeyDown()
    {
        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) ||
                Input.GetKeyDown(KeyCode.Keypad0 + i))
                return i;
        }
        return -1;
    }


    private void OnDisable()
    {
        Destroy(ghost);
        ghost = null;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }


    public override void Init(RuntimeBlock rBlock)
    {
        canvasGroup.alpha = 1f;
        base.Init(rBlock);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (runtimeBlock == null) return;
        if (eventData.pointerDrag != null) return;

        _isPointerOver = true;

        // 색상 변경
        _image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

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

            CardTooltip.Instance.ShowAction(RuntimeBlock, screenPos, cam);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerOver = false;
        _image.color = _originColor;
        CardTooltip.Instance.Hide();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (GameManager.Instance.IsExecutingRound) return;

        // 드래그용 복제 생성
        ghost = Instantiate(dragGhostPrefab, canvas.transform);
        ghostRect = ghost.GetComponent<RectTransform>();

        // 드래그 복제본 초기화 세팅
        ghost.GetComponent<Film_UI>().Show(runtimeBlock);

        UpdateGhostPosition(eventData);

        // 원본은 숨기기 or 투명화
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        // 클립 들 때 사운드
        if (SoundManager.Instance != null)
            SoundManager.Instance.Play(SoundID.Clip_PickUp);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (GameManager.Instance.IsExecutingRound) return;

        if (ghost != null)
            UpdateGhostPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (GameManager.Instance.IsExecutingRound) return;

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

    public void OnPointerClick(PointerEventData eventData)
    {
        //if (eventData.button == PointerEventData.InputButton.Right)
        //{
        //    ToggleFavorite();
        //}
    }

    private void ToggleFavorite()
    {
        if (runtimeBlock == null) return;

        runtimeBlock.IsFavorite = !runtimeBlock.IsFavorite;

        //여기에 별 아이콘 토글 코드 추가 가능

        UserGameData userData = GameManager.Instance.UserGameData;
        Saved_BlockData saved = userData.Unlocked_Blocks.Find(b => b.Owner_blockID == runtimeBlock.BlockID);

        if (saved != null)
        {
            saved.IsFavorite = runtimeBlock.IsFavorite;
            Debug.Log($"[Favorite] 블록 {runtimeBlock.BlockID} : {saved.IsFavorite}");
        }
        GetComponentInParent<FilmHand_Panel>().UpdateHandUI(new List<RuntimeBlock>(TimelineManager.Instance.CurrentHand));
    }
}

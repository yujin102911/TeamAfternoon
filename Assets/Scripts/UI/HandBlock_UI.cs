using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VInspector;


public class HandBlock_UI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler,
     IDragHandler, IEndDragHandler
{
    public RuntimeBlock runtimeBlock; // 원본 블록 (항상 유지)


    [Header("참조")]
    public TextMeshProUGUI BlockNameText;
    public TextMeshProUGUI BlockInfoText;
    public TextMeshProUGUI BlockDamageText;
    public GameObject[] _tickCells;        // 틱 셀 프리팹

    [Header("부분 블록 설정")]
    [Tooltip("각 버튼을 눌렀을 때 드래그할 RuntimeBlock들 (자동으로 로드됨)")]
    public RuntimeBlock[] partialBlocks; // BlockData.partialBlockDatas에서 자동 생성

    [Header("드래그 복제본")]
    public GameObject dragGhostPrefab; // UI 프리팹 복제본
    private GameObject ghost;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    [TabGroup("Attack")]
    [SerializeField]
    private Color _attackColor;
    [TabGroup("Attack")]
    public Sprite Sword_icon;

    [TabGroup("Move")]
    [SerializeField]
    private Color _moveColor;
    [TabGroup("Move")]
    public Sprite CW_icon;
    [TabGroup("Move")]
    public Sprite CCW_icon;

    [TabGroup("Cure")]
    [SerializeField]
    private Color[] _cureColors;
    [TabGroup("Cure")]
    public Sprite[] _cureIcons;

    [TabGroup("None")]
    [SerializeField]
    private Color _noneColor;

    [Header("드래그할 블록 선택")]
    private RuntimeBlock _selectedBlockForDrag; // 버튼 클릭 시 선택된 블록
    private RuntimeBlock _originalRuntimeBlock; // 원본 보관용 (항상 유지)

    [Header("배치 상태")]
    private bool _isPlaced = false; // 배치되었는지 여부
    private PlacedBlock _currentPlacedBlock; // 현재 배치된 블록 참조


    private ScrollRect parentScroll;
    private RectTransform rectTransform;

    [Header("마우스 호버 설정")]
    [SerializeField]
    private Image _image;
    private Color _originColor;

    private void Awake()
    {
        if (_image == null)
            _image = GetComponent<Image>();

        _originColor = _image.color;

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        parentScroll = GetComponentInParent<ScrollRect>();
    }

    private void OnDisable()
    {
        Destroy(ghost);
        ghost = null;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }

    void Start()
    {

    }

    void Update()
    {

    }

    public void Init(RuntimeBlock rBlock)
    {
        canvasGroup.alpha = 1f;
        runtimeBlock = rBlock;
        _originalRuntimeBlock = rBlock; // 원본 백업 (항상 유지)
        BlockData data = rBlock.BaseData;

        //블럭 이름 설정
        if (BlockNameText != null) BlockNameText.text = "~" + data.BlockName + "~";

        BlockDamageText.text = "데미지: " + data.attackDamage.ToString();

        //틱 정보 설정
        UpdateTickVisuals(data);

        //키워드 표시 설정
        Update_KeywordText();

        // ⭐ 부분 블록 로드 (BlockData.partialBlockDatas에서)
        LoadPartialBlocksFromBlockData();

        // 버튼 이벤트 연결
        SetupTickCellButtons();

        // 기본값: 전체 블록 선택
        _selectedBlockForDrag = runtimeBlock;

        // 배치 상태 초기화
        _isPlaced = false;
        _currentPlacedBlock = null;

        // TimelineManager에 자신을 등록
        if (TimelineManager.Instance != null)
        {
            TimelineManager.Instance.RegisterHandBlockUI(this);
        }
    }

    private void OnDestroy()
    {
        // TimelineManager에서 등록 해제
        if (TimelineManager.Instance != null)
        {
            TimelineManager.Instance.UnregisterHandBlockUI(this);
        }
    }

    /// <summary>
    /// BlockData의 partialBlockDatas에서 RuntimeBlock 배열 생성
    /// </summary>
    void LoadPartialBlocksFromBlockData()
    {
        BlockData baseData = runtimeBlock.BaseData;

        // BlockData에 부분 블록 리스트가 있는지 확인
        if (baseData.PartialBlockDatas == null || baseData.PartialBlockDatas.Length == 0)
        {
            Debug.LogWarning($"[HandBlock_UI] {baseData.BlockName}에 부분 블록이 설정되지 않았습니다!");
            // 폴백: 원본 블록만 사용
            partialBlocks = new RuntimeBlock[] { runtimeBlock };
            return;
        }

        // BlockData 배열 → RuntimeBlock 배열 변환
        int partialCount = baseData.PartialBlockDatas.Length;
        partialBlocks = new RuntimeBlock[partialCount];

        for (int i = 0; i < partialCount; i++)
        {
            BlockData partialBlockData = baseData.PartialBlockDatas[i];

            if (partialBlockData == null)
            {
                Debug.LogWarning($"[HandBlock_UI] {baseData.BlockName}의 {i}번째 부분 블록이 null입니다!");
                continue;
            }

            // RuntimeBlock 생성
            partialBlocks[i] = new RuntimeBlock(partialBlockData);

            // 키워드 복사 (원본 블록의 키워드를 부분 블록에도 적용)
            foreach (KeywordData keyword in runtimeBlock.AttachedKeywords)
            {
                partialBlocks[i].AttachedKeywords.Add(keyword);
            }
        }

        Debug.Log($"[HandBlock_UI] {baseData.BlockName} 부분 블록 {partialCount}개 로드 완료!");
    }

    /// <summary>
    /// 각 틱 셀에 버튼 컴포넌트 추가 및 이벤트 연결
    /// </summary>
    private void SetupTickCellButtons()
    {
        if (_tickCells == null) return;

        for (int i = 0; i < _tickCells.Length; i++)
        {
            GameObject cell = _tickCells[i];
            if (!cell.activeSelf) continue;

            // Button 컴포넌트 가져오거나 추가
            Button btn = cell.GetComponent<Button>();
            if (btn == null)
                btn = cell.AddComponent<Button>();

            // 기존 리스너 제거
            btn.onClick.RemoveAllListeners();

            int cellIndex = i; // 클로저 캡처용
            btn.onClick.AddListener(() => OnTickCellClicked(cellIndex));
        }
    }

    /// <summary>
    /// 틱 셀 버튼 클릭 시 호출
    /// </summary>
    private void OnTickCellClicked(int clickedIndex)
    {
        if (GameManager.Instance.IsExecutingRound) return;
        if (_isPlaced) return; // 이미 배치된 경우 무시

        // partialBlocks 배열에서 해당하는 부분 블록 선택
        if (partialBlocks != null && clickedIndex < partialBlocks.Length)
        {
            _selectedBlockForDrag = partialBlocks[clickedIndex];

            string blockName = _selectedBlockForDrag?.BaseData?.BlockName ?? "Unknown";
            Debug.Log($"[HandBlock_UI] {clickedIndex}번 버튼 클릭 → {blockName} 선택됨");
        }
        else
        {
            Debug.LogWarning($"[HandBlock_UI] {clickedIndex}번 인덱스에 할당된 부분 블록이 없습니다!");
            _selectedBlockForDrag = runtimeBlock; // 기본값으로 폴백
        }
    }

    private void UpdateTickVisuals(BlockData data)
    {
        if (_tickCells == null) return;

        // 기존 틱 셀 비활성화
        foreach (GameObject tickcell in _tickCells) tickcell.SetActive(false);

        for (int i = 0; i < data.blockLength; i++)
        {
            GameObject cell = _tickCells[i];

            if (!cell.activeSelf) cell.SetActive(true);

            Image img = cell.GetComponent<Image>();
            TextMeshProUGUI txt = cell.GetComponentInChildren<TextMeshProUGUI>();
            Image iconImage = cell.transform.Find("Icon")?.GetComponent<Image>();

            Sprite iconSprite = null;

            ActionType action = data.GetEffectAt(i);
            if (txt)
            {
                if (action == ActionType.Attack)
                {
                    txt.text = "▲";
                    img.color = new Color(_attackColor.r, _attackColor.g, _attackColor.b, 1.0f);
                    iconSprite = Sword_icon;
                }
                else if (action == ActionType.Move)
                {
                    txt.text = ">";
                    img.color = new Color(_moveColor.r, _moveColor.g, _moveColor.b, 1.0f);
                    iconSprite = CW_icon;
                }
                else if (action == ActionType.Cure)
                {
                    txt.text = "";
                    int index = data.CalCulate_CurePower(i) - 1;
                    Color CureColor = _cureColors[index];
                    img.color = new Color(CureColor.r, CureColor.g, CureColor.b, 1.0f);
                    iconSprite = _cureIcons[index];
                }
                else
                {
                    txt.text = "-";
                    img.color = new Color(_noneColor.r, _noneColor.g, _noneColor.b, 1.0f);
                }
            }

            if (iconSprite != null)
            {
                txt.text = "";
                iconImage.sprite = iconSprite;
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }
    }

    private void Update_KeywordText()
    {
        BlockInfoText.text = "";

        foreach (KeywordData keyword in runtimeBlock.AttachedKeywords)
        {
            BlockInfoText.text += $"#{keyword.KeywordName}\n";
        }
    }

    // ========================
    // 호버 → 툴팁
    // ========================
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (runtimeBlock == null) return;
        if (CardTooltip.Instance == null) return;
        if (eventData.pointerDrag != null) return;

        // 색상 변경
        _image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        bool hasSpecial = runtimeBlock.AttachedKeywords != null && runtimeBlock.AttachedKeywords.Count > 0;
        if (!hasSpecial)
            return;

        string effectTitle = "";


        string body = BuildTooltipText();

        // 캔버스에 연결된 카메라 사용 (Screen Space - Camera 대응)
        Camera cam = canvas != null ? canvas.worldCamera : Camera.main;

        // 카드 Rect의 오른쪽 중앙 월드 좌표
        Vector3 worldBottomCenter = rectTransform.TransformPoint(
            new Vector3(rectTransform.rect.width * 0.6f, rectTransform.rect.height * 0.5f, 0f)
        );

        // 월드 → 스크린 좌표
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldBottomCenter);

        int index = 0;

        foreach (KeywordData keyword in runtimeBlock.AttachedKeywords)
        {
            if (keyword == null) continue;

            // 이름 세팅
            effectTitle = $"#{keyword.KeywordName}";
            body = $"{keyword.KeywordDescription}";

            CardTooltip.Instance.Show(
            effectTitle,
            body,
            screenPos,
            cam,
            index
            );

            index++;
        }


    }

    private string BuildTooltipText()
    {
        List<string> lines = new List<string>();

        if (runtimeBlock.AttachedKeywords != null)
        {
            int index = 1;
            foreach (KeywordData keyword in runtimeBlock.AttachedKeywords)
            {
                if (keyword == null) continue;

                // 이름 제외 → 설명만
                if (!string.IsNullOrEmpty(keyword.KeywordDescription))
                    lines.Add($"{index}. {keyword.KeywordDescription}");
                index++;
            }
        }

        return string.Join("\n", lines);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _image.color = _originColor;

        if (CardTooltip.Instance != null)
        {
            CardTooltip.Instance.Hide();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (GameManager.Instance.IsExecutingRound) return;
        if (_isPlaced) return; // 이미 배치된 경우 드래그 불가

        // 선택된 블록이 없으면 전체 블록 사용
        if (_selectedBlockForDrag == null)
            _selectedBlockForDrag = runtimeBlock;

        // 드래그용 복제 생성
        ghost = Instantiate(dragGhostPrefab, canvas.transform);
        ghost.transform.position = transform.position;

        // 드래그 복제본 초기화 세팅 (선택된 블록으로)
        Draggable_Block draggable = ghost.GetComponent<Draggable_Block>();
        draggable.Show(_selectedBlockForDrag);

        // ⭐ Draggable_Block에 HandBlock_UI 참조 전달
        draggable.SetSourceHandBlock(this);

        // 원본은 숨기기 or 투명화
        canvasGroup.alpha = 0.3f; // 완전히 숨기지 않고 반투명
        canvasGroup.blocksRaycasts = false;

        if (SoundManager.Instance != null)
            SoundManager.Instance.Play(SoundID.SFX_Spell_Cancle);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghost != null)
            ghost.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드롭 성공 여부 확인
        bool dropSuccess = CheckDrop(eventData);

        if (!dropSuccess)
        {
            // 드롭 실패 시 원래대로 복구
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            _selectedBlockForDrag = null;
        }
        // dropSuccess인 경우 HideBlock()이 Draggable_Block에서 호출됨

        // 드래그 종료
        Destroy(ghost);
        ghost = null;
    }

    private bool CheckDrop(PointerEventData eventData)
    {
        // 드랍 성공 여부 계산 (슬롯 태그/존 충돌/레이트레이캐스트 등)
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var hit in results)
        {
            // 원하는 컴포넌트가 붙어있다면 성공
            if (hit.gameObject.GetComponent<TimelineDropZone>() != null)
                return true;
        }

        return false;
    }

    // ========================================
    // 공개 메서드 (외부 호출용)
    // ========================================

    /// <summary>
    /// 배치 성공 시 호출 - HandBlock_UI 숨김
    /// </summary>
    public void HideBlock(PlacedBlock placedBlock)
    {
        _isPlaced = true;
        _currentPlacedBlock = placedBlock;

        // UI 숨김
        gameObject.SetActive(false);

        Debug.Log($"[HandBlock_UI] {_originalRuntimeBlock.BaseData.BlockName} 숨김 (배치됨)");
    }

    /// <summary>
    /// 배치 해제 시 호출 - HandBlock_UI 복구 (원본 블록으로)
    /// </summary>
    public void RestoreBlock()
    {
        _isPlaced = false;
        _currentPlacedBlock = null;
        _selectedBlockForDrag = null;

        // 원본 블록으로 복구
        runtimeBlock = _originalRuntimeBlock;

        // UI 복구
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        Debug.Log($"[HandBlock_UI] {_originalRuntimeBlock.BaseData.BlockName} 복구됨");
    }

    /// <summary>
    /// 현재 선택된 드래그 블록 가져오기
    /// </summary>
    public RuntimeBlock GetSelectedBlock()
    {
        return _selectedBlockForDrag ?? runtimeBlock;
    }

    /// <summary>
    /// 원본 블록 가져오기
    /// </summary>
    public RuntimeBlock GetOriginalBlock()
    {
        return _originalRuntimeBlock;
    }

    /// <summary>
    /// 배치 여부 확인
    /// </summary>
    public bool IsPlaced()
    {
        return _isPlaced;
    }

    /// <summary>
    /// 현재 배치된 PlacedBlock 가져오기
    /// </summary>
    public PlacedBlock GetPlacedBlock()
    {
        return _currentPlacedBlock;
    }
}
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// ========================================
// 드래그 가능한 카드 UI
// ========================================
public class Draggable_Block : MonoBehaviour
{
    [Header("참조")]
    public GameObject[] _tickCells;        // 틱 셀 프리팹

    [Header("설명창")]
    [SerializeField]
    private TextMeshProUGUI _blockNameText;
    [SerializeField]
    private TextMeshProUGUI _keywordNameText;

    [Header("드래그 설정")]
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;
    private bool isDragging = false;

    [Header("색상 설정")]
    [SerializeField]
    private Color _attackColor;
    [SerializeField]
    private Color _moveColor;
    [SerializeField]
    private Color _noneColor;

    [Header("빌딩인지 체크")]
    public bool isBuildingPhase = false; // 빌딩 페이즈인지 여부

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        Hide();
    }

    public void Show(BlockData data)
    {
        gameObject.SetActive(true);

        //틱 정보 설정
        Set_TickVisuals(data);
    }

    public void Show(RuntimeBlock r_block)
    {
        gameObject.SetActive(true);

        //틱 정보 설정
        Set_TickVisuals(r_block.BaseData);

        SetText(r_block);
    }

    private void Set_TickVisuals(BlockData data)
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

            ActionType action = data.GetEffectAt(i);
            if (txt)
            {
                if (action == ActionType.Attack)
                {
                    txt.text = "▲";
                    img.color = new Color(_attackColor.r, _attackColor.g, _attackColor.b, 1.0f);
                }
                else if (action == ActionType.Move)
                {
                    txt.text = ">"; // TODO: 추후에 동그란 화살표 모양으로 바꿔야됨
                    img.color = new Color(_moveColor.r, _moveColor.g, _moveColor.b, 1.0f);
                }
                else
                {
                    txt.text = "-";
                    img.color = new Color(_noneColor.r, _noneColor.g, _noneColor.b, 1.0f);
                }
            }
        }
    }

    private void SetText(RuntimeBlock r_block)
    {
        // 이름 설정
        _blockNameText.text = r_block.BaseData.blockName;

        _keywordNameText.text = "";
        // 키워드 설정
        if (r_block.AttachedKeywords.Count > 0)
        {
            for (int i = 0; i < r_block.AttachedKeywords.Count; i++)
            {
                _keywordNameText.text += $"#{r_block.AttachedKeywords[i].KeywordName} ";
            }
        }
        else
        {
            _keywordNameText.text = "";
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    ///// <summary>
    ///// 블럭 데이터 설정
    ///// </summary>
    //public void SetCard(BlockData blockData)
    //{
    //    BlockData = blockData;
    //    UpdateVisual();
    //}

    ///// <summary>
    ///// 효과 지우기(추후 수정)
    ///// </summary>
    //public void EffectClear()
    //{
    //    //card.specialEffects.Clear();
    //    UpdateVisual();
    //}

    ///// <summary>
    ///// 카드 비주얼 업데이트
    ///// </summary>
    //private void UpdateVisual()
    //{
    //    if (BlockData == null) return;

    //    // 카드 너비를 틱 길이에 맞춰 자동 조정
    //    if (rectTransform != null)
    //    {
    //        Vector2 sizeDelta = rectTransform.sizeDelta;
    //        sizeDelta.x = BlockData.blockLength * tickWidth - 10; // 10은 여백
    //        rectTransform.sizeDelta = sizeDelta;
    //    }

    //    // 카드 이름
    //    if (BlockNameText != null)
    //    {
    //        BlockNameText.text = BlockData.blockName;
    //    }

    //    // 틱 셀 생성 (각 틱마다 효과 표시)
    //    if (tickContainer != null && _tickCellPrefab != null)
    //    {
    //        // 기존 셀 제거
    //        foreach (Transform child in tickContainer)
    //        {
    //            Destroy(child.gameObject);
    //        }

    //        // 새로운 셀 생성
    //        for (int i = 0; i < BlockData.blockLength; i++)
    //        {
    //            GameObject cell = Instantiate(_tickCellPrefab, tickContainer);

    //            Image cellImage = cell.GetComponent<Image>();
    //            TextMeshProUGUI cellText = cell.GetComponentInChildren<TextMeshProUGUI>();
    //            Image iconImage = cell.transform.Find("Icon")?.GetComponent<Image>(); // ⬅ 자식 Image

    //            // 기본 초기화
    //            if (cellText != null) cellText.text = "";
    //            if (iconImage != null)
    //            {
    //                iconImage.enabled = false;
    //                iconImage.sprite = null;
    //            }

    //            ActionType effect = BlockData.actionTypes[i];

    //            // 효과에 따라 색상 및 아이콘 설정
    //            switch (effect)
    //            {
    //                case ActionType.None:
    //                    if (cellImage != null) cellImage.color = new Color(0.3f, 0.3f, 0.3f);
    //                    if (cellText != null) cellText.text = "-";
    //                    break;

    //                case ActionType.Attack:
    //                    if (cellImage != null) cellImage.color = new Color(1f, 0.3f, 0.3f);
    //                    if (cellText != null) cellText.text = "▲";
    //                    break;

    //                case ActionType.Move:
    //                    if (cellImage != null) cellImage.color = new Color(0.3f, 0.7f, 1f);
    //                    if (cellText != null) cellText.text = "↻";
    //                    break;

    //                //case ActionType.Parrying:
    //                    //if (cellImage != null) cellImage.color = new Color(1f, 0.85f, 0.2f); // 취향대로 바꿔도 됨
    //                    //if (iconImage != null && parryIcon != null)
    //                    //{
    //                    //    iconImage.enabled = true;
    //                    //    iconImage.sprite = parryIcon; // ⬅ 스프라이트로 표시
    //                    //}
    //                    //break;
    //            }
    //        }
    //    }

    //    // 카드 정보 (데미지, 이동 등)
    //    if (BlockInfoText != null)
    //    {
    //        List<string> info = new List<string>();
    //        //info.Add($"<b>[-EN{card.cardCost}]</b>");
    //        //// 이동 횟수 계산
    //        //int moveCount = 0;
    //        //foreach (EffectType effect in card.actionEffects)
    //        //{
    //        //    if (effect == EffectType.Move) moveCount++;
    //        //}
    //        //if (moveCount > 0)
    //        //{
    //        //    info.Add($"이동: {moveCount}칸");
    //        //}

    //        // 공격이 있으면 데미지 표시
    //        if (BlockData.HasAttack())
    //        {
    //            info.Add($"데미지: {BlockData.attackDamage}");
    //        }


    //        //옛날 코드 보관
    //        //if (card.attackDamage != null)
    //        //{
    //        //    info.Add($"데미지: {card.attackDamage}");
    //        //}


    //        //특수 효과 설명
    //        //if (BlockData.specialEffects != null)
    //        //{
    //        //    foreach (CardEffect effect in BlockData.specialEffects)
    //        //    {
    //        //        if (effect != null && string.IsNullOrEmpty(effect.description) == false)
    //        //        {
    //        //            //info.Add($"● {effect.description}");
    //        //            info.Add($"#{effect.effectName}");
    //        //        }
    //        //    }
    //        //}

    //        BlockInfoText.text = string.Join("\n", info);
    //    }
    //}

    // ========================
    // 호버 → 툴팁
    // ========================
    //public void OnPointerEnter(PointerEventData eventData)
    //{ }
    ////    if (BlockData == null) return;
    ////    if (CardTooltip.Instance == null) return;

    //    //    //bool hasSpecial = card.specialEffects != null && card.specialEffects.Count > 0;
    //    //    //if (tooltipOnlyForSpecial && !hasSpecial)
    //    //    //    return;

    //    //    string effectTitle = "";
    //    //    //if (card.specialEffects != null && card.specialEffects.Count > 0)
    //    //    //{
    //    //    //    foreach (CardEffect effect in card.specialEffects)
    //    //    //    {
    //    //    //        if (effect == null) continue;

    //    //    //        // 이름 나열
    //    //    //        if (!string.IsNullOrEmpty(effect.effectName))
    //    //    //            effectTitle += $"#{effect.effectName} ";
    //    //    //    }
    //    //    //}

    //    //    string body = "";
    //    //    //body = BuildTooltipText();

    //    //    // ✅ 캔버스에 연결된 카메라 사용 (Screen Space - Camera 대응)
    //    //    Camera cam = canvas != null ? canvas.worldCamera : Camera.main;

    //    //    // 카드 Rect의 오른쪽 중앙 월드 좌표
    //    //    Vector3 worldBottomCenter = rectTransform.TransformPoint(
    //    //        new Vector3(rectTransform.rect.width * 0.75f, rectTransform.rect.height * 0.5f, 0f)
    //    //    );

    //    //    // 월드 → 스크린 좌표
    //    //    Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldBottomCenter);

    //    //    CardTooltip.Instance.Show(
    //    //        effectTitle,
    //    //        body,
    //    //        screenPos,
    //    //        cam
    //    //    );
    //    //}


    //public void OnPointerExit(PointerEventData eventData)
    //{
    //    if (CardTooltip.Instance != null)
    //    {
    //        CardTooltip.Instance.Hide();
    //    }
    //}

    //private string BuildTooltipText()
    //{
    //    List<string> lines = new List<string>();

    //    //if (card.specialEffects != null)
    //    //{
    //    //    int index = 1;
    //    //    foreach (CardEffect effect in card.specialEffects)
    //    //    {
    //    //        if (effect == null) continue;

    //    //        // 이름 제외 → 설명만
    //    //        if (!string.IsNullOrEmpty(effect.description))
    //    //            lines.Add($"{index}. {effect.description}");
    //    //        index++;
    //    //    }
    //    //}

    //    return string.Join("\n", lines);
    //}

    ///// <summary>
    ///// 드래그 시작
    ///// </summary>
    //public void OnBeginDrag(PointerEventData eventData)
    //{
    //    // 좌클릭만 허용
    //    if (eventData.button != PointerEventData.InputButton.Left)
    //        return;

    //    // 타임라인 실행 중에는 드래그 불가
    //    //if (TimeLine_Manager.Instance != null && TimeLine_Manager.Instance.IsExecutingRound)
    //    //    return;

    //    isDragging = true;
    //    originalPosition = rectTransform.anchoredPosition;
    //    originalParent = transform.parent;

    //    // 드래그 중에는 반투명하게
    //    canvasGroup.alpha = 0.6f;
    //    canvasGroup.blocksRaycasts = false;

    //    // 최상위로 이동 (다른 UI 위에 표시)
    //    transform.SetParent(canvas.transform);

    //    // 드래그 시작하면 툴팁은 숨김
    //    if (CardTooltip.Instance != null)
    //    {
    //        CardTooltip.Instance.Hide();
    //    }
    //}

    ///// <summary>
    ///// 드래그 중
    ///// </summary>
    //public void OnDrag(PointerEventData eventData)
    //{
    //    if (eventData.button != PointerEventData.InputButton.Left)
    //        return;

    //    // 타임라인 실행 중에는 드래그 불가
    //    //if (TimeLine_Manager.Instance != null && TimeLine_Manager.Instance.IsExecutingRound)
    //    //    return;

    //    // 마우스 위치로 이동
    //    rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    //}

    ///// <summary>
    ///// 드래그 종료
    ///// </summary>
    //public void OnEndDrag(PointerEventData eventData)
    //{
    //    if (eventData.button != PointerEventData.InputButton.Left)
    //        return;

    //    // 타임라인 실행 중에는 드래그 불가
    //    //if (TimeLine_Manager.Instance != null && TimeLine_Manager.Instance.IsExecutingRound)
    //    //    return;

    //    isDragging = false;
    //    canvasGroup.alpha = 1f;
    //    canvasGroup.blocksRaycasts = true;

    //    // 드롭 성공 여부 확인은 TimelineDropZone에서 처리

    //    // 원래 위치로 복귀
    //    transform.SetParent(originalParent);
    //    rectTransform.anchoredPosition = originalPosition;
    //}

    //public void OnDrop(PointerEventData eventData)
    //{
    //    // 드래그 중인 카드가 있으면 하이라이트
    //    if (eventData.pointerDrag != null)
    //    {
    //        //DraggableEffect draggable = eventData.pointerDrag.GetComponent<DraggableEffect>();

    //        //// 빌딩 페이즈이고 드래그된 게 효과면 카드에 효과 추가
    //        //if (draggable != null && isBuildingPhase)
    //        //{
    //        //    //효과 추가
    //        //    card.specialEffects.Add(draggable.Effect);
    //        //    //이미지 업데이트
    //        //    UpdateVisual();
    //        //}

    //    }
    //}

    ///// <summary>
    ///// 우클릭 시 취소
    ///// </summary>
    //void Update()
    //{
    //    // 이 카드가 드래그 중이고 우클릭하면 취소
    //    if (isDragging && Input.GetMouseButtonDown(1))
    //    {
    //        // 드래그 취소
    //        transform.SetParent(originalParent);
    //        rectTransform.anchoredPosition = originalPosition;
    //        canvasGroup.alpha = 1f;
    //        canvasGroup.blocksRaycasts = true;
    //        isDragging = false;

    //        if (CardTooltip.Instance != null)
    //        {
    //            CardTooltip.Instance.Hide();
    //        }
    //    }
    //}
}

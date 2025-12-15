using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandBlock_UI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler,
     IDragHandler, IEndDragHandler
{
    public RuntimeBlock runtimeBlock;

    [Header("참조")]
    public TextMeshProUGUI BlockNameText;
    public TextMeshProUGUI BlockInfoText;
    public TextMeshProUGUI BlockDamageText;
    public GameObject[] _tickCells;        // 틱 셀 프리팹

    [Header("드래그 복제본")]
    public GameObject dragGhostPrefab; // UI 프리팹 복제본
    private GameObject ghost;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    [Header("색상 설정")]
    [SerializeField]
    private Color _attackColor;
    [SerializeField]
    private Color _moveColor;
    [SerializeField]
    private Color _noneColor;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(RuntimeBlock rBlock)
    {
        canvasGroup.alpha = 1f;
        runtimeBlock = rBlock;
        BlockData data = rBlock.BaseData;

        //블럭 이름 설정
        if (BlockNameText != null) BlockNameText.text = data.BlockName;

        BlockDamageText.text = "데미지: " + data.attackDamage.ToString();

        //틱 정보 설정
        UpdateTickVisuals(data);

        //키워드 표시 설정
        Update_KeywordText();
    }

    private void UpdateTickVisuals(BlockData data)
    {
        if (_tickCells == null) return;

        // 기존 틱 셀 비활성화
        foreach (GameObject tickcell in _tickCells) tickcell.SetActive(false);

        for (int i = 0; i < data.blockLength; i++)
        {
            GameObject cell = _tickCells[i];

            if(!cell.activeSelf) cell.SetActive(true);

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

        // 드래그용 복제 생성
        ghost = Instantiate(dragGhostPrefab, canvas.transform);
        ghost.transform.position = transform.position;

        // 드래그 복제본 초기화 세팅
        ghost.GetComponent<Draggable_Block>().Show(runtimeBlock);


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
        // 지금은 테스트용
        return false;
    }
}

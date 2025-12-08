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
    public GameObject[] _tickCells;        // 틱 셀 프리팹

    [Header("드래그 복제본")]
    public GameObject dragGhostPrefab; // UI 프리팹 복제본
    private GameObject ghost;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private ScrollRect parentScroll;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        parentScroll = GetComponentInParent<ScrollRect>();
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
        runtimeBlock = rBlock;
        BlockData data = rBlock.BaseData;

        //블럭 이름 설정
        if (BlockNameText != null) BlockNameText.text = data.BlockName;

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
                    img.color = new Color(1f, 0.3f, 0.3f);
                }
                else if (action == ActionType.Move)
                {
                    txt.text = ">"; // TODO: 추후에 동그란 화살표 모양으로 바꿔야됨
                    img.color = new Color(0.3f, 0.7f, 1f);
                }
                else
                {
                    txt.text = "-";
                    img.color = new Color(0.3f, 0.3f, 0.3f, 1f);
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
    { }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CardTooltip.Instance != null)
        {
            CardTooltip.Instance.Hide();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("드래그 시작");
        
        // 드래그용 복제 생성
        ghost = Instantiate(dragGhostPrefab, canvas.transform);
        ghost.transform.position = transform.position;

        // 드래그 복제본 초기화 세팅
        ghost.GetComponent<Draggable_Block>().Init(runtimeBlock);

        // 원본은 숨기기 or 투명화
        canvasGroup.alpha = 0f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghost != null)
            ghost.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        bool dropSuccess = CheckDrop(eventData);

        if (dropSuccess)
        {
            // 🟢 성공 → ghost만 남기고 원본 제거
            Destroy(gameObject);
        }
        else
        {
            // 🔴 실패 → ghost 삭제, 원본 되돌리기
            Destroy(ghost);
            canvasGroup.alpha = 1f;
        }
    }

    private bool CheckDrop(PointerEventData eventData)
    {
        // 드랍 성공 여부 계산 (슬롯 태그/존 충돌/레이트레이캐스트 등)
        // 지금은 테스트용
        return false;
    }
}

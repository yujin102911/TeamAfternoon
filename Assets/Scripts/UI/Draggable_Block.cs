using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Draggable_Block : MonoBehaviour, IEndDragHandler
{
    public RuntimeBlock runtimeBlock; // 드래그 중인 블록 (부분 블록 가능)

    [Header("참조")]
    public GameObject[] _tickCells;
    public TextMeshProUGUI BlockNameText;

    [Header("색상 설정")]
    public Color _attackColor;
    public Color _moveColor;
    public Color[] _cureColors;
    public Color _noneColor;

    [Header("아이콘")]
    public Sprite Sword_icon;
    public Sprite CW_icon;
    public Sprite CCW_icon;
    public Sprite[] _cureIcons;

    private Canvas canvas;

    // ⭐ HandBlock_UI 참조 (배치 성공 시 숨김 처리용)
    private HandBlock_UI _sourceHandBlock;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    /// <summary>
    /// 드래그 블록 표시 (HandBlock_UI에서 호출)
    /// </summary>
    public void Show(RuntimeBlock rBlock)
    {
        runtimeBlock = rBlock;

        // ⭐ 디버깅 로그
        Debug.Log($"[Draggable_Block] Show 호출됨 - 블록: {rBlock.BaseData.BlockName}, 길이: {rBlock.BaseData.BlockLength}");

        Set_TickVisuals(rBlock.BaseData);
        SetText(rBlock.BaseData);
    }

    /// <summary>
    /// BlockData로 블록 표시 (오버로드)
    /// </summary>
    public void Show(BlockData blockData)
    {
        Set_TickVisuals(blockData);
        SetText(blockData);
    }

    /// <summary>
    /// 소스 HandBlock_UI 설정 ⭐ NEW!
    /// </summary>
    public void SetSourceHandBlock(HandBlock_UI handBlockUI)
    {
        _sourceHandBlock = handBlockUI;
    }

    private void Set_TickVisuals(BlockData blockData)
    {
        if (_tickCells == null) return;

        // 모든 셀 비활성화
        foreach (GameObject cell in _tickCells)
        {
            cell.SetActive(false);
        }

        // 블록 길이만큼 활성화 및 색상 설정
        for (int i = 0; i < blockData.blockLength; i++)
        {
            GameObject cell = _tickCells[i];
            cell.SetActive(true);

            Image img = cell.GetComponent<Image>();
            TextMeshProUGUI txt = cell.GetComponentInChildren<TextMeshProUGUI>();
            Image iconImage = cell.transform.Find("Icon")?.GetComponent<Image>();

            Sprite iconSprite = null;

            ActionType action = blockData.GetEffectAt(i);

            switch (action)
            {
                case ActionType.Attack:
                    img.color = new Color(_attackColor.r, _attackColor.g, _attackColor.b, 1.0f);
                    txt.text = "▲";
                    iconSprite = Sword_icon;
                    break;

                case ActionType.Move:
                    img.color = new Color(_moveColor.r, _moveColor.g, _moveColor.b, 1.0f);
                    txt.text = ">";
                    iconSprite = CW_icon;
                    break;

                case ActionType.Cure:
                    txt.text = "";
                    int index = blockData.CalCulate_CurePower(i) - 1;
                    Color cureColor = _cureColors[index];
                    img.color = new Color(cureColor.r, cureColor.g, cureColor.b, 1.0f);
                    iconSprite = _cureIcons[index];
                    break;

                case ActionType.None:
                    img.color = new Color(_noneColor.r, _noneColor.g, _noneColor.b, 1.0f);
                    txt.text = "-";
                    break;
            }

            // 아이콘 적용
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

    private void SetText(BlockData blockData)
    {
        if (BlockNameText != null)
        {
            BlockNameText.text = blockData.BlockName;
        }
    }

    /// <summary>
    /// 드래그 종료 시 호출
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        // 레이캐스트로 드롭 대상 찾기
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        TimelineDropZone dropZone = null;

        foreach (var result in results)
        {
            dropZone = result.gameObject.GetComponent<TimelineDropZone>();
            if (dropZone != null)
                break;
        }

        if (dropZone != null)
        {
            // 드롭존에 배치 시도
            int tickIndex = dropZone.tickIndex;

            // ⭐ 디버깅 로그
            Debug.Log($"[Draggable_Block] OnEndDrag - 배치 시도 블록: {runtimeBlock?.BaseData?.BlockName ?? "NULL"}, 길이: {runtimeBlock?.BaseData?.BlockLength ?? 0}");

            if (TimelineManager.Instance != null)
            {
                // ⭐ HandBlock_UI가 있으면 새로운 메서드 사용
                if (_sourceHandBlock != null)
                {
                    RuntimeBlock originalBlock = _sourceHandBlock.GetOriginalBlock();

                    // ⭐ 추가 디버깅
                    Debug.Log($"[Draggable_Block] 원본 블록: {originalBlock.BaseData.BlockName}, 배치 블록: {runtimeBlock.BaseData.BlockName}");

                    bool success = TimelineManager.Instance.TryPlaceBlockWithUI(
                        runtimeBlock,
                        originalBlock,
                        _sourceHandBlock,
                        tickIndex
                    );

                    //if (success)
                    //{
                    //    if (SoundManager.Instance != null)
                    //        SoundManager.Instance.Play(SoundID.SFX_Card_Place);
                    //}
                }
                else
                {
                    // 기존 방식 (호환성)
                    bool success = TimelineManager.Instance.TryPlaceBlock(runtimeBlock, tickIndex);

                    //if (success)
                    //{
                    //    if (SoundManager.Instance != null)
                    //        SoundManager.Instance.Play(SoundID.SFX_Card_Place);
                    //}
                }
            }
        }
    }
}
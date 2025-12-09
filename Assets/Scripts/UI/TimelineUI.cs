using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimelineUI : MonoBehaviour
{
    [Header("타임라인 슬롯 프리팹")]
    public GameObject enemySlotPrefab;      // 적 전조 슬롯
    public GameObject cursorSlotPrefab;     // 실행 커서 슬롯
    public GameObject playerSlotPrefab;     // 플레이어 배치 슬롯

    [Header("타임라인 패널 (3줄)")]
    public Transform enemyTimelinePanel;    // 위쪽: 적 전조
    public Transform cursorTimelinePanel;   // 중간: 실행 커서
    public Transform playerTimelinePanel;   // 아래쪽: 플레이어 배치

    [Header("툴팁 UI")]
    public GameObject tooltipPanel;          // 툴팁 패널
    public TextMeshProUGUI tooltipText;      // 툴팁 텍스트
    [SerializeField]
    private Vector3 tooltipOffset = new Vector3(10f, 10f, 0f);

    [Header("색상 설정")]
    public Color normalColor = Color.white;
    public Color attackColor = new Color(1f, 0.3f, 0.3f);
    public Color parryingColor = new Color(1f, 1f, 0.3f);
    public Color cursorColor = Color.yellow;
    public Color occupiedColor = new Color(0.3f, 0.3f, 0.3f);

    [Header("연결")]
    public Transform handContainer;
    public GameObject cardPrefab;

    [Header("아이콘 스프라이트")]
    [SerializeField] private Sprite parryIcon;   // 패링 아이콘 스프라이트

    // 슬롯 저장 (틱 1~18)
    private List<GameObject> enemySlots = new List<GameObject>();
    private List<GameObject> cursorSlots = new List<GameObject>();
    private List<GameObject> playerSlots = new List<GameObject>();

    // events
    public event System.Action<List<int>> OnRequestHighlight;
    public event System.Action OnRequestClearHighlight;

    // 현재 적 시퀀스
    private EnemyPattern _currentPattern;

    void Start()
    {
        CreateTimelineSlots();

        if (TimelineManager.Instance != null)
        {
            TimelineManager.Instance.OnTimelineChanged += UpdatePlayerTimeline;
            TimelineManager.Instance.OnEnemyPatternChanged += DisplayEnemySequence;
        }
    }

    /// <summary>
    /// 타임라인 슬롯 생성 (3줄 - 8틱)
    /// </summary>
    private void CreateTimelineSlots()
    {
        // 1줄: 적 전조 (위쪽)
        for (int tick = 1; tick <= 8; tick++)
        {
            GameObject slot = Instantiate(enemySlotPrefab, enemyTimelinePanel);
            slot.name = $"EnemySlot_{tick}";

            // 틱 번호 표시
            TextMeshProUGUI text = slot.GetComponentInChildren<TextMeshProUGUI>();

            // 마우스 오버 이벤트 추가
            EnemySlotHover hoverHandler = slot.AddComponent<EnemySlotHover>();
            hoverHandler.tick = tick;
            hoverHandler.timelineUI = this;

            enemySlots.Add(slot);
        }

        // 2줄: 실행 커서 (중간)
        for (int tick = 1; tick <= 8; tick++)
        {
            GameObject slot = Instantiate(cursorSlotPrefab, cursorTimelinePanel);
            slot.name = $"CursorSlot_{tick}";

            TextMeshProUGUI text = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = tick.ToString();
            }
            TimelineTickHoverHandler hoverHandler = slot.AddComponent<TimelineTickHoverHandler>();
            hoverHandler.tickIndex = tick;
            cursorSlots.Add(slot);
        }

        // 3줄: 플레이어 배치 (아래쪽)
        for (int tick = 1; tick <= 8; tick++)
        {
            GameObject slot = Instantiate(playerSlotPrefab, playerTimelinePanel);
            slot.name = $"PlayerSlot_{tick}";

            // 드롭 이벤트 핸들러 추가
            TimelineDropZone dropZone = slot.AddComponent<TimelineDropZone>();
            dropZone.tickIndex = tick;

            TextMeshProUGUI text = slot.GetComponentInChildren<TextMeshProUGUI>();
            // 틱 번호 표시
            if (text != null)
            {
                //text.text = tick.ToString();
            }

            playerSlots.Add(slot);
        }
    }

    

    /// <summary>
    /// 적 시퀀스 표시 (위쪽 줄)
    /// </summary>
    public void DisplayEnemySequence(EnemyPattern sequence)
    {
        _currentPattern = sequence;

        // 모든 슬롯 초기화
        foreach (GameObject slot in enemySlots)
        {

            UnityEngine.UI.Image image = slot.GetComponent<Image>();
            TextMeshProUGUI text = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (image != null)
            {
                image.color = normalColor;
            }
            if (text != null)
            {
                text.text = " "; // 데미지가 없으면 공백 표시
            }
        }

        if (sequence == null) return;

        // 공격이 있는 틱을 빨간색으로 표시
        foreach (EnemyAttack attack in sequence.attacks)
        {
            if (attack.tick >= 1 && attack.tick <= enemySlots.Count)
            {
                Image image = enemySlots[attack.tick - 1].GetComponent<Image>();
                TextMeshProUGUI text = enemySlots[attack.tick - 1].GetComponentInChildren<TextMeshProUGUI>();

                if (image != null)
                {
                    image.color = attackColor;
                }
                if (text != null)
                {
                    // 데미지 수치를 문자열로 표시
                    text.text = attack.damage.ToString();
                    text.color = Color.white;
                }
            }
            else
            {
                Debug.LogWarning($"적 공격 틱 {attack.tick}이 범위를 벗어났습니다 (1~{enemySlots.Count})");
            }
        }
        // 패링이 있는 틱을 노란색으로 표시
        foreach (EnemyParrying parrying in sequence.parryings)
        {
            if (parrying.tick >= 1 && parrying.tick <= enemySlots.Count)
            {
                Image image = enemySlots[parrying.tick - 1].GetComponent<Image>();
                TextMeshProUGUI text = enemySlots[parrying.tick - 1].GetComponentInChildren<TextMeshProUGUI>();

                if (image != null)
                {
                    image.color = parryingColor;
                }
                if (text != null)
                {
                    text.text = "P";
                    text.color = Color.white;
                }
            }
            else
            {
                Debug.LogWarning($"적 공격 틱 {parrying.tick}이 범위를 벗어났습니다 (1~{enemySlots.Count})");
            }
        }
    }


    /// <summary>
    /// 적 공격 정보 툴팁 표시
    /// </summary>
    public void ShowEnemyAttackTooltip(int tick, Vector3 position)
    {
        if (_currentPattern == null || tooltipPanel == null) return;

        EnemyAttack attack = _currentPattern.GetAttackAt(tick);
        EnemyParrying parrying = _currentPattern.GetParryingAt(tick);

        if (attack == null && parrying == null)
        {
            HideTooltip();
            return;
        }

        // 툴팁 텍스트 설정
        if (tooltipText != null)
        {
            if (attack != null)
            {
                string sectors = string.Join(", ", attack.targetSectors);
                tooltipText.text = $"틱 {tick}\n섹터: {sectors}\n데미지: {attack.damage}";
            }
            if (parrying != null)
            {
                tooltipText.text = $"틱 {tick}\n공격 튕겨내기";
            }
        }

        // 툴팁 위치 설정
        tooltipPanel.transform.position = position + tooltipOffset;
        tooltipPanel.SetActive(true);

        // 맵에 공격 섹터 표시
        if (attack != null && attack.targetSectors != null)
        {
            OnRequestHighlight?.Invoke(attack.targetSectors);
        }
    }

    /// <summary>
    /// 툴팁 숨기기
    /// </summary>
    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }

        OnRequestClearHighlight?.Invoke();
    }

    /// <summary>
    /// 플레이어 카드 정보 툴팁 표시
    /// </summary>
    public void ShowPlayerCardTooltip(int tick, PlacedBlock placedBlock, Vector3 position, bool is_prev = false)
    {
        if (tooltipPanel == null || placedBlock == null) return;

        // 이 틱에서의 효과 확인
        int cardTickIndex = tick - placedBlock.startTick;
        // 놓인 블럭 정보
        BlockData blockData = placedBlock.GetBlockData();

        ActionType effect = blockData.GetEffectAt(cardTickIndex);
        MoveDirection moveDir = blockData.moveDirections[cardTickIndex];
        string effectText = "";
        switch (effect)
        {
            case ActionType.None:
                effectText = "행동 없음";
                break;
            case ActionType.Attack:
                effectText = $"데미지: {blockData.attackDamage}";
                break;
            case ActionType.Move:
                if (moveDir == MoveDirection.Right)
                    effectText = $"이동: 시계방향";
                else if (moveDir == MoveDirection.Left)
                    effectText = $"이동: 반시계방향";
                break;
        }

        // 툴팁 텍스트 설정
        if (tooltipText != null)
        {
            if (is_prev)
            {
                tooltipText.text = $"과거의 {blockData.blockName} - 틱 {tick} ({cardTickIndex + 1}/{blockData.blockLength})\n{effectText}\n";
            }
            else
            {
                tooltipText.text = $"{blockData.blockName} - 틱 {tick} ({cardTickIndex + 1}/{blockData.blockLength})\n{effectText}\n\n우클릭: 제거";
            }

        }

        // 툴팁 위치 설정
        tooltipPanel.transform.position = position + tooltipOffset;
        tooltipPanel.SetActive(true);
    }

    /// <summary>
    /// 실행 커서 업데이트 (중간 줄)
    /// </summary>
    public void UpdateCursor(int currentTick)
    {
        // 모든 커서 슬롯 초기화
        foreach (GameObject slot in cursorSlots)
        {
            Image image = slot.GetComponent<Image>();
            if (image != null)
            {
                image.color = normalColor;
            }
        }

        // 현재 틱 하이라이트
        if (currentTick >= 1 && currentTick <= 8)
        {
            Image image = cursorSlots[currentTick - 1].GetComponent<Image>();
            if (image != null)
            {
                image.color = cursorColor;
            }
        }
    }

    /// <summary>
    /// 플레이어 카드 배치 영역 업데이트 (아래쪽 줄)
    /// </summary>
    public void UpdatePlayerTimeline(IReadOnlyList<PlacedBlock> placedBlocks, IReadOnlyList<PlacedBlock> prevBlocks)
    {
        // 모든 슬롯 초기화
        foreach (GameObject slot in playerSlots)
        {
            Image image = slot.GetComponent<Image>();
            TextMeshProUGUI cellText = slot.GetComponentInChildren<TextMeshProUGUI>();
            Image iconImage = slot.transform.Find("Icon")?.GetComponent<Image>();

            // 기본 색상 초기화
            if (image != null)
            {
                image.color = normalColor;
            }

            // 텍스트 초기화
            if (cellText != null)
            {
                cellText.text = "";
            }

            // 아이콘 초기화
            if (iconImage != null)
            {
                iconImage.enabled = false;
                iconImage.sprite = null;
            }

            // 기존 호버 핸들러 제거
            PlayerSlotHover hoverHandler = slot.GetComponent<PlayerSlotHover>();
            if (hoverHandler != null)
            {
                hoverHandler.placedBlock = null;
            }
        }

        // 잔상 카드 표시
        foreach (PlacedBlock placed in prevBlocks)
        {
            Debug.Log("잔상 카드 표시 중...");

            // 놓인 블럭 정보
            BlockData blockData = placed.GetBlockData();

            for (int i = 0; i < blockData.blockLength; i++)
            {
                int tickIndex = placed.startTick + i - 1;
                if (tickIndex >= 0 && tickIndex < playerSlots.Count)
                {
                    GameObject slot = playerSlots[tickIndex];
                    Image image = slot.GetComponent<Image>();
                    TextMeshProUGUI cellText = slot.GetComponentInChildren<TextMeshProUGUI>();
                    Image iconImage = slot.transform.Find("Icon")?.GetComponent<Image>();

                    // 기본값
                    Color color = occupiedColor;
                    string text = "";
                    bool showIcon = false;
                    Sprite iconSprite = null;

                    // 이 틱의 효과에 따라 색상 / 텍스트 / 아이콘 설정
                    ActionType effect = blockData.actionTypes[i];

                    switch (effect)
                    {
                        case ActionType.None:
                            color = new Color(occupiedColor.r, occupiedColor.g, occupiedColor.b, 0.25f); // 회색
                            text = "-";
                            break;

                        case ActionType.Attack:
                            color = new Color(1f, 0.5f, 0.5f, 0.25f); // 연한 빨강
                            text = "▲";
                            break;

                        case ActionType.Move:
                            color = new Color(0.5f, 0.8f, 1f, 0.25f); // 연한 파랑
                            MoveDirection dir = MoveDirection.None;
                            if (placed.linkedRuntimeBlock != null && placed.linkedRuntimeBlock.CurrentMoveDirections != null)
                            {
                                dir = placed.linkedRuntimeBlock.CurrentMoveDirections[i];
                            }
                            else
                            {
                                dir = blockData.moveDirections[i];
                            }
                            if (dir == MoveDirection.Left)
                                text = "↺";
                            else if (dir == MoveDirection.Right)
                                text = "↻";
                            break;
                    }

                    // 적용
                    if (image != null)
                        image.color = color;

                    if (cellText != null)
                        cellText.text = text;

                    if (iconImage != null)
                    {
                        iconImage.enabled = showIcon;
                        iconImage.sprite = iconSprite;
                    }

                    // 호버 핸들러 추가/업데이트
                    PlayerSlotHover hoverHandler = slot.GetComponent<PlayerSlotHover>();
                    if (hoverHandler == null)
                    {
                        hoverHandler = slot.AddComponent<PlayerSlotHover>();
                        hoverHandler.timelineUI = this;
                    }

                    //과거 잔상 표시용
                    hoverHandler.Is_prev = true;
                    hoverHandler.tick = tickIndex + 1;
                    hoverHandler.placedBlock = placed;
                }
            }
        }

        // 배치된 카드 표시
        foreach (PlacedBlock placed in placedBlocks)
        {
            // 놓인 블럭 정보
            BlockData blockData = placed.GetBlockData();

            for (int i = 0; i < blockData.blockLength; i++)
            {
                int tickIndex = placed.startTick + i - 1;
                if (tickIndex >= 0 && tickIndex < playerSlots.Count)
                {
                    GameObject slot = playerSlots[tickIndex];
                    Image image = slot.GetComponent<Image>();
                    TextMeshProUGUI cellText = slot.GetComponentInChildren<TextMeshProUGUI>();
                    Image iconImage = slot.transform.Find("Icon")?.GetComponent<Image>();

                    // 기본값
                    Color color = occupiedColor;
                    string text = "";
                    bool showIcon = false;
                    Sprite iconSprite = null;

                    // 이 틱의 효과에 따라 색상 / 텍스트 / 아이콘 설정
                    ActionType effect = blockData.actionTypes[i];

                    switch (effect)
                    {
                        case ActionType.None:
                            color = occupiedColor;          // 회색
                            text = "-";
                            break;

                        case ActionType.Attack:
                            color = new Color(1f, 0.3f, 0.3f); // 연한 빨강
                            text = "▲";
                            break;

                        case ActionType.Move:
                            color = new Color(0.3f, 0.7f, 1f); // 연한 파랑
                            MoveDirection dir = MoveDirection.None;
                            if (placed.linkedRuntimeBlock != null && placed.linkedRuntimeBlock.CurrentMoveDirections != null)
                            {
                                dir = placed.linkedRuntimeBlock.CurrentMoveDirections[i];
                            }
                            else
                            {
                                dir = blockData.moveDirections[i];
                            }
                            if (dir == MoveDirection.Left)
                                text = "↺";
                            else if (dir == MoveDirection.Right)
                                text = "↻";
                            break;

                    }

                    // 적용
                    if (image != null)
                        image.color = color;

                    if (cellText != null)
                        cellText.text = text;

                    if (iconImage != null)
                    {
                        iconImage.enabled = showIcon;
                        iconImage.sprite = iconSprite;
                    }

                    // 호버 핸들러 추가/업데이트
                    PlayerSlotHover hoverHandler = slot.GetComponent<PlayerSlotHover>();
                    if (hoverHandler == null)
                    {
                        hoverHandler = slot.AddComponent<PlayerSlotHover>();
                        hoverHandler.timelineUI = this;
                    }

                    if (hoverHandler != null)
                    {
                        hoverHandler.Is_prev = false;
                    }

                    hoverHandler.tick = tickIndex + 1;
                    hoverHandler.placedBlock = placed;
                }
            }
        }
    }

    /// <summary>
    /// 특정 틱 범위가 비어있는지 확인
    /// </summary>
    public bool IsRangeAvailable(int startTick, int length)
    {
        if (startTick < 1 || startTick + length - 1 > TimelineManager.Instance.TotalTicks)
            return false;

        if (TimelineManager.Instance != null)
        {
            return TimelineManager.Instance.CanPlaceAt(startTick, length);
        }

        return false;
    }
}

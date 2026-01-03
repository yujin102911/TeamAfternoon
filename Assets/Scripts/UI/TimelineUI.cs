using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class TimelineUI : MonoBehaviour
{
    [Header("새로운 레이아웃 전용")]
    public bool New_Layout = false;
    public GameObject New_Layout_description; // 설명 슬롯

    [Header("타임라인 슬롯 프리팹")]
    public GameObject enemySlotPrefab;      // 적 전조 슬롯
    public GameObject cursorSlotPrefab;     // 실행 커서 슬롯
    public GameObject playerSlotPrefab;     // 플레이어 배치 슬롯
    public GameObject descriptionSlotPrefab; // 설명 슬롯

    [Header("타임라인 슬롯 강조색")]
    [SerializeField] private Color _highlightedColor;
    [SerializeField] private Color _defaultColor;

    [Header("타임라인 슬롯 스프라이트")]
    public Sprite Set_SlotSprite;         // 장착 시 슬롯 스프라이트
    public Sprite Empty_SlotSprite;       // 빈 슬롯 스프라이트

    [Header("타임라인 패널 (3줄)")]
    public Transform enemyTimelinePanel;    // 위쪽: 적 전조
    public Transform cursorTimelinePanel;   // 중간: 실행 커서
    public Transform playerTimelinePanel;   // 아래쪽: 플레이어 배치
    public Transform descriptionPanel;

    //[Header("패턴 대사 UI")]
    //public TextMeshProUGUI sentenceText;

    [Header("플레이어 슬롯 설정")]
    public float playerSlotSpacing = 5f;    // 플레이어 슬롯 간격
    public float playerSlotWidth = 60f;     // 플레이어 슬롯 너비

    [Header("툴팁 UI")]
    public GameObject tooltipPanel;          // 툴팁 패널
    public TextMeshProUGUI tooltipTitleText;      // 툴팁 제목
    public TextMeshProUGUI tooltipDetailText;      // 툴팁 설명

    [Header("플레이어 툴팁 UI")]
    public GameObject Player_tooltipPanel;          // 툴팁 패널
    public TextMeshProUGUI Player_tooltipTitleText;      // 툴팁 제목
    public TextMeshProUGUI Player_tooltipDetailText;      // 툴팁 설명

    [SerializeField]
    private Vector3 tooltipOffset = new Vector3(10f, 10f, 0f);

    [Header("색상 설정")]
    public Color normalColor = Color.white;
    public Color cursorColor = Color.yellow;
    [Header("적 색상")]
    public Color attackColor = new Color(1f, 0.3f, 0.3f);
    public Color parryingColor = new Color(1f, 1f, 0.3f);
    
    [Header("플레이어 색상 설정")]
    [Header("none 색상")]
    public Color occupiedColor = new Color(0.3f, 0.3f, 0.3f);
    [TabGroup("Attack")]
    public Color Player_attackColor;
    [TabGroup("Attack")]
    public Sprite Sword_icon;
    [TabGroup("Move")]
    public Color Player_moveColor;
    [TabGroup("Move")]
    public Sprite Front_icon;
    [TabGroup("Move")]
    public Sprite Back_icon;
    [TabGroup("Move")]
    public Sprite Left_icon;
    [TabGroup("Move")]
    public Sprite Right_icon;
    

    [TabGroup("Cure")]
    [SerializeField]
    private Color[] _cureColors;
    [TabGroup("Cure")]
    public Sprite[] _cureIcons;

    private int _currentSliderTick = -1;

    // 슬롯 저장 (틱 1~18)
    private List<GameObject> enemySlots = new List<GameObject>();
    private List<GameObject> cursorSlots = new List<GameObject>();
    private List<GameObject> playerSlots = new List<GameObject>();
    private List<GameObject> descriptionSlots = new List<GameObject>();

    // events
    public event Action<List<int>> OnRequestHighlight;
    public event Action OnRequestClearHighlight;
    public event Action<int, ActionType> OnRequestPreviewPlayer;
    public event Action OnRequestHidePreview;

    // 현재 적 시퀀스
    private EnemyPattern _currentPattern;

    void Start()
    {
        CreateTimelineSlots();

        SetActive_Slots(false);

        if (TimelineManager.Instance != null)
        {
            TimelineManager.Instance.OnTimelineChanged += UpdatePlayerTimeline;
            TimelineManager.Instance.OnTimelineChanged += (cur, prev) => UpdateDangerIndicators();
            TimelineManager.Instance.OnEnemyPatternChanged += DisplayEnemySequence;
            TimelineManager.Instance.OnEnemyPatternChanged += (pattern) => UpdateDangerIndicators();
            TimelineManager.Instance.OnCurrentTickChanged += UpdateCursor;
        }
    }

    private void OnDestroy()
    {
        if (TimelineManager.Instance != null)
        {
            TimelineManager.Instance.OnTimelineChanged -= UpdatePlayerTimeline;
            TimelineManager.Instance.OnTimelineChanged -= (cur, prev) => UpdateDangerIndicators();
            TimelineManager.Instance.OnEnemyPatternChanged -= DisplayEnemySequence;
            TimelineManager.Instance.OnEnemyPatternChanged -= (pattern) => UpdateDangerIndicators();
            TimelineManager.Instance.OnCurrentTickChanged -= UpdateCursor;
        }
    }

    // 슬롯 온오프
    public void SetActive_Slots(bool is_active)
    {
        foreach (var enemy in enemySlots)
        {
            enemy.SetActive(is_active);
        }

        foreach (var player in playerSlots)
        {
            player.SetActive(is_active);
        }
    }

    // 슬롯 색 변경
    public void HighlightSlots(int startIndex, int endIndex, bool is_enter)
    {
        //ResetSlotColor();

        // 인덱스 보정 (역순 / 범위 초과 방지)
        startIndex = Mathf.Clamp(startIndex, 1, playerSlots.Count);
        endIndex = Mathf.Clamp(endIndex, 1, playerSlots.Count);

        if (startIndex > endIndex)
            (startIndex, endIndex) = (endIndex, startIndex);

        for (int i = startIndex - 1; i <= endIndex - 1; i++)
        {
            Image img = playerSlots[i].GetComponent<Image>();
            if (img != null)
            {
                if (is_enter)
                {
                    img.color = _highlightedColor;
                }
                else
                {
                    img.color = _defaultColor;
                }
                
            }
        }
    }

    private void ResetSlotColor()
    {
        foreach (var slot in playerSlots)
        {
            Image img = slot.GetComponent<Image>();
            img.color = _defaultColor;
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
            //TextMeshProUGUI text = slot.GetComponentInChildren<TextMeshProUGUI>();

            // 마우스 오버 이벤트 추가
            EnemySlotHover hoverHandler = slot.AddComponent<EnemySlotHover>();
            hoverHandler.tick = tick;
            hoverHandler.timelineUI = this;

            enemySlots.Add(slot);
        }

        // 2줄: 실행 커서 (중간)
        for (int tick = 1; tick <= 16; tick++)
        {
            GameObject slot = Instantiate(cursorSlotPrefab, cursorTimelinePanel);
            slot.name = $"CursorSlot_{tick}";

            TimelineTickHoverHandler hoverHandler = slot.GetComponent<TimelineTickHoverHandler>();
            hoverHandler.tickIndex = tick;
            cursorSlots.Add(slot);
            hoverHandler.Hide();
        }

        // 3줄: 플레이어 배치 (아래쪽)
        for (int tick = 1; tick <= 8; tick++)
        {
            GameObject slot = Instantiate(playerSlotPrefab, playerTimelinePanel);
            slot.name = $"PlayerSlot_{tick}";


            TimelineDropZone dropZone = null;

            // TODO: New_Layout 확정되면 나중에 지우기
            // 드롭 이벤트 핸들러 추가
            if (New_Layout)
            {
                dropZone = slot.transform.Find("Image")?.AddComponent<TimelineDropZone>();
            }
            else
            {
                dropZone = slot.AddComponent<TimelineDropZone>();
            }

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
            slot.GetComponent<Enemy_slot>().Hide();

            //Image image = slot.GetComponent<Image>();
            //TextMeshProUGUI text = slot.GetComponentInChildren<TextMeshProUGUI>();
            //if (image != null)
            //{
            //    image.color = normalColor;
            //}
            //if (text != null)
            //{
            //    text.text = " "; // 데미지가 없으면 공백 표시
            //}
        }

        if (sequence == null) return;

        // 공격이 있는 틱을 빨간색으로 표시
        foreach (EnemyAttack attack in sequence.attacks)
        {
            if (attack.tick >= 1 && attack.tick <= enemySlots.Count)
            {
                //Image image = enemySlots[attack.tick - 1].GetComponent<Image>();
                //TextMeshProUGUI text = enemySlots[attack.tick - 1].GetComponentInChildren<TextMeshProUGUI>();

                //if (image != null)
                //{
                //    image.color = attackColor;
                //}
                //if (text != null)
                //{
                //    // 데미지 수치를 문자열로 표시
                //    text.text = attack.damage.ToString();
                //    //text.color = Color.white;
                //}

                enemySlots[attack.tick - 1].GetComponent<Enemy_slot>().Show(attackColor, attack.damage.ToString(), Special_Pattern.None, attack.targetSectors);
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
                //Image image = enemySlots[parrying.tick - 1].GetComponent<Image>();
                //TextMeshProUGUI text = enemySlots[parrying.tick - 1].GetComponentInChildren<TextMeshProUGUI>();

                //if (image != null)
                //{
                //    image.color = parryingColor;
                //}
                //if (text != null)
                //{
                //    text.text = "P";
                //    //text.color = Color.white;
                //}

                enemySlots[parrying.tick - 1].GetComponent<Enemy_slot>().Show(parryingColor, "P");
            }
            else
            {
                Debug.LogWarning($"적 공격 틱 {parrying.tick}이 범위를 벗어났습니다 (1~{enemySlots.Count})");
            }
        }

        foreach (EnemyStone stone in sequence.stones)
        {
            if (stone.tick >= 1 && stone.tick <= enemySlots.Count)
            {

                enemySlots[stone.tick - 1].GetComponent<Enemy_slot>().Show(attackColor, "", Special_Pattern.Stone, null);
            }
            else
            {
                Debug.LogWarning($"적 돌던지기 틱 {stone.tick}이 범위를 벗어났습니다 (1~{enemySlots.Count})");
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

        if(tooltipTitleText != null)
        {
            tooltipTitleText.text = $"슬라임의 물기";
        }

        // 툴팁 텍스트 설정
        if (tooltipDetailText != null)
        {
            if (attack != null)
            {
                string sectors = string.Join(", ", attack.targetSectors);
                tooltipDetailText.text = $"<color=#D94036>♥</color> -{attack.damage}";
            }
            if (parrying != null)
            {
                tooltipDetailText.text = $"공격 튕겨내기";
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

    public void HidePlayerTooltip()
    {
        if (Player_tooltipPanel != null)
        {
            Player_tooltipPanel.SetActive(false);
        }

        OnRequestClearHighlight?.Invoke();
    }

    /// <summary>
    /// 플레이어 카드 정보 툴팁 표시
    /// </summary>
    public void ShowPlayerCardTooltip(int tick, PlacedBlock placedBlock, Vector3 position, bool is_prev = false)
    {
        if (Player_tooltipPanel == null || placedBlock == null) return;

        // 이 틱에서의 효과 확인
        int cardTickIndex = tick - placedBlock.startTick;
        // 놓인 블럭 정보 (기본 데이터)
        BlockData blockData = placedBlock.GetBlockData();

        ActionType effect = blockData.GetEffectAt(cardTickIndex);
        MoveDirection moveDir = MoveDirection.None;

        if (placedBlock.linkedRuntimeBlock != null && placedBlock.linkedRuntimeBlock.CurrentMoveDirections != null)
        {
            moveDir = placedBlock.linkedRuntimeBlock.CurrentMoveDirections[cardTickIndex];
        }
        else
        {
            moveDir = blockData.moveDirections[cardTickIndex];
        }


        string titleText = "";
        string effectText = "";
        switch (effect)
        {
            case ActionType.None:
                titleText = "행동 없음";
                //effectText = "움직이지 않고 가만히 있는다";
                break;
            case ActionType.Cure:
                titleText = "정화";
                effectText = $"정화 수치: {blockData.CalCulate_CurePower(cardTickIndex)}";
                break;
            case ActionType.Attack:
                titleText = "근거리 공격";
                effectText = $"적에게 가장 가까운 열에서만 <b>{blockData.attackDamage}피해</b>를 줍니다.";
                break;
            case ActionType.Move:
                titleText = "이동";
                if (moveDir == MoveDirection.Right)
                    effectText = $"이동 방향: 아래\n좌클릭: 방향 변경";
                else if (moveDir == MoveDirection.Left)
                    effectText = $"이동 방향: 위\n좌클릭: 방향 변경";
                else if (moveDir == MoveDirection.Front)
                    effectText = $"이동 방향: 앞\n좌클릭: 방향 변경";
                else if (moveDir == MoveDirection.Back)
                    effectText = $"이동 방향: 뒤\n좌클릭: 방향 변경";
                    break;

            case ActionType.Bow_end:
            case ActionType.Bow_middle:
                titleText = "활시위 당기는중...";
                break;
            
            case ActionType.Bow_single:
            case ActionType.Bow_start:
                titleText = "원거리 공격";
                effectText = $"거리에 관계없이 적에게 <b>{blockData.attackDamage}피해</b>를 줍니다.";
                break;
        }

        // 툴팁 텍스트 설정
        if (Player_tooltipPanel != null)
        {
            if (is_prev)
            {
                Player_tooltipTitleText.text = $"과거의 {blockData.blockName} ({cardTickIndex + 1}/{blockData.blockLength})";
                Player_tooltipTitleText.text = "과거의 " + titleText;
                Player_tooltipDetailText.text = $"{effectText}\n";
            }
            else
            {
                Player_tooltipTitleText.text = $"{blockData.blockName} ({cardTickIndex + 1}/{blockData.blockLength})";
                Player_tooltipTitleText.text = titleText;
                Player_tooltipDetailText.text = $"{effectText}\n<b>우클릭:</b> 영상 제거";
            }

            

        }

        // 툴팁 위치 설정
        Player_tooltipPanel.transform.position = position + tooltipOffset;
        Player_tooltipPanel.SetActive(true);
    }

    /// <summary>
    /// 실행 커서 업데이트 (중간 줄)
    /// </summary>
    public void UpdateCursor(int currentTick)
    {
        // 모든 커서 슬롯 초기화
        foreach (GameObject slot in cursorSlots)
        {
            TimelineTickHoverHandler hoverHandler = slot.GetComponent<TimelineTickHoverHandler>();
            hoverHandler.Hide();

            //Image image = slot.GetComponent<Image>();
            //if (image != null)
            //{
            //    image.color = normalColor;
            //}
        }

        // 현재 틱 하이라이트
        if (currentTick >= 1 && currentTick <= TimelineManager.Instance.TotalTicks * 2)
        {
            TimelineTickHoverHandler hoverHandler = cursorSlots[currentTick - 1].GetComponent<TimelineTickHoverHandler>();
            hoverHandler.Show();

            //Image image = cursorSlots[currentTick - 1].GetComponent<Image>();
            //if (image != null)
            //{
            //    image.color = cursorColor;
            //}
        }
    }

    public void OnCursorEnter(int tick)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsExecutingRound)
        {
            return;
        }

        if (TimelineManager.Instance != null)
        {
            int new_tick = (tick - 1) / 2 + 1;
            int predictedSector = TimelineManager.Instance.SimulatePlayerPosition(new_tick);
            ActionType action = TimelineManager.Instance.GetActionAtTick(new_tick);

            ActionType previewAction = ActionType.None;

            if (tick % 2 == 1)
            {
                previewAction = action;
            }
            else
            {
                if (action == ActionType.Bow_middle || action == ActionType.Bow_end)
                {
                    previewAction = ActionType.Bow_middle;
                }
                else
                {
                    previewAction = ActionType.None;
                }

            }

            Debug.Log($"[TimelineUI] previewAction: {previewAction}");

            OnRequestPreviewPlayer?.Invoke(predictedSector, previewAction);

            if (tick % 2 == 0) // 적 공격 범위 표시
            {
                List<int> attackSectors = TimelineManager.Instance.GetEnemyAttackSectors(new_tick);
                if (attackSectors != null && attackSectors.Count > 0)
                    OnRequestHighlight?.Invoke(attackSectors);
            }

        }
    }

    public void OnCursorExit()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsExecutingRound)
        {
            return;
        }
        OnRequestHidePreview?.Invoke();
        OnRequestClearHighlight?.Invoke();
    }

    // 슬라이더에서 호출
    public void Show_Preview(int tick)
    {
        _currentSliderTick = tick;

        if (TimelineManager.Instance != null)
        {
            int new_tick = (tick - 1) / 2 + 1;
            int predictedSector = TimelineManager.Instance.SimulatePlayerPosition(new_tick);
            ActionType action = TimelineManager.Instance.GetActionAtTick(new_tick);

            ActionType previewAction = ActionType.None;

            if (tick % 2 == 1)
            {
                previewAction = action;
            }
            else
            {
                if (action == ActionType.Bow_middle || action == ActionType.Bow_end)
                {
                    previewAction = ActionType.Bow_middle;
                }
                else
                {
                    previewAction = ActionType.None;
                }

            }

            OnRequestPreviewPlayer?.Invoke(predictedSector, previewAction);

            if (tick % 2 == 0) // 적 공격 범위 표시
            {
                List<int> attackSectors = TimelineManager.Instance.GetEnemyAttackSectors(new_tick);
                if (attackSectors != null && attackSectors.Count > 0)
                    OnRequestHighlight?.Invoke(attackSectors);
            }
        }
    }

    // 슬라이더에서 호출
    public void Hide_Preview()
    {
        OnRequestHidePreview?.Invoke();
        OnRequestClearHighlight?.Invoke();
    }

    public void OnPatternChanged(EnemyPattern pattern)
    {
        //if (sentenceText == null) return;
        //if (pattern != null)
        //    sentenceText.text = pattern.Sentence;
        //else
        //{
        //    sentenceText.text = "";

        //}
    }

    /// <summary>
    /// 플레이어 카드 배치 영역 업데이트 (아래쪽 줄)
    /// </summary>
    public void UpdatePlayerTimeline(IReadOnlyList<PlacedBlock> placedBlocks, IReadOnlyList<PlacedBlock> prevBlocks)
    {
        // 설명 슬롯 제거
        foreach (var go in descriptionSlots)
        {
            if (go != null)
                Destroy(go);
        }

        descriptionSlots.Clear();


        // 모든 슬롯 초기화
        foreach (GameObject slot in playerSlots)
        {
            ITimelineSlotView slotView = slot.GetComponent<ITimelineSlotView>();

            if (slotView == null)
            {
                Debug.LogError($"ITimelineSlotView missing on {slot.name}");
                continue;
            }

            slotView.Clear();
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

                    if (tickIndex < 0 || tickIndex >= playerSlots.Count)
                        continue;

                    GameObject slotGO = playerSlots[tickIndex];

                    ITimelineSlotView slotView = slotGO.GetComponent<ITimelineSlotView>();

                    if (slotView == null)
                    {
                        Debug.LogError($"ITimelineSlotView missing on {slotGO.name}");
                        continue;
                    }

                    ActionType action = blockData.GetEffectAt(i);

                    MoveDirection dir = MoveDirection.None;
                    if (placed.linkedRuntimeBlock != null)
                        dir = placed.linkedRuntimeBlock.CurrentMoveDirections[i];

                    int damage = action == ActionType.Cure
                        ? blockData.CalCulate_CurePower(i)
                        : blockData.attackDamage;

                    slotView.SetAction(
                        action,
                        dir,
                        damage,
                        isPreview: false,
                        isPrev: true
                    );

                    // 호버 핸들러 추가/업데이트
                    slotView.SetHoverData(
                        this,
                        tickIndex + 1,
                        placed,
                        isPrev: true
                    );
                }
            }
        }

        // 배치된 카드 표시
        foreach (PlacedBlock placed in placedBlocks)
        {
            // 놓인 블럭 정보
            BlockData blockData = placed.GetBlockData();

            // 레이아웃에 따른 프리펩 선정
            GameObject prefab = New_Layout ? New_Layout_description : descriptionSlotPrefab;

            GameObject descriptionSlot = Instantiate(prefab, descriptionPanel);

            descriptionSlot.name = $"{placed.startTick}. DescriptionSlot";
            descriptionSlot.GetComponent<Block_descript>().SetUp(placed.startTick, playerSlotWidth, playerSlotSpacing, placed);
            descriptionSlots.Add(descriptionSlot);

            for (int i = 0; i < blockData.blockLength; i++)
            {
                int tickIndex = placed.startTick + i - 1;

                if (tickIndex >= 0 && tickIndex < playerSlots.Count)
                {

                    if (tickIndex < 0 || tickIndex >= playerSlots.Count)
                        continue;

                    GameObject slotGO = playerSlots[tickIndex];

                    ITimelineSlotView slotView = slotGO.GetComponent<ITimelineSlotView>();

                    if (slotView == null)
                    {
                        Debug.LogError($"ITimelineSlotView missing on {slotGO.name}");
                        continue;
                    }

                    ActionType action = blockData.GetEffectAt(i);

                    MoveDirection dir = MoveDirection.None;
                    if (placed.linkedRuntimeBlock != null)
                        dir = placed.linkedRuntimeBlock.CurrentMoveDirections[i];

                    int damage = action == ActionType.Cure
                        ? blockData.CalCulate_CurePower(i)
                        : blockData.attackDamage;

                    slotView.SetAction(
                        action,
                        dir,
                        damage,
                        isPreview: false,
                        isPrev: false
                    );

                    // 호버 핸들러 추가/업데이트
                    slotView.SetHoverData(
                        this,
                        tickIndex + 1,
                        placed,
                        isPrev: false
                    );
                }
            }
        }

        if(_currentSliderTick != -1)
            Show_Preview(_currentSliderTick);
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
            //return TimelineManager.Instance.CanPlaceAt(startTick, length);
        }

        return false;
    }

    public void UpdateDangerIndicators()
    {
        if (TimelineManager.Instance == null) return;
        foreach(GameObject slot in cursorSlots)
        {
            TimelineTickHoverHandler handler = slot.GetComponent<TimelineTickHoverHandler>();
            if (handler != null)
                handler.SetCautionStatus(false);
        }
        List<int> dangerTicks = TimelineManager.Instance.GetProjectedDangerTicks();
        foreach (int tick in dangerTicks)
        {
            int slotIndex = (tick * 2) - 1;
            if (slotIndex >= 0 && slotIndex < cursorSlots.Count)
            {
                TimelineTickHoverHandler handler = cursorSlots[slotIndex].GetComponent<TimelineTickHoverHandler>();
                if (handler != null)
                    handler.SetCautionStatus(true);
            }
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MineCell : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler
{
    [SerializeField] private Button button; // 그래픽/상태용으로만 사용해도 됨
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private GameObject cover;   // 숨김 상태 덮개
    [SerializeField] private GameObject flag;    // 깃발 표시
    [SerializeField] private GameObject mine;    // 지뢰 표시

    [SerializeField] private Color[] colors;

    [Header("Preview")]
    [SerializeField] private GameObject pressPreview; // 눌림 표시 오브젝트(하나로 합성 표시)

    public int x { get; private set; }
    public int y { get; private set; }

    private MinesweeperGridView owner;

    private CellState _state;
    private int _adjacent;

    // ✅ 프리뷰 상태를 “눌림/주변”으로 분리해서 합성 표시
    private bool _pressOn;
    private bool _neighborOn;

    public void Init(MinesweeperGridView owner)
    {
        this.owner = owner;

        // 버튼 onClick은 쓰지 않는 구조(Up에서 owner.EndPress가 실행)
        // button.onClick.RemoveAllListeners(); // 필요시
    }

    public void Bind(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public bool IsRevealedNumberCell()
        => _state == CellState.Revealed && _adjacent > 0;

    // =========================
    // 입력
    // =========================

    public void OnPointerClick(PointerEventData eventData)
    {
        if (owner == null || owner.gameOver) return;

        // 우클릭: 깃발 토글
        if (eventData.button == PointerEventData.InputButton.Right)
            owner.OnCellRightClick(x, y);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (owner == null || owner.gameOver) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        owner.BeginPress(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (owner == null || owner.gameOver) return;

        // 좌클릭을 누른 채로 들어왔을 때만 이동 눌림 처리
        if (Input.GetMouseButton(0))
            owner.MovePress(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        

        if (owner == null || owner.gameOver) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (owner.currentPressedCell != null)
            owner.EndPress(owner.currentPressedCell, cancel: false);

    }

    // =========================
    // 프리뷰 표시
    // =========================

    // “현재 눌린 셀” 프리뷰
    public void SetPreview(bool on)
    {
        _pressOn = on;
        RefreshPreview();
    }

    // chord 주변 8칸 프리뷰
    public void SetNeighborPreview(bool on)
    {
        _neighborOn = on;
        RefreshPreview();
    }

    private void RefreshPreview()
    {
        if (pressPreview == null) return;

        bool on = _pressOn || _neighborOn;

        // 원조 감성: 깃발 꽂힌 칸은 눌림 표시 안 함
        if (flag != null && flag.activeSelf) on = false;

        // 이미 열린 칸이면 프리뷰 의미 없음(주변 프리뷰도 안 보이게)
        if (cover != null && !cover.activeSelf) on = false;

        pressPreview.SetActive(on);
    }

    // =========================
    // 비주얼 갱신
    // =========================

    public void ResetVisual()
    {
        cover.SetActive(true);
        flag.SetActive(false);
        mine.SetActive(false);
        numberText.text = "";

        _pressOn = false;
        _neighborOn = false;
        RefreshPreview();
    }

    public void Apply(BoardData.Cell cell)
    {
        _state = cell.state;
        _adjacent = cell.adjacent;

        switch (cell.state)
        {
            case CellState.Hidden:
                cover.SetActive(true);
                flag.SetActive(false);
                mine.SetActive(false);
                numberText.text = "";
                break;

            case CellState.Flagged:
                cover.SetActive(true);
                flag.SetActive(true);
                mine.SetActive(false);
                numberText.text = "";
                break;

            case CellState.Revealed:
                cover.SetActive(false);
                flag.SetActive(false);

                if (cell.isMine)
                {
                    mine.SetActive(true);
                    numberText.text = "";
                }
                else
                {
                    mine.SetActive(false);
                    numberText.text = (cell.adjacent > 0) ? cell.adjacent.ToString() : "";
                }
                break;
        }

        SetTextColor(numberText.text);

        // 상태 바뀌면 프리뷰 조건도 바뀔 수 있으니 한번 갱신
        RefreshPreview();
    }

    private void SetTextColor(string number)
    {
        if (string.IsNullOrEmpty(number)) return;

        // switch 대신 파싱이 더 안전/간단
        if (!int.TryParse(number, out int n)) return;

        // colors[0]=1, [1]=2 ... [5]=6, [6]=기타(7,8)
        int idx = Mathf.Clamp(n - 1, 0, 6);
        numberText.color = colors[idx];
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MineCell : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private GameObject cover;   // 숨김 상태 덮개
    [SerializeField] private GameObject flag;    // 깃발 표시
    [SerializeField] private GameObject mine;    // 지뢰 표시 (게임오버 시)

    [SerializeField]
    private Color[] colors;

    [Header("Preview")]
    [SerializeField] private GameObject pressPreview;

    public int x { get; private set; }
    public int y { get; private set; }

    private MinesweeperGridView owner;
    private bool isPressedLeft; // 좌클릭 다운 상태 추적

    private CellState _state;
    private int _adjacent;

    public void Init(MinesweeperGridView owner)
    {
        this.owner = owner;
        button.onClick.AddListener(OnLeftClick);
    }

    public void Bind(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (owner == null) return;


        if (eventData.button == PointerEventData.InputButton.Right)
        {
            owner.OnCellRightClick(x, y);
        }
    }

    // ---- 좌클릭: Down -> Preview, Up -> 실행 ----
    public void OnPointerDown(PointerEventData eventData)
    {
        if (owner == null) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        SetPreview(true);

        // 이미 열린 "숫자칸(1~8)"일 때만 주변 눌림 프리뷰
        if (_state == CellState.Revealed && _adjacent > 0)
        {
            isPressedLeft = true;
            owner.SetChordPreview(x, y, true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (owner == null) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (!isPressedLeft) return;

        isPressedLeft = false;
        owner.SetChordPreview(x, y, false);
        SetPreview(false);

        // Up이 “내 위에서” 일어났을 때만 실행(드래그로 벗어나서 떼면 취소)
        var rayGo = eventData.pointerCurrentRaycast.gameObject;
        if (rayGo == null) return;
        if (!rayGo.transform.IsChildOf(transform)) return;

        owner.OnCellLeftClick(x, y);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 드래그로 나가면 프리뷰 취소
        if (!isPressedLeft) return;
        isPressedLeft = false;
        owner.SetChordPreview(x, y, false);
        SetPreview(false);
    }

    private void SetPreview(bool on)
    {
        // 전용 프리뷰 오브젝트 켜기/끄기
        if (pressPreview != null)
            pressPreview.SetActive(on);
    }

    public void SetNeighborPreview(bool on)
    {
        if (pressPreview == null) return;

        // 깃발이 있으면 원조처럼 안 눌리게(추천)
        if (flag.activeSelf) on = false;

        // 이미 열린 칸이면 프리뷰 의미 없음
        if (!cover.activeSelf) on = false;

        pressPreview.SetActive(on);
    }

    private void OnLeftClick()
    {
        owner.OnCellLeftClick(x, y);
    }

    public void ResetVisual()
    {
        cover.SetActive(true);
        flag.SetActive(false);
        mine.SetActive(false);
        numberText.text = "";
        SetNeighborPreview(false);
        isPressedLeft = false;
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

        Set_textColor(numberText.text);
    }

    private void Set_textColor(string number)
    {
        if (number == "") return;

        switch (number)
        {
            case "1":
                numberText.color = colors[0];
                break;
            case "2":
                numberText.color = colors[1];
                break;
            case "3":
                numberText.color = colors[2];
                break;
            case "4":
                numberText.color = colors[3];
                break;
            case "5":
                numberText.color = colors[4];
                break;
            case "6":
                numberText.color = colors[5];
                break;
            default:
                numberText.color = colors[6];
                break;

        }
    }
}


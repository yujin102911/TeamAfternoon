using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MineCell : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private GameObject cover;   // 숨김 상태 덮개
    [SerializeField] private GameObject flag;    // 깃발 표시
    [SerializeField] private GameObject mine;    // 지뢰 표시 (게임오버 시)

    [SerializeField]
    private Color[] colors;

    public int x { get; private set; }
    public int y { get; private set; }

    private MinesweeperGridView owner;

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
    }

    public void Apply(BoardData.Cell cell)
    {
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


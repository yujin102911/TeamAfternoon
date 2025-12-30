using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Action_cell : MonoBehaviour
{
    [Header("수정할 UI연결")]
    [SerializeField]
    private Image _rootImage;
    [SerializeField]
    private Image _backImage;
    [SerializeField]
    private Image _actionIcon;
    [SerializeField]
    private Image _directionIcon;
    [SerializeField]
    private TextMeshProUGUI _damageText;

    [SerializeField]
    private Sprite _noneSprite;
    [SerializeField]
    private Sprite _actionSprite;

    [TabGroup("Attack")]
    public Sprite Attack_back;
    [TabGroup("Attack")]
    public Sprite Sword_icon;
    [TabGroup("Attack")]
    public Sprite Attack_startIcon;
    [TabGroup("Attack")]
    public GameObject Attack_startLine;
    [TabGroup("Attack")]
    public GameObject Attack_middleLine;
    [TabGroup("Attack")]
    public GameObject Attack_endLine;

    [TabGroup("Move")]
    public Sprite Move_back;
    [TabGroup("Move")]
    public Sprite Shoes_icon;
    [TabGroup("Move")]
    public Sprite Front_icon;
    [TabGroup("Move")]
    public Sprite Back_icon;
    [TabGroup("Move")]
    public Sprite Left_icon;
    [TabGroup("Move")]
    public Sprite Right_icon;

    [TabGroup("Move_2")]
    public Sprite NE_icon;
    [TabGroup("Move_2")]
    public Sprite SE_icon;
    [TabGroup("Move_2")]
    public Sprite SW_icon;
    [TabGroup("Move_2")]
    public Sprite NW_icon;

    [TabGroup("Bow")]
    public Sprite Bow_back;
    [TabGroup("Bow")]
    public Sprite Bow_startIcon;
    [TabGroup("Bow")]
    public GameObject Bow_startLine;
    [TabGroup("Bow")]
    public GameObject Bow_middleLine;
    [TabGroup("Bow")]
    public GameObject Bow_endLine;

    [TabGroup("Guard")]
    public Sprite Guard_Back;
    [TabGroup("Guard")]
    public Sprite Guard_Icon;

    [TabGroup("None")]
    public Sprite Bow_endIcon;

    public void Clear()
    {
        _rootImage.sprite = _noneSprite;
        
        _backImage.gameObject.SetActive(false);

        OffLines();
    }

    // 셀 설정
    public void Update_CellVisual(ActionType action, MoveDirection dir = MoveDirection.None, int dam = -1)
    {
        _rootImage.sprite = _actionSprite;

        _backImage.gameObject.SetActive(true);
        _directionIcon.gameObject.SetActive(true);
        _damageText.text = "";

        ChangeAlpha(_backImage, 1.0f);

        OffLines();

        switch (action)
        {
            case ActionType.None:
                _backImage.gameObject.SetActive(false);
                break;

            case ActionType.Sword_start:
                Attack_startLine.SetActive(true);
                goto case ActionType.Attack;

            case ActionType.Sword_middle:
                _backImage.gameObject.SetActive(false);
                Attack_middleLine.SetActive(true);
                break;

            case ActionType.Sword_end:
                ChangeAlpha(_backImage, 0f);
                _actionIcon.sprite = Attack_startIcon;
                _directionIcon.gameObject.SetActive(false);
                _damageText.text = "";
                Attack_endLine.SetActive(true);
                break;

            case ActionType.Attack:
                _backImage.sprite = Attack_back;
                _actionIcon.sprite = Sword_icon;
                _directionIcon.gameObject.SetActive(false);
                _damageText.text = dam.ToString();
                break;

            case ActionType.Jump:
            case ActionType.Move:
                _backImage.sprite = Move_back;
                _actionIcon.sprite = Shoes_icon;
                
                switch (dir)
                {
                    case MoveDirection.Front: _directionIcon.sprite = Front_icon; break;
                    case MoveDirection.Right: _directionIcon.sprite = Right_icon; break;
                    case MoveDirection.Back: _directionIcon.sprite = Back_icon; break;
                    case MoveDirection.Left: _directionIcon.sprite = Left_icon; break;

                        //추후 방향 나오면 연결
                    case MoveDirection.DiagonalRu: _directionIcon.sprite = SE_icon; break;
                    case MoveDirection.DiagonalRd: _directionIcon.sprite = SW_icon; break;
                    case MoveDirection.DiagonalLd: _directionIcon.sprite = NW_icon; break;
                    case MoveDirection.DiagonalLu: _directionIcon.sprite = NE_icon; break;
                }

                _damageText.text = "";
                break;

            case ActionType.Bow_single:
            case ActionType.Cure:
                _backImage.sprite = Bow_back;
                _actionIcon.sprite = Bow_startIcon;
                _directionIcon.gameObject.SetActive(false);
                _damageText.text = dam.ToString();
                break;

            case ActionType.Bow_start:
                _backImage.sprite = Bow_back;
                _actionIcon.sprite = Bow_startIcon;
                _directionIcon.gameObject.SetActive(false);
                _damageText.text = dam.ToString();
                Bow_startLine.SetActive(true);
                break;

            case ActionType.Bow_middle:
                _backImage.gameObject.SetActive(false);
                Bow_middleLine.SetActive(true);
                break;

            case ActionType.Bow_end:
                ChangeAlpha(_backImage, 0f);
                _actionIcon.sprite = Bow_endIcon;
                _directionIcon.gameObject.SetActive(false);
                _damageText.text = "";
                Bow_endLine.SetActive(true);
                break;

            case ActionType.Guard:
                _backImage.sprite = Guard_Back;
                _actionIcon.sprite = Guard_Icon;
                _directionIcon.gameObject.SetActive(false);
                _damageText.text = "G";
                break;

            default:
                break;
        }
    }

    private void OffLines()
    {
        Bow_startLine.SetActive(false);
        Bow_middleLine.SetActive(false);
        Bow_endLine.SetActive(false);
        Attack_startLine.SetActive(false);
        Attack_middleLine.SetActive(false);
        Attack_endLine.SetActive(false);
    }

    private void ChangeAlpha(Image target, float alpha)
    {
        Color color = target.color;
        color.a = alpha;
        target.color = color;
    }
}


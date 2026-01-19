using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum Cell_Pos
{
    None = 0,
    Start = 1,
    Middle = 2,
    End = 3
}


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
    private Sprite[] _actionSprite;

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
    public Sprite jump_icon;
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

    // ---- 색상 상수(한번만 파싱) ----
    private static readonly Color MeleeColor = Hex("#FFA7A3");
    private static readonly Color BowColor = Hex("#FFEF64");
    private static readonly Color ForceColor = Hex("#00e48b");

    private static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
    }

    private static bool IsMelee(ActionType action)
        => action == ActionType.Attack || action == ActionType.Sword_start;

    private static bool IsBow(ActionType action)
        => action == ActionType.Bow_single || action == ActionType.Bow_start;

    private void ApplyActionColor(ActionType action)
    {
        if (IsMelee(action)) _damageText.color = Color.white;
        else if (IsBow(action)) _damageText.color = Color.white;
    }

    private void ApplyAdditionalEffect(ref int finalDam, Additional_Effect effect, ActionType action)
    {
        if (effect == null) return;

        switch (effect.effectType)
        {
            case EffectType.Damage_Up:
                finalDam += 3;
                ApplyActionColor(action);
                break;

            case EffectType.Critical:
                finalDam *= 2;
                ApplyActionColor(action);
                break;

            case EffectType.Critical_3:
                finalDam *= 3;
                ApplyActionColor(action);
                break;
        }

        if(finalDam>=10 && finalDam < 30)
        {
            _damageText.fontSize = 24;
        }
        else if (finalDam >= 30 && finalDam < 40)
        {
            _damageText.fontSize = 26;
        }
        else if (finalDam >= 40)
        {
            _damageText.fontSize = 28;
        }

        _damageText.fontStyle = FontStyles.Bold | FontStyles.Italic;
    }

    public void Clear()
    {
        _rootImage.sprite = _noneSprite;
        
        _backImage.gameObject.SetActive(false);

        OffLines();
    }

    // 셀 설정
    public void Update_CellVisual(ActionType action, MoveDirection dir = MoveDirection.None, int dam = -1, Cell_Pos cell_Pos = Cell_Pos.None,
        Additional_Effect additional_Effect = null)
    {
        _rootImage.sprite = _actionSprite[1];

        // 위치에 따른 필름 이미지 변경
        switch (cell_Pos)
        {
            case Cell_Pos.Start:
                _rootImage.sprite = _actionSprite[0];
                break;

            case Cell_Pos.End:
                _rootImage.sprite = _actionSprite[2];
                break;

            default:
                break;
        }

        _backImage.gameObject.SetActive(true);
        _directionIcon.gameObject.SetActive(true);
        _damageText.text = "";
        _damageText.color = Color.white;
        _damageText.fontSize = 22;
        _damageText.fontStyle = FontStyles.Bold;

        ChangeAlpha(_backImage, 1.0f);

        OffLines();

        int final_dam = dam;

        //특수효과로 인한 데미지 증가 계산
        ApplyAdditionalEffect(ref final_dam, additional_Effect, action);
        

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
                    _damageText.text = final_dam.ToString();
                    break;

                case ActionType.Jump:
                    _backImage.sprite = Move_back;
                    _actionIcon.sprite = jump_icon;
                    switch (dir)
                    {
                        //추후 방향 나오면 연결
                        case MoveDirection.DiagonalRu: _directionIcon.sprite = SE_icon; break;
                        case MoveDirection.DiagonalRd: _directionIcon.sprite = SW_icon; break;
                        case MoveDirection.DiagonalLd: _directionIcon.sprite = NW_icon; break;
                        case MoveDirection.DiagonalLu: _directionIcon.sprite = NE_icon; break;
                    }

                    _damageText.text = "";
                    break;

                case ActionType.Move:
                    _backImage.sprite = Move_back;
                    _actionIcon.sprite = Shoes_icon;

                    switch (dir)
                    {
                        case MoveDirection.Front: _directionIcon.sprite = Front_icon; break;
                        case MoveDirection.Right: _directionIcon.sprite = Right_icon; break;
                        case MoveDirection.Back: _directionIcon.sprite = Back_icon; break;
                        case MoveDirection.Left: _directionIcon.sprite = Left_icon; break;
                    }

                    _damageText.text = "";
                    break;

                case ActionType.Bow_single:
                case ActionType.Cure:
                    _backImage.sprite = Bow_back;
                    _actionIcon.sprite = Bow_startIcon;
                    _directionIcon.gameObject.SetActive(false);
                    _damageText.text = final_dam.ToString();
                    break;

                case ActionType.Bow_start:
                    _backImage.sprite = Bow_back;
                    _actionIcon.sprite = Bow_startIcon;
                    _directionIcon.gameObject.SetActive(false);
                    _damageText.text = final_dam.ToString();
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
                    _damageText.text = "";
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


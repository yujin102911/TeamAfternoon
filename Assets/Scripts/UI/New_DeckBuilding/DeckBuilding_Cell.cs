using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckBuilding_Cell : MonoBehaviour
{
    [Header("수정할 UI연결")]
    [SerializeField]
    private Image _actionIcon;
    [SerializeField]
    private Image _directionIcon;
    [SerializeField]
    private TextMeshProUGUI _damageText;

    [TabGroup("Attack")]
    public Sprite Attack_icon;
    [TabGroup("Attack")]
    public Color Attack_color;

    [TabGroup("Move")]
    public Sprite Move_icon;
    [TabGroup("Move")]
    public Sprite Move_Di_icon;

    [TabGroup("Bow")]
    public Sprite Bow_startIcon;
    [TabGroup("Bow")]
    public Sprite Bow_bigline;
    [TabGroup("Bow")]
    public Sprite Bow_line;
    [TabGroup("Bow")]
    public Sprite Bow_endIcon;
    [TabGroup("Bow")]
    public Color Bow_color;

    public void Clear()
    {
        _actionIcon.gameObject.SetActive(true);
        _directionIcon.gameObject.SetActive(true);
        _damageText.text = "";
    }

    // 셀 설정
    public void Update_CellVisual(ActionType action, MoveDirection dir = MoveDirection.None, int dam = -1)
    {
        Clear();

        switch (action)
        {
            case ActionType.None:
                break;

            case ActionType.Attack:
                _actionIcon.sprite = Attack_icon;
                _directionIcon.gameObject.SetActive(false);
                _damageText.color = Attack_color;
                _damageText.text = dam.ToString();
                break;

            case ActionType.Move:
                _actionIcon.sprite = Move_icon;
                _directionIcon.sprite = Move_Di_icon;
                break;

            case ActionType.Cure:
            case ActionType.Bow_start:
                _actionIcon.sprite = Bow_startIcon;
                _directionIcon.gameObject.SetActive(false);
                _damageText.color = Bow_color;
                _damageText.text = dam.ToString();
                break;

            case ActionType.Bow_middle:
                _actionIcon.sprite = Bow_bigline;
                _directionIcon.sprite = Bow_line;
                break;

            case ActionType.Bow_end:
                _actionIcon.sprite = Bow_endIcon;
                _directionIcon.sprite = Bow_line;
                break;

            default:
                break;
        }

        Color color = _damageText.color;
        color.a = 1;
        _damageText.color = color;
    }
}

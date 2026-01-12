using Sirenix.OdinInspector;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class Action_Desc_Cell : MonoBehaviour
{
    [Header("액션 DB")]
    [SerializeField]
    private ActionData _actionData;

    [Header("수정할 UI연결")]
    [SerializeField]
    private Image _colorBack;
    [SerializeField]
    private Image _icon;
    [SerializeField] private TextMeshProUGUI nameTMP;
    [SerializeField] private TextMeshProUGUI descTMP;
    [SerializeField]
    private LocalizedString _nameText;
    [SerializeField]
    private LocalizedString _descriptionText;

    [TabGroup("Attack")]
    public Sprite Attack_back;
    [TabGroup("Attack")]
    public Sprite Sword_icon;
    [TabGroup("Attack")]
    public Sprite Sword_Charge_icon;

    [TabGroup("Move")]
    public Sprite Move_back;
    [TabGroup("Move")]
    public Sprite Shoes_Icon;
    [TabGroup("Move")]
    public Sprite Jump_Icon;

    [TabGroup("Bow")]
    public Sprite Bow_back;
    [TabGroup("Bow")]
    public Sprite Bow_Icon;
    [TabGroup("Bow")]
    public Sprite Bow_Charge_Icon;

    [TabGroup("Guard")]
    public Sprite Guard_Back;
    [TabGroup("Guard")]
    public Sprite Guard_Icon;

    private void OnEnable()
    {
        _nameText.StringChanged += OnNameChanged;
        _descriptionText.StringChanged += OnDescChanged;

        _nameText.RefreshString();
        _descriptionText.RefreshString();
    }

    private void OnDisable()
    {
        _nameText.StringChanged -= OnNameChanged;
        _descriptionText.StringChanged -= OnDescChanged;
    }

    private void OnNameChanged(string value) => nameTMP.text = value;
    private void OnDescChanged(string value) => descTMP.text = value;

    // TODO: 설명들은 나중에 SO로 따로 빼기
    public void Update_descriptionCell(ActionType action, int damage)
    {
        Action_info action_Info = _actionData.GetAction_Info(action);

        this.gameObject.SetActive(true);


        // 설명 텍스트 키값으로 출력
        _nameText.TableEntryReference = action_Info.Name_key;

        _descriptionText.TableEntryReference = action_Info.Desc_Key;
        _descriptionText.Arguments = new object[]
        {
            damage
        };
        _descriptionText.RefreshString();

        switch (action)
        {
            case ActionType.Sword_start:
            case ActionType.Attack:
                _colorBack.sprite = Attack_back;
                _icon.sprite = Sword_icon;
                break;

            case ActionType.Sword_end:
            case ActionType.Sword_middle:
                _colorBack.sprite = Attack_back;
                _icon.sprite = Sword_Charge_icon;
                break;

            case ActionType.Move:
                _colorBack.sprite = Move_back;
                _icon.sprite = Shoes_Icon;
                break;

            case ActionType.Jump:
                _colorBack.sprite = Move_back;
                _icon.sprite = Jump_Icon;
                break;

            case ActionType.Bow_single:
            case ActionType.Bow_start:
                _colorBack.sprite = Bow_back;
                _icon.sprite = Bow_Icon;
                break;
                
            case ActionType.Bow_middle:
            case ActionType.Bow_end:
                _colorBack.sprite = Bow_back;
                _icon.sprite = Bow_Charge_Icon;
                break;

            case ActionType.Guard:
                _colorBack.sprite = Guard_Back;
                _icon.sprite = Guard_Icon;
                break;

            default:
                this.gameObject.SetActive(false);
                break;
        }
    }
}

using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class Mini_Action_cell : MonoBehaviour
{
    [Header("수정할 UI연결")]
    [SerializeField]
    private Image _icon;
    [SerializeField]
    private TextMeshProUGUI _typeTMP;
    [SerializeField]
    private LocalizedString _typeTxt;

    [TabGroup("Attack")]
    public Sprite Attack_icon;
    [TabGroup("Attack")]
    public string Attack_key;

    [TabGroup("Bow")]
    public Sprite Bow_icon;
    [TabGroup("Bow")]
    public string Bow_key;

    [TabGroup("Move")]
    public Sprite Move_icon;
    [TabGroup("Move")]
    public string Move_key;

    [TabGroup("Jump")]
    public Sprite Jump_icon;
    [TabGroup("Jump")]
    public string Jump_key;
    private void OnEnable()
    {
        _typeTxt.StringChanged += OnTypeChanged;
        _typeTxt.RefreshString();;
    }

    private void OnDisable()
    {
        _typeTxt.StringChanged -= OnTypeChanged;
    }

    private void OnTypeChanged(string value) => _typeTMP.text = value;


    public void Update_CellVisual(ActionType action) 
    {
        this.gameObject.SetActive(true);

        switch (action)
        {
            case ActionType.Move:
                _icon.sprite = Move_icon;
                _typeTxt.TableEntryReference = Move_key;
                break;
            case ActionType.Attack:
                _icon.sprite = Attack_icon;
                _typeTxt.TableEntryReference = Attack_key;
                break;
            case ActionType.Bow_single:
                _icon.sprite = Bow_icon;
                _typeTxt.TableEntryReference = Bow_key;
                break;
            case ActionType.Jump:
                _icon.sprite = Jump_icon;
                _typeTxt.TableEntryReference = Jump_key;
                break;
        }
    }
}

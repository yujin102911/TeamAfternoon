using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class Timeline_Action_TooltipPanel : MonoBehaviour
{
    [Header("호버 설정")]
    [SerializeField] private GameObject root;
    [SerializeField] private RectTransform panelRt;   // root의 RectTransform
    [SerializeField] private Vector2 offset = new Vector2(12f, 12f); // 마우스 기준 오른쪽 위

    [Header("액션 DB")]
    [SerializeField]
    private ActionData _actionData;

    [Header("수정할 UI연결")]
    [SerializeField]
    private Image _icon;
    [SerializeField] private TextMeshProUGUI nameTMP;
    [SerializeField] private TextMeshProUGUI descTMP;
    [SerializeField]
    private LocalizedString _nameText;
    [SerializeField]
    private LocalizedString _descriptionText;
    [SerializeField]
    private GameObject _direction;

    [TabGroup("Attack")]
    public Sprite Sword_icon;
    [TabGroup("Attack")]
    public Sprite Sword_Charge_icon;

    [TabGroup("Move")]
    public Sprite Shoes_Icon;
    [TabGroup("Move")]
    public Sprite Jump_Icon;

    [TabGroup("Bow")]
    public Sprite Bow_Icon;
    [TabGroup("Bow")]
    public Sprite Bow_Charge_Icon;

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

    public void Show(ActionType action, int damage, Vector2 screenPos, Camera cam)
    {
        this.gameObject.SetActive(true);

        root.SetActive(true);

        // ✅ pivot을 좌하단으로 강제(인스펙터에서 해도 됨)
        panelRt.pivot = Vector2.zero; // (0,0) = 좌하단

        RectTransform parentRt = panelRt.parent as RectTransform;
        if (parentRt == null) return;

        Vector2 targetScreenPos = screenPos + offset;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRt,
            targetScreenPos,
            cam,
            out Vector2 localPos
        );

        // 1) 일단 “좌하단을 마우스+offset에”
        panelRt.anchoredPosition = localPos;

        // 2) 화면(부모 Rect) 밖으로 나가지 않게 클램프
        ClampToParent(panelRt, parentRt);

        // 내용 설정
        Update_Info(action, damage);

        
    }

    private void ClampToParent(RectTransform rt, RectTransform parentRt)
    {
        // rt는 pivot=(0,0) 기준이므로
        // anchoredPosition은 “좌하단”
        Vector2 pos = rt.anchoredPosition;

        float minX = parentRt.rect.xMin;
        float maxX = parentRt.rect.xMax - rt.rect.width;
        float minY = parentRt.rect.yMin;
        float maxY = parentRt.rect.yMax - rt.rect.height;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        rt.anchoredPosition = pos;
    }


    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    private void Update_Info(ActionType action, int damage)
    {
        Action_info action_Info = _actionData.GetAction_Info(action);

        _direction.SetActive(false);

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
                _icon.sprite = Sword_icon;
                break;

            case ActionType.Sword_end:
            case ActionType.Sword_middle:
                _icon.sprite = Sword_Charge_icon;
                break;

            case ActionType.Move:
                _icon.sprite = Shoes_Icon;
                _direction.SetActive(true);
                break;

            case ActionType.Jump:
                _icon.sprite = Jump_Icon;
                _direction.SetActive(true);
                break;

            case ActionType.Bow_single:
            case ActionType.Bow_start:
                _icon.sprite = Bow_Icon;
                break;

            case ActionType.Bow_middle:
            case ActionType.Bow_end:
                _icon.sprite = Bow_Charge_Icon;
                break;

            case ActionType.Guard:
                _icon.sprite = Guard_Icon;
                break;

            default:
                this.gameObject.SetActive(false);
                break;
        }
    }
}

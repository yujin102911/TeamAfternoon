using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UI;

public class Effect_TooltipPanel : MonoBehaviour
{
    [Header("호버 설정")]
    [SerializeField] private GameObject root;
    [SerializeField] private RectTransform panelRt;   // root의 RectTransform
    [SerializeField] private RectTransform cell_Rt;   // cell_root의 RectTransform
    [SerializeField] private RectTransform name_Rt;   // 효과 설명부의 RectTransform
    [SerializeField] private Vector2 offset = new Vector2(12f, 12f); // 마우스 기준 오른쪽 위

    [Header("번역 텍스트")]
    [SerializeField] private TextMeshProUGUI nameTMP;
    [SerializeField] private TextMeshProUGUI descTMP;
    [SerializeField]
    private LocalizedString _nameText;
    [SerializeField]
    private LocalizedString _descriptionText;

    [Header("적용 범위 표시")]
    [SerializeField]
    private Mini_Action_cell[] _actionCell;

    [Header("아이콘")]
    [SerializeField]
    private Image _back;
    [SerializeField]
    private Image _icon;

    [TabGroup("Dash")]
    [SerializeField]
    private Sprite _dashBack;
    [TabGroup("Dash")]
    [SerializeField]
    private Sprite _dubleDashIcon;

    [TabGroup("Attack")]
    [SerializeField]
    private Sprite _damageBack;
    [TabGroup("Attack")]
    [SerializeField]
    private Sprite _damageUpIcon;

    [TabGroup("Critical")]
    [SerializeField]
    private Sprite _criticBack;
    [TabGroup("Critical")]
    [SerializeField]
    private Sprite _criticIcon;

    [TabGroup("Sturn")]
    [SerializeField]
    private Sprite _sturnBack;
    [TabGroup("Sturn")]
    [SerializeField]
    private Sprite _sturnIcon;

    [TabGroup("Critical_3")]
    [SerializeField]
    private Sprite _Critical_3Back;
    [TabGroup("Critical_3")]
    [SerializeField]
    private Sprite _Critical_3Icon;

    [TabGroup("HealAll")]
    [SerializeField]
    private Sprite _HealAllBack;
    [TabGroup("HealAll")]
    [SerializeField]
    private Sprite _HealAllIcon;

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

    private void Clear_Cells()
    {
        for (int i = 0; i < _actionCell.Length; i++)
        {
            _actionCell[i].gameObject.SetActive(false);
        }
    }

    public void Show(Additional_Effect effect, Vector2 screenPos, Camera cam)
    {
        this.gameObject.SetActive(true);

        root.SetActive(true);

        Clear_Cells();

        // 아이콘 설정
        Set_Icon(effect.effectType);

        Dictionary<ActionType, int> keyValuePairs = new Dictionary<ActionType, int>();

        foreach (var action in effect.Apply_actionTypes)
        {
            var key = GetGroupKey(action);
            if (key == null) continue;

            if (keyValuePairs.ContainsKey(key.Value))
                keyValuePairs[key.Value]++;
            else
                keyValuePairs[key.Value] = 1;
        }

        //전체일경우(지금은 "가드"를 포함하고있을경우)
        if (keyValuePairs.TryGetValue(ActionType.Guard, out int count) && count >= 1)
        {
            _actionCell[0].Update_CellVisual(ActionType.Guard);
        }
        else
        {

            int index = 0;
            foreach (var key in keyValuePairs.Keys)
            {
                if (keyValuePairs[key] >= 1)
                {
                    _actionCell[index].Update_CellVisual(key);
                    index++;
                }
            }
        }

        // 설명 텍스트 키값으로 출력
        _nameText.TableEntryReference = effect.effectName;

        _descriptionText.TableEntryReference = effect.effectDescription;

        _nameText.RefreshString();
        _descriptionText.RefreshString();


        // ✅ pivot을 좌하단으로 강제(인스펙터에서 해도 됨)
        panelRt.pivot = Vector2.zero; // (0,0) = 좌하단

        // ✅ 레이아웃 강제 갱신 (ContentSizeFitter/레이아웃 그룹 반영)
        ForceRebuild(cell_Rt);
        ForceRebuild(name_Rt);
        ForceRebuild(panelRt);

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
    }

    private void ForceRebuild(RectTransform rt)
    {
        // rt가 레이아웃 그룹/CSF가 붙은 "루트"라면 이것만으로 충분한 경우가 많음
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    ActionType? GetGroupKey(ActionType type)
    {
        return type switch
        {
            ActionType.Attack or ActionType.Sword_start => ActionType.Attack,
            ActionType.Bow_start or ActionType.Bow_single => ActionType.Bow_single,
            ActionType.Move => ActionType.Move,
            ActionType.Jump => ActionType.Jump,
            ActionType.Guard => ActionType.Guard,
            _ => null
        };
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

    private void Set_Icon(EffectType effectType)
    {
        switch (effectType)
        {
            case EffectType.Critical:
                _back.sprite = _criticBack;
                _icon.sprite = _criticIcon;
                break;
            case EffectType.Duble_Dash:
                _back.sprite = _dashBack;
                _icon.sprite = _dubleDashIcon;
                break;
            case EffectType.Damage_Up:
                _back.sprite = _damageBack;
                _icon.sprite = _damageUpIcon;
                break;
            case EffectType.Sturn:
                _back.sprite = _sturnBack;
                _icon.sprite = _sturnIcon;
                break;
            case EffectType.Critical_3:
                _back.sprite = _Critical_3Back;
                _icon.sprite = _Critical_3Icon;
                break;
            case EffectType.HealAll:
                _back.sprite = _HealAllBack;
                _icon.sprite = _HealAllIcon;
                break;
        }
    }


    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}

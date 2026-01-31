using Sirenix.OdinInspector;
using System.Collections;
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

        StartCoroutine(RepositionNextFrame(screenPos, cam));
    }

    private void ForceRebuild(RectTransform rt)
    {
        // rt가 레이아웃 그룹/CSF가 붙은 "루트"라면 이것만으로 충분한 경우가 많음
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    private IEnumerator RepositionNextFrame(Vector2 screenPos, Camera cam)
    {
        yield return null;                // 텍스트/레이아웃 반영 기다림
        Canvas.ForceUpdateCanvases();     // 캔버스 레이아웃 강제 반영

        ForceRebuild(cell_Rt);
        ForceRebuild(name_Rt);
        ForceRebuild(panelRt);

        RectTransform parentRt = panelRt.parent as RectTransform;
        if (parentRt == null) yield break;

        Vector2 targetScreenPos = screenPos + offset;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRt, targetScreenPos, cam, out Vector2 localPos
        );

        panelRt.anchoredPosition = localPos;
        ClampToParent(panelRt, parentRt);
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


    private void ClampToParent(RectTransform tooltip, RectTransform parent)
    {
        // 캔버스 레이아웃 최신화
        Canvas.ForceUpdateCanvases();

        // 코너 4개
        Vector3[] tipCorners = new Vector3[4];
        Vector3[] parentCorners = new Vector3[4];

        tooltip.GetWorldCorners(tipCorners);   // 0:LB 1:LT 2:RT 3:RB
        parent.GetWorldCorners(parentCorners);

        float tipLeft = tipCorners[0].x;
        float tipTop = tipCorners[1].y;
        float tipRight = tipCorners[2].x;
        float tipBottom = tipCorners[3].y;

        float pLeft = parentCorners[0].x;
        float pTop = parentCorners[1].y;
        float pRight = parentCorners[2].x;
        float pBottom = parentCorners[3].y;

        // 부모 밖으로 나간 만큼 계산
        float dx = 0f;
        float dy = 0f;

        if (tipRight > pRight) dx -= (tipRight - pRight);
        if (tipLeft < pLeft) dx += (pLeft - tipLeft);

        if (tipTop > pTop) dy -= (tipTop - pTop);
        if (tipBottom < pBottom) dy += (pBottom - tipBottom);

        if (dx == 0f && dy == 0f) return;

        // world delta → parent local delta로 변환 후 위치 보정
        Vector2 localDelta = WorldDeltaToParentLocal(parent, new Vector3(dx, dy, 0f));
        tooltip.anchoredPosition += localDelta;
    }

    private Vector2 WorldDeltaToParentLocal(RectTransform parent, Vector3 worldDelta)
    {
        // 부모의 right/up 방향으로 투영해서 local delta로 변환
        Vector3 right = parent.right;
        Vector3 up = parent.up;

        float localX = Vector3.Dot(worldDelta, right) / parent.lossyScale.x;
        float localY = Vector3.Dot(worldDelta, up) / parent.lossyScale.y;

        return new Vector2(localX, localY);
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

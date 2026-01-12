using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class Effect_TooltipPanel : MonoBehaviour
{
    [Header("호버 설정")]
    [SerializeField] private GameObject root;
    [SerializeField] private RectTransform panelRt;   // root의 RectTransform
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


        Clear_Cells();

        

        // 아이콘 설정
        Set_Icon(effect.effectType);

        // 설명 텍스트 키값으로 출력
        _nameText.TableEntryReference = effect.effectName;

        _descriptionText.TableEntryReference = effect.effectDescription;

        for (int i = 0; i < effect.Apply_actionTypes.Length; i++)
        {
            _actionCell[i].Update_CellVisual(effect.Apply_actionTypes[i]);
        }
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
        }
    }


    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}

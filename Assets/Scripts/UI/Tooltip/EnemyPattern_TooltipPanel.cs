using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class EnemyPattern_TooltipPanel : MonoBehaviour
{
    [Header("호버 설정")]
    [SerializeField] private GameObject root;
    [SerializeField] private RectTransform panelRt;   // root의 RectTransform
    [SerializeField] private Vector2 offset = new Vector2(12f, 12f); // 마우스 기준 오른쪽 위

    [Header("표시 아이콘")]
    [SerializeField]
    private Image _icon;

    [Header("공격")]
    [SerializeField]
    private Pattern_Key_Data _attackData;
    [SerializeField]
    private Sprite _attackIcon;
    [SerializeField]
    private Color _attackColor;

    [Header("돌진")]
    [SerializeField]
    private Pattern_Key_Data _dashData;
    [SerializeField]
    private Sprite _dashIcon;
    [SerializeField]
    private Color _dashColor;

    [Header("바람")]
    [SerializeField]
    private Pattern_Key_Data _windData;
    [SerializeField]
    private Sprite _windIcon;
    [SerializeField]
    private Color _windColor;

    [Header("돌")]
    [SerializeField]
    private Pattern_Key_Data _stoneData;
    [SerializeField]
    private Sprite _stoneIcon;
    [SerializeField]
    private Color _stoneColor;

    [Header("기절 패널")]
    [SerializeField]
    private GameObject _stunPanel;

    [Header("수정할 UI연결")]
    [SerializeField] private TextMeshProUGUI nameTMP;
    [SerializeField] private TextMeshProUGUI descTMP;
    [SerializeField] private TextMeshProUGUI typeTMP;
    [SerializeField]
    private LocalizedString _nameText;
    [SerializeField]
    private LocalizedString _descriptionText;
    [SerializeField]
    private LocalizedString _typeText;

    private object[] _descArgs = new object[1];


    private void Awake()
    {
        _nameText.StringChanged += OnNameChanged;
        _descriptionText.StringChanged += OnDescChanged;
        _typeText.StringChanged += OnTypeChanged;
    }

    private void OnDestroy()
    {
        _nameText.StringChanged -= OnNameChanged;
        _descriptionText.StringChanged -= OnDescChanged;
        _typeText.StringChanged -= OnTypeChanged;
    }

    private void OnNameChanged(string value) => nameTMP.text = value;
    private void OnDescChanged(string value) => descTMP.text = value;
    private void OnTypeChanged(string value) => typeTMP.text = value;

    public void Show(Pattern_Label label, Vector2 screenPos, Camera cam, int stone_num)
    {
        this.gameObject.SetActive(true);
        root.SetActive(true);

        // 내용 설정
        Update_Info(label, stone_num);

        // ✅ pivot을 좌하단으로 강제(인스펙터에서 해도 됨)
        panelRt.pivot = Vector2.zero; // (0,0) = 좌하단

        RectTransform parentRt = panelRt.parent as RectTransform;
        if (parentRt == null) return;

        // ✅ 레이아웃 강제 갱신 (ContentSizeFitter/레이아웃 그룹 반영)
        ForceRebuild(panelRt);

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
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        Canvas.ForceUpdateCanvases();
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
        _stunPanel.SetActive(false);
    }

    private void Update_Info(Pattern_Label label, int stone_num)
    {
        bool is_hard = false;
        Pattern_Key_Data pattern_Key_Data = null;

        if (GameManager.Instance != null)
            is_hard = GameManager.Instance.UserGameData.Difficulty == Difficulty.Hard;

        _icon.sprite = _attackIcon;
        _icon.color = _attackColor;

        switch (label)
        {
            case Pattern_Label.Attack:
                pattern_Key_Data = _attackData; 
                if(is_hard)
                    _stunPanel.SetActive(true);
                break;
            case Pattern_Label.Dash:
                pattern_Key_Data = _dashData;

                _icon.sprite = _dashIcon;
                _icon.color = _dashColor;

                if (is_hard)
                    _stunPanel.SetActive(true);
                break;
            case Pattern_Label.Wind:
                pattern_Key_Data = _windData;

                _icon.sprite = _windIcon;
                _icon.color = _windColor;
                break;
            case Pattern_Label.Stone:
                pattern_Key_Data = _stoneData;

                _icon.sprite = _stoneIcon;
                _icon.color = _stoneColor;
                break;
        }

        // 설명 텍스트 키값으로 출력
        _nameText.TableEntryReference = pattern_Key_Data.Name_key;

        _descArgs[0] = stone_num;
        _descriptionText.TableEntryReference = pattern_Key_Data.Desc_key;
        _descriptionText.Arguments = _descArgs;

        _typeText.TableEntryReference = pattern_Key_Data.Type_key;

        if(is_hard)
            _typeText.TableEntryReference = pattern_Key_Data.Hard_key;

        _nameText.RefreshString();
        _descriptionText.RefreshString();
        _typeText.RefreshString();
    }
}

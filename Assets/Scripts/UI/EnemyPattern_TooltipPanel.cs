using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class EnemyPattern_TooltipPanel : MonoBehaviour
{
    [Header("호버 설정")]
    [SerializeField] private GameObject root;
    [SerializeField] private RectTransform panelRt;   // root의 RectTransform
    [SerializeField] private Vector2 offset = new Vector2(12f, 12f); // 마우스 기준 오른쪽 위

    [Header("공격")]
    [SerializeField]
    private Pattern_Key_Data _attackData;
    [Header("돌진")]
    [SerializeField]
    private Pattern_Key_Data _dashData;
    [Header("바람")]
    [SerializeField]
    private Pattern_Key_Data _windData;
    [Header("돌")]
    [SerializeField]
    private Pattern_Key_Data _stoneData;

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

    private void OnEnable()
    {
        _nameText.StringChanged += OnNameChanged;
        _descriptionText.StringChanged += OnDescChanged;
        _typeText.StringChanged += OnTypeChanged;

        _nameText.RefreshString();
        _descriptionText.RefreshString();
        _typeText.RefreshString();
    }

    private void OnDisable()
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
        Update_Info(label, stone_num);

        Debug.Log($"[Tooltip] NameTable={_nameText.TableReference}  NameKey={_nameText.TableEntryReference}");
        Debug.Log($"[Tooltip] DescTable={_descriptionText.TableReference} DescKey={_descriptionText.TableEntryReference}");

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

    private void Update_Info(Pattern_Label label, int stone_num)
    {
        Pattern_Key_Data pattern_Key_Data = null;

        switch (label)
        {
            case Pattern_Label.Attack:
                pattern_Key_Data = _attackData; 
                break;
            case Pattern_Label.Dash:
                pattern_Key_Data = _dashData;
                break;
            case Pattern_Label.Wind:
                pattern_Key_Data = _windData;
                break;
            case Pattern_Label.Stone:
                pattern_Key_Data = _stoneData;
                break;
        }

        // 설명 텍스트 키값으로 출력
        _nameText.TableEntryReference = pattern_Key_Data.Name_key;

        _descriptionText.TableEntryReference = pattern_Key_Data.Desc_key;
        _descriptionText.Arguments = new object[]
        {
            stone_num
        };
        _typeText.TableEntryReference = pattern_Key_Data.Type_key;
    }
}

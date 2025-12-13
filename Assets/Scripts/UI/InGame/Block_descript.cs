using TMPro;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.EventSystems;

public class Block_descript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RuntimeBlock _runtimeBlock;

    [SerializeField]
    private RectTransform _rectTransform;
    [SerializeField]
    private TextMeshProUGUI _blockNameText;
    [SerializeField] 
    private TextMeshProUGUI _keywordNameText;

    private Canvas canvas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetUp(int start_tick, float width, float spacing, RuntimeBlock runtimeBlock)
    {
        _runtimeBlock = runtimeBlock;

        _rectTransform.position = new Vector3(
            _rectTransform.position.x + (start_tick - 1) * (width + spacing),
            _rectTransform.position.y,
            _rectTransform.position.z
        );

        // 너비 설정
        Vector2 size = _rectTransform.sizeDelta;
        int length = runtimeBlock.BaseData.blockLength;
        size.x = (width * length) + spacing * (length - 1);
        _rectTransform.sizeDelta = size;

        // 이름 설정
        _blockNameText.text = runtimeBlock.BaseData.blockName;

        _keywordNameText.text = "";
        // 키워드 설정
        if (runtimeBlock.AttachedKeywords.Count > 0)
        {
            for (int i = 0; i < runtimeBlock.AttachedKeywords.Count; i++)
            {
                _keywordNameText.text += $"#{runtimeBlock.AttachedKeywords[i].KeywordName} ";
            }
        }
        else
        {
            _keywordNameText.text = "";
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null) return;

        showPanel();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CardTooltip.Instance != null)
        {
            CardTooltip.Instance.Hide();
        }
    }

    private void showPanel()
    {
        if (_runtimeBlock == null) return;
        if (CardTooltip.Instance == null) return;

        bool hasSpecial = _runtimeBlock.AttachedKeywords != null && _runtimeBlock.AttachedKeywords.Count > 0;
        if (!hasSpecial)
            return;

        string effectTitle = "";
        string description = "";

        // 캔버스에 연결된 카메라 사용 (Screen Space - Camera 대응)
        Camera cam = canvas != null ? canvas.worldCamera : Camera.main;

        // 카드 Rect의 오른쪽 중앙 월드 좌표 (pivot = 0, 0.5 기준)
        Vector3 worldRightCenter = _rectTransform.TransformPoint(
            new Vector3(
                _rectTransform.rect.width + 100.0f,
                _rectTransform.rect.height * 0.5f,
                0f
            )
        );

        Debug.Log($"worldRightCenter: {worldRightCenter}");

        // 월드 → 스크린 좌표
        Vector2 screenPos =
            RectTransformUtility.WorldToScreenPoint(cam, worldRightCenter);

        Debug.Log($"screenPos: {screenPos}");


        int index = 0;

        foreach (KeywordData keyword in _runtimeBlock.AttachedKeywords)
        {
            if (keyword == null) continue;

            // 이름 세팅
            effectTitle = $"#{keyword.KeywordName}";
            description = $"{keyword.KeywordDescription}";

            CardTooltip.Instance.Show(
            effectTitle,
            description,
            screenPos,
            cam,
            index
            );

            index++;
        }
    }
}

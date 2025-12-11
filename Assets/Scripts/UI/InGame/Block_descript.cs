using TMPro;
using UnityEngine;

public class Block_descript : MonoBehaviour
{
    [SerializeField]
    private RectTransform _rectTransform;
    [SerializeField]
    private TextMeshProUGUI _blockNameText;
    [SerializeField] 
    private TextMeshProUGUI _keywordNameText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetUp(int start_tick, float width, float spacing, RuntimeBlock runtimeBlock)
    {
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
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ReturnKeyword_Dropzone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Image _image;
    private Color _highlightColor = new Color(1f, 1f, 0f, 0.5f); // 강조 색상 (노란색 반투명)
    private Color _originalColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _image = GetComponent<Image>();
        if (_image != null)
        {
            _originalColor = _image.color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null || eventData.pointerDrag.GetComponent<ReturnKeyword>() == null) return;

        if (_image != null)
        {
            _image.color = _highlightColor;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (_image != null)
        {
            _image.color = _originalColor;
        }

        if (eventData.pointerDrag.GetComponent<ReturnKeyword>() == null) return;

        // 드래그 중인 키워드 가져오기
        KeywordData keyword_info = eventData.pointerDrag.GetComponent<ReturnKeyword>().Keyword_Data;

        if (keyword_info != null && DeckBuildingManager.Instance != null)
        {
            // 배치 시도
            bool success = DeckBuildingManager.Instance.TryRemove_Keyword(keyword_info.KeywordID);

        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_image != null)
        {
            _image.color = _originalColor;
        }
    }

    

    // Update is called once per frame
    void Update()
    {
        
    }
}

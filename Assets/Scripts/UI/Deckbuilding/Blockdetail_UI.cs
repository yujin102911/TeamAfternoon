using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Blockdetail_UI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private RuntimeBlock _runtimeBlock;

    [SerializeField]
    private Draggable_Block _tickInfo; // 틱 정보 출력 담당
    [SerializeField]
    private TextMeshProUGUI _damageTxt; // 데미지 텍스트 UI

    [Header("키워드 UI")]
    [SerializeField]
    private Keyword_UI[] _keywordUIs;    // 키워드 UI 배열
    [Header("키워드 반납")]
    [SerializeField]
    private ReturnKeyword[] _returnKeywords; // 키워드 반납 데이터 배열
    [Header("키워드 상세 UI")]
    [SerializeField]
    private KeywordDetail_UI[] _keywordDetailUIs;    // 키워드 상세 UI 배열


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

        if (this.gameObject.activeSelf)
            this.gameObject.SetActive(false);

        if(DeckBuildingManager.Instance != null)
            DeckBuildingManager.Instance.OnBlockDetailChanged += Show;
    }

    private void OnDestroy()
    {
        DeckBuildingManager.Instance.OnBlockDetailChanged -= Show;
    }

    public void Show()
    {
        if (_runtimeBlock == null) return;

        this.gameObject.SetActive(true);

        // 틱 정보 출력
        _tickInfo.Show(_runtimeBlock.BaseData);

        // 데미지 텍스트 설정
        _damageTxt.text = "데미지: " + _runtimeBlock.BaseData.attackDamage.ToString();

        // 키워드 ID 리스트 가져오기
        List<int> keywords = DeckBuildingManager.Instance.GetKeywords(_runtimeBlock.BlockID);

        if (keywords != null)
        {
            // 모든 키워드 UI 숨기기
            for (int i = 0; i < _keywordUIs.Length; i++)
            {
                _keywordUIs[i].Hide();
            }

            for (int i = 0; i < _keywordDetailUIs.Length; i++)
            {
                _keywordDetailUIs[i].Hide();
            }

            int index = 0;

            // 키워드 UI 설정
            foreach (int keyword_id in keywords)
            {
                KeywordData keyword = DataRepository.Instance.GetKeyword(keyword_id);

                // 반납용 데이터 설정
                if (_returnKeywords[index] != null)
                {
                    _returnKeywords[index].Keyword_Data = keyword;
                }

                if (_keywordUIs[index] != null)
                {
                    _keywordUIs[index].SetAndShow(keyword.KeywordName);

                }

                if (_keywordDetailUIs[index] != null)
                {
                    _keywordDetailUIs[index].SetAndShow(keyword.KeywordName, keyword.KeywordDescription);
                }


                index++;
            }
        }
    }

    public void Show(RuntimeBlock rBlock)
    {
        _runtimeBlock = rBlock;

        this.gameObject.SetActive(true);

        // 틱 정보 출력
        _tickInfo.Show(rBlock.BaseData);

        // 데미지 텍스트 설정
        _damageTxt.text = "데미지: " + rBlock.BaseData.attackDamage.ToString();

        // 키워드 ID 리스트 가져오기
        List<int> keywords = DeckBuildingManager.Instance.GetKeywords(rBlock.BlockID);

        if (keywords != null)
        {
            // 모든 키워드 UI 숨기기
            for (int i = 0; i < _keywordUIs.Length; i++)
            {
                _keywordUIs[i].Hide();
            }

            for (int i = 0; i < _keywordDetailUIs.Length; i++)
            {
                _keywordDetailUIs[i].Hide();
            }

            int index = 0;

            // 키워드 UI 설정
            foreach (int keyword_id in keywords)
            {
                KeywordData keyword = DataRepository.Instance.GetKeyword(keyword_id);

                // 반납용 데이터 설정
                if (_returnKeywords[index] != null)
                {
                    _returnKeywords[index].Keyword_Data = keyword;
                }

                if (_keywordUIs[index] != null)
                {
                    _keywordUIs[index].SetAndShow(keyword.KeywordName);
                    
                }

                if (_keywordDetailUIs[index] != null)
                {
                    _keywordDetailUIs[index].SetAndShow(keyword.KeywordName, keyword.KeywordDescription);
                }


                index++;
            }
        }
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);

        for (int i = 0; i < _keywordDetailUIs.Length; i++)
        {
            _keywordDetailUIs[i].Hide();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null || eventData.pointerDrag.GetComponent<KeywordSpawn_UI>() == null) return;

        if (_image != null)
        {
            _image.color = _highlightColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_image != null)
        {
            _image.color = _originalColor;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (_image != null)
        {
            _image.color = _originalColor;
        }

        if(eventData.pointerDrag.GetComponent<KeywordSpawn_UI>() == null) return;

        // 드래그 중인 키워드 가져오기
        KeywordData keyword_info = eventData.pointerDrag.GetComponent<KeywordSpawn_UI>().Keyword_Data;

        if (keyword_info != null && DeckBuildingManager.Instance != null)
        {
            // 배치 시도
            bool success = DeckBuildingManager.Instance.TrySet_Keyword(_runtimeBlock.BaseData.blockID, keyword_info.KeywordID);

        }
    }
}

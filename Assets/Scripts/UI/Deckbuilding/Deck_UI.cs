using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// ========================================
// 덱빌딩 씬에서 덱 UI 출력을 담당하는 클래스
// ========================================
public class Deck_UI : MonoBehaviour
{
    public RuntimeBlock R_Block;

    [Header("참조")]
    public TextMeshProUGUI BlockNameText;
    public TextMeshProUGUI BlockInfoText;
    public TextMeshProUGUI BlockDamageText;

    [SerializeField]
    private Draggable_Block _tickInfo; // 틱 정보 출력 담당
    [SerializeField]
    private Keyword_UI[] _keywordUIs;    // 키워드 UI 배열
    [SerializeField]
    private Button _button;
    [SerializeField]
    private Image _image; // 선택 강조 이미지
    private Color _highlightColor = new Color(1f, 1f, 0f, 0.5f); // 강조 색상 (노란색 반투명)
    private Color _originColor;

    public bool _isSelected = false; // 선택 상태

    private void OnEnable()
    {
        _originColor = _image.color;
    }


    private void OnDisable()
    {
        _image.color = _originColor;
        _isSelected = false;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(RuntimeBlock rBlock)
    {
        _isSelected = DeckBuildingManager.Instance.SelectedBlockID == rBlock.BlockID;

        // 색변경
        if (_isSelected)
        {
            DeckBuildingManager.Instance.SetcurrentSlot(this);
            _image.color = _highlightColor;
        }
        else
        {
            _image.color = _originColor;
        }

        //블럭 이름 설정
        if (BlockNameText != null) 
            BlockNameText.text = rBlock.BaseData.BlockName;

        if(BlockDamageText != null)
            BlockDamageText.text = "데미지: " + rBlock.BaseData.attackDamage.ToString();

        // 틱 정보 출력
        _tickInfo.Show(rBlock.BaseData);

        BlockInfoText.text = "";

        // 키워드 ID 리스트 가져오기
        List<int> keywords = DeckBuildingManager.Instance.GetKeywords(rBlock.BlockID);

        if (keywords != null)
        {
            // 모든 키워드 UI 숨기기
            for (int i = 0; i < _keywordUIs.Length; i++)
            {
                _keywordUIs[i].Hide();
            }

            int index = 0;

            // 키워드 UI 설정
            foreach (int keyword_id in keywords)
            {
                KeywordData keyword = DataRepository.Instance.GetKeyword(keyword_id);

                BlockInfoText.text += $"#{keyword.KeywordName}\n";

                if (_keywordUIs[index] != null)
                {
                    _keywordUIs[index].SetAndShow(keyword.KeywordName);
                    index++;
                }
            }
        }

        // 버튼 클릭 리스너 설정
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() =>
        {
            DeckBuildingManager.Instance.OnSlotClicked(this);
        });
    }

    public void SetSelected(bool isSelected)
    {
        _isSelected = isSelected;

        // 색변경
        if (_isSelected)
        {
            _image.color = _highlightColor;
        }
        else
        {
            _image.color = _originColor;
        }
    }

}

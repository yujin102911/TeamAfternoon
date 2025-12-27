using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Unlocked_BlockUI : MonoBehaviour, IPointerClickHandler
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
        R_Block = rBlock;

        //블럭 이름 설정
        if (BlockNameText != null)
            BlockNameText.text = rBlock.BaseData.BlockName;

        if (BlockDamageText != null)
            BlockDamageText.text = "데미지: " + rBlock.BaseData.attackDamage.ToString();

        // 틱 정보 출력
        _tickInfo.Show(rBlock.BaseData);

        BlockInfoText.text = "";

        // 버튼 클릭 리스너 설정
        //_button.onClick.RemoveAllListeners();
        //_button.onClick.AddListener(() =>
        //{
        //    //DeckBuildingManager.Instance.OnSlotClicked(this);
        //});
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

    public void OnPointerClick(PointerEventData eventData)
    {
        return;

        //IngameBuildingManager.Instance.OnSlotClicked(this);
    }
}

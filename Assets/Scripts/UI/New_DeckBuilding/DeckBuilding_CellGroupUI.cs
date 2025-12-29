using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckBuilding_CellGroupUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public RuntimeBlock R_Block;

    [Header("UI 참조")]
    public TextMeshProUGUI BlockNameText;

    [SerializeField]
    private DeckBuilding_Cell[] _cells;

    [SerializeField]
    private GameObject _leftLine;
    [SerializeField]
    private GameObject _rightLine;

    [SerializeField]
    private Image _image; // 선택 강조 이미지
    [SerializeField]
    private Color _selectedColor; // 강조 색상
    [SerializeField]
    private Color _highlightColor; // 강조 색상
    private Color _originColor;

    public bool _isSelected = false; // 선택 상태

    private void OnEnable()
    {
        _originColor = _image.color;
        Reset_Cells();

        _leftLine.SetActive(false);
        _rightLine.SetActive(false);
    }


    private void OnDisable()
    {
        _image.color = _originColor;
        _isSelected = false;
    }

    public void Init(RuntimeBlock rBlock)
    {
        R_Block = rBlock;

        //블럭 이름 설정
        if (BlockNameText != null)
            BlockNameText.text = rBlock.BaseData.BlockName;

        // 틱 정보 출력
        for(int i = 0; i < rBlock.BaseData.BlockLength; i++)
        {
            ActionType action = rBlock.BaseData.GetEffectAt(i);

            int damage = rBlock.BaseData.AttackDamage;

            _cells[i].Update_CellVisual(action, MoveDirection.None, damage);

            if(i == 1)
            {
                _rightLine.SetActive(true);
            }

            if (i == 2)
            {
                _leftLine.SetActive(true);
            }

            _cells[i].gameObject.SetActive(true);
        }
    }

    public void SetSelected(bool isSelected)
    {
        _isSelected = isSelected;

        // 색변경
        if (_isSelected)
        {
            _image.color = _selectedColor;
        }
        else
        {
            _image.color = _originColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IngameBuildingManager.Instance == null) return;

        IngameBuildingManager.Instance.OnSlotClicked(this);
    }

    private void Reset_Cells()
    {
        if (_cells == null) return;

        for(int i = 0; i < _cells.Length; i++)
        {
            if(_cells[i] == null) continue;
            _cells[i].gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isSelected) return;

        _image.color = _highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isSelected) return;

        _image.color = _originColor;
    }
}

using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;
using VInspector.Libs;

public class IngameBuildingManager : MonoBehaviour
{
    public static IngameBuildingManager Instance;

    [Header("블럭 정보창")]
    [SerializeField] 
    private New_DetailPanel detailUI;

    [Header("텍스트")]
    [SerializeField]
    private TextMeshProUGUI _handCountTxt;

    [Header("패널")]
    [SerializeField]
    private GameObject _handPanel;
    [SerializeField]
    private GameObject _gamestartPanel;
    [SerializeField]
    private GameObject _buildingPanel;

    [Header("버튼 참조")]
    [SerializeField]
    private Button _addBtn;
    [SerializeField]
    private Button _bigOpenBtn;
    [SerializeField]
    private Button _smallOpenBtn;
    [SerializeField]
    private Button _gameStartBtn;

    private List<RuntimeBlock> _buildingDeck = new List<RuntimeBlock>();
    private List<RuntimeBlock> _buildingHand = new List<RuntimeBlock>();

    private SelectionManager<DeckBuilding_CellGroupUI> _selection = new();

    private int _maxHandCount;

    public int? SelectedBlockID = null;
    

    // 이벤트 (UI가 구독)
    public event Action<List<RuntimeBlock>> OnBuildingDeckChanged;
    public event Action<List<RuntimeBlock>> OnBuildingHandChanged;

    public IReadOnlyList<RuntimeBlock> BuildingDeck => _buildingDeck;
    public IReadOnlyList<RuntimeBlock> BuildingHand => _buildingHand;

    private void OnEnable()
    {
        OnBuildingHandChanged += Update_HandTxt;
        OnBuildingHandChanged += UpdateGameStartButton;
    }

    private void OnDisable()
    {
        OnBuildingHandChanged -= Update_HandTxt;
        OnBuildingHandChanged -= UpdateGameStartButton;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _buildingHand.Clear();
        // 유저가 알고 있는 블럭들의 정보를 가져옴
        _buildingDeck.Clear();

        if (_buildingDeck.Count == 0 && GameManager.Instance != null)
        {
            foreach (var block in GameManager.Instance.UserGameData.Unlocked_Blocks)
            {
                int blockId = block.Owner_blockID;
                RuntimeBlock runtimeBlock = GameManager.Instance.DeckSystem.CreateRuntimeBlock(blockId);
                if (runtimeBlock != null)
                {
                    _buildingDeck.Add(runtimeBlock);
                }
            }
        }

        if(GameManager.Instance != null)
        {
            _maxHandCount = GameManager.Instance.StartHandSize;
        }

        Connect_BtnEvent();

        Update_HandTxt(_buildingHand);
        UpdateGameStartButton(_buildingHand);
    }

    /// <summary>
    /// 각 버튼들에 이벤트 연결
    /// </summary>
    private void Connect_BtnEvent()
    {
        // 추가버튼 이벤트
        if (_addBtn != null)
        {
            _addBtn.onClick.RemoveAllListeners();
            _addBtn.onClick.AddListener(Deck_to_Hand);
        }

        // 큰 빌딩 패널 열기 버튼
        if (_bigOpenBtn != null)
        {
            _bigOpenBtn.onClick.RemoveAllListeners();
            _bigOpenBtn.onClick.AddListener(OnClicked_BigOpen);
        }

        // 작은 빌딩 패널 열기 버튼
        if (_smallOpenBtn != null)
        {
            _smallOpenBtn.onClick.RemoveAllListeners();
            _smallOpenBtn.onClick.AddListener(OnClicked_SmallOpen);
        }

        // 게임 시작 버튼
        if (_gameStartBtn != null)
        {
            _gameStartBtn.onClick.RemoveAllListeners();
            _gameStartBtn.onClick.AddListener(OnClicked_GameStart);
        }
    }

    /// <summary>
    /// 블록 페이지 슬롯 클릭 시 호출
    /// </summary>
    public void OnSlotClicked(DeckBuilding_CellGroupUI slot)
    {
        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (ctrl)
        {
            _selection.Toggle(slot);
        }
        else
        {
            _selection.SelectSingle(slot);
        }

        UpdateVisuals();
        UpdateDetail();
    }

    private void UpdateVisuals()
    {
        foreach (DeckBuilding_CellGroupUI slot in FindObjectsByType<DeckBuilding_CellGroupUI>(FindObjectsSortMode.None))
        {
            bool selected = _selection.Selected.Contains(slot);
            slot.SetSelected(selected);
        }
    }

    private void UpdateDetail()
    {
        if (_selection.Focused == null)
        {
            detailUI.Hide();
            return;
        }

        detailUI.Show(_selection.Focused.R_Block);
    }

    /// <summary>
    /// 손패 개수 UI 업데이트
    /// </summary>
    /// <param name="hand"></param>
    private void Update_HandTxt(List<RuntimeBlock> hand)
    {
        _handCountTxt.text = $"My Clip {hand.Count} / {_maxHandCount}";
    }

    private void UpdateGameStartButton(List<RuntimeBlock> hand)
    {
        _gameStartBtn.gameObject.SetActive(hand.Count > 0);
    }


    private void OnClicked_BigOpen()
    {
        _buildingPanel.SetActive(true);
        _handPanel.SetActive(true);
        _bigOpenBtn.gameObject.SetActive(false);
        
        OnBuildingDeckChanged?.Invoke(_buildingDeck);
    }

    private void OnClicked_SmallOpen()
    {
        _buildingPanel.SetActive(true);
        
        OnBuildingDeckChanged?.Invoke(_buildingDeck);
    }

    private void OnClicked_GameStart()
    {
        _handPanel.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.GameStart(_buildingHand);
    }

    // 추가 버튼에서 작동
    private void Deck_to_Hand()
    {
        if (_buildingHand.Count + _selection.Selected.Count > _maxHandCount) return; // 덱 최대 매수 오버

        List<DeckBuilding_CellGroupUI> toRemove = new List<DeckBuilding_CellGroupUI>();

        foreach (var cellGroup in _selection.Selected)
        {
            RuntimeBlock r_block = cellGroup.R_Block;

            if (MoveDeckToHand(r_block))
            {
                toRemove.Add(cellGroup);
            }
        }

        foreach (var cell in toRemove)
        {
            _selection.Selected.Remove(cell);
        }

        detailUI.Hide();

        OnBuildingDeckChanged?.Invoke(_buildingDeck);
        OnBuildingHandChanged?.Invoke(_buildingHand);
    }

    public bool MoveDeckToHand(RuntimeBlock block)
    {
        

        if (_buildingDeck.Remove(block))
        {
            _buildingHand.Add(block);
            return true;
        }

        return false; // 덱에 없었음
    }

    public void Hand_to_Deck(RuntimeBlock block)
    {
        if (_buildingHand.Remove(block))
        {
            _buildingDeck.Add(block);
        }

        OnBuildingDeckChanged?.Invoke(_buildingDeck);
        OnBuildingHandChanged?.Invoke(_buildingHand);
    }
}

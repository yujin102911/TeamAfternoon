using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Collections.AllocatorManager;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class DeckBuildingManager : MonoBehaviour
{
    public static DeckBuildingManager Instance;

    [Header("데이터 참조")]
    [SerializeField] private DataRepository _repo;
    [SerializeField] private UserGameData _user;

    [Header("돌아기가 버튼")]
    [SerializeField]
    private Button _backBtn;

    [Header("책 패널들")]
    [SerializeField]
    private GameObject[] _panels;
    private GameObject _currentPanel;

    [Header("블럭 정보창")]
    [SerializeField] private Blockdetail_UI detailUI;
    private Deck_UI current;
    private BlockData _seleckedBlock;

    public int Number_buffer = 0;
    public int? SelectedBlockID = null;

    // 외부 접근용 프로퍼티
    public DataRepository DataRepository => _repo;
    public UserGameData UserGameData => _user;

    // 블록의 키워드 변경 시 발생하는 이벤트
    public event Action<List<Owned_Keyword_Data>> OnKeywordChanged;
    // 덱 변경 시 발생하는 이벤트
    public event Action<List<int>> OnDeckChanged;
    public event Action OnBlockDetailChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _currentPanel = _panels[0];
        if (_currentPanel != null)
            _currentPanel.SetActive(true);

        if(_backBtn != null && ServiceLocator.Instance != null)
            _backBtn.onClick.AddListener(() => ServiceLocator.Instance.Scene.Back());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<int> GetKeywords(int block_id)
    {
        List<int> result = new List<int>();
        Saved_BlockData saved = _user.Unlocked_Blocks.Find(b => b.Owner_blockID == block_id);
        
        if (saved != null)
        {
            result = saved.Attached_Keyword_IDs;
        }

        return result;
    }

    /// <summary>
    /// 유저 데이터의 덱에 블록 추가
    /// </summary>
    public bool AddBlockToDeck(int blockId)
    {
        if (!_repo.blockDatas.ContainsKey(blockId))
        {
            Debug.LogError($"Block({blockId}) not found in DB");
            return false;
        }

        if (_user.Deck_Block_IDs.Contains(blockId))
            return false; // 중복 불가라면

        _user.Deck_Block_IDs.Add(blockId);
        return true;
    }

    /// <summary>
    /// 유저 데이터의 덱에 블록 제거
    /// </summary>
    public void RemoveBlock(int blockId)
    {
        _user.Deck_Block_IDs.Remove(blockId);
    }

    public void SetcurrentSlot(Deck_UI slot)
    {
        current = slot;
    }


    /// <summary>
    /// 블록 페이지 슬롯 클릭 시 호출
    /// </summary>
    public void OnSlotClicked(Deck_UI slot)
    {
        SelectedBlockID = slot.R_Block.BlockID;

        // 같은 슬롯 클릭 → 토글 OFF
        if (current == slot)
        {
            current.SetSelected(false);
            current = null;
            detailUI.Hide();
            _seleckedBlock = null;
            SelectedBlockID = -1;
            return;
        }

        // 기존 선택 OFF
        if (current != null)
            current.SetSelected(false);

        // 새로운 선택 ON
        current = slot;
        current.SetSelected(true);
        detailUI.Show(slot.R_Block);
        _seleckedBlock = slot.R_Block.BaseData;
    }

    public void SwitchPanel(int panelIndex)
    {
        if (_currentPanel != null)
            _currentPanel.SetActive(false);
        _currentPanel = _panels[panelIndex];
        _currentPanel.SetActive(true);

        // 변경사항 반영
        OnKeywordChanged?.Invoke(UserGameData.Owned_Keywords);
        OnDeckChanged?.Invoke(UserGameData.Deck_Block_IDs);
    }

    /// <summary>
    /// 키워드 획득 시도
    /// </summary>
    public bool TryPickUp_Keyword(int keywordId, int amount)
    {
        var owned = UserGameData.Owned_Keywords.Find(k => k.Owned_KeywordID == keywordId);

        if (owned == null || owned.Keyword_Num < amount)
            return false;

        owned.Keyword_Num -= amount;

        // 임시 버퍼에 보유 개수 저장
        Number_buffer = owned.Keyword_Num;

        // 이벤트 호출 → UI에게 업데이트 요청
        //OnKeywordChanged?.Invoke(UserGameData.Owned_Keywords);
        return true;
    }

    public void Return_Keyword(int keywordId, int amount)
    {
        var owned = UserGameData.Owned_Keywords.Find(k => k.Owned_KeywordID == keywordId);
        if (owned == null)
            return;
        owned.Keyword_Num += amount;
        // 임시 버퍼 초기화
        Number_buffer = 0;
        // 이벤트 호출 → UI에게 업데이트 요청
        OnKeywordChanged?.Invoke(UserGameData.Owned_Keywords);
    }

    public bool TrySet_Keyword(int blockId, int keywordId)
    {
        var owned = UserGameData.Owned_Keywords.Find(k => k.Owned_KeywordID == keywordId);
        var block = UserGameData.Unlocked_Blocks.Find(b => b.Owner_blockID == blockId);

        if (block == null || block.Attached_Keyword_IDs.Count > 2)
        {
            // 키워드 장착 실패
            owned.Keyword_Num += 1;
            // 키워드 UI만 업데이트 요청
            OnKeywordChanged?.Invoke(UserGameData.Owned_Keywords);
            return false;
        }

        // 키워드 장착
        block.Attached_Keyword_IDs.Add(keywordId);

        // 임시 버퍼 초기화
        Number_buffer = 0;

        // 이벤트 호출 → UI에게 업데이트 요청
        OnKeywordChanged?.Invoke(UserGameData.Owned_Keywords);
        OnBlockDetailChanged?.Invoke();
        OnDeckChanged?.Invoke(UserGameData.Deck_Block_IDs);
        return true;
    }

    /// <summary>
    /// 블록에서 키워드 제거 시도
    /// </summary>
    public bool TryRemove_Keyword(int keywordId)
    {
        if (_seleckedBlock == null)
            return false;

        var owned_keyword = UserGameData.Owned_Keywords.Find(k => k.Owned_KeywordID == keywordId);
        var target_block = UserGameData.Unlocked_Blocks.Find(b => b.Owner_blockID == _seleckedBlock.blockID);

        // 타겟 블럭에 키워드가 없으면 실패
        if (target_block == null || !target_block.Attached_Keyword_IDs.Contains(keywordId))
            return false;

        // 키워드 제거
        target_block.Attached_Keyword_IDs.Remove(keywordId);
        owned_keyword.Keyword_Num += 1;

        // 이벤트 호출 → UI에게 업데이트 요청
        OnKeywordChanged?.Invoke(UserGameData.Owned_Keywords);
        OnBlockDetailChanged?.Invoke();
        OnDeckChanged?.Invoke(UserGameData.Deck_Block_IDs);
        return true;
    }
}

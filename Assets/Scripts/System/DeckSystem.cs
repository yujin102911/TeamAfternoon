using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 전투 중 덱 관리 시스템
/// 덱 초기화, 셔플, 카드 드로우 등을 담당
/// </summary>
public class DeckSystem
{
    private readonly DataRepository _repo;
    private readonly UserGameData _userData;

    // 전투 중 사용할 덱 (초기 상태)
    private readonly List<RuntimeBlock> _drawPile = new List<RuntimeBlock>();
    // 현재 손 패
    private readonly List<RuntimeBlock> _hand = new List<RuntimeBlock>();
    // 사용한 카드
    private readonly List<RuntimeBlock> _discardPile = new List<RuntimeBlock>();


    // UI 업데이트용 이벤트
    public event Action OnDeckChanged;
    public event Action OnHandChanged;

    // 덱시스템 생성자 public
    public DeckSystem(DataRepository repo, UserGameData userData)
    {
        _repo = repo;
        _userData = userData;
    }

    // 프로퍼티
    public IReadOnlyList<RuntimeBlock> Hand => _hand;
    public int DrawPileCount => _drawPile.Count;
    public int DiscardPileCount => _discardPile.Count;


    /// <summary>
    /// UserData 기반으로 덱 초기화하는 함수
    /// </summary>
    public void InitializeDeck() 
    {
        _drawPile.Clear();
        _hand.Clear();
        _discardPile.Clear();

        foreach (int blockId in _userData.Deck_Block_IDs)
        {
            RuntimeBlock runtimeBlock = CreateRuntimeBlock(blockId);
            if (runtimeBlock != null)
            {
                _drawPile.Add(runtimeBlock);
            }
        }
        ShuffleDeck();
        Debug.Log($"[DeckSystem] 덱 초기화 완료:{_drawPile.Count}장");
        OnDeckChanged?.Invoke();
    }

    public void InitializeDeck(List<RuntimeBlock> hand)
    {
        _drawPile.Clear();
        _hand.Clear();
        _discardPile.Clear();

        if (hand == null)
            return;

        _drawPile.AddRange(hand);

        Debug.Log($"[DeckSystem] 덱 동기화 완료:{_drawPile.Count}장");
        OnDeckChanged?.Invoke();
    }

    /// <summary>
    /// 카드를 count장 뽑는 함수
    /// </summary>
    /// <param name="count"></param>
    public void DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (_drawPile.Count == 0)
            {
                if (_discardPile.Count == 0)
                {
                    Debug.Log("[DeckSystem] 더 이상 뽑을 카드가 없습니다.");
                    break;
                }
                ReshuffleDiscardPile();
            }
            RuntimeBlock drawnBlock = _drawPile[0];
            _drawPile.RemoveAt(0);
            _hand.Add(drawnBlock);

            Debug.Log($"[DeckSystem] 카드 드로우: {drawnBlock.BaseData.BlockName}");
        }
        OnHandChanged?.Invoke();
        OnDeckChanged?.Invoke();
    }

    /// <summary>
    /// 손패의 카드를 사용하는 함수
    /// </summary>
    public bool UseCard(RuntimeBlock runtimeBlock)
    {
        if (!_hand.Contains(runtimeBlock))
        {
            Debug.LogWarning("[DeckSystem] 손패에 없는 카드를 사용하려 했습니다.");
            return false;
        }
        _hand.Remove(runtimeBlock);
        _discardPile.Add(runtimeBlock);

        Debug.Log($"[DeckSystem] 카드 사용: {runtimeBlock.BaseData.BlockName}");

        OnHandChanged?.Invoke();
        OnDeckChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// 배치된 카드를 손패로 되돌리는 함수
    /// </summary>
    public void ReturnCardToHand(RuntimeBlock runtimeBlock)
    {
        if (_discardPile.Contains(runtimeBlock))
        {
            _discardPile.Remove(runtimeBlock);
            _hand.Add(runtimeBlock);

            Debug.Log($"[DeckSystem] 카드 회수: {runtimeBlock.BaseData.blockName}");

            OnHandChanged?.Invoke();
            OnDeckChanged?.Invoke();
        }
    }


    public void DiscardHand()
    {
        _discardPile.AddRange( _hand );
        _hand.Clear();

        Debug.Log("[DeckSystem] 손패를 모두 버렸습니다");

        OnHandChanged?.Invoke();
        OnDeckChanged?.Invoke();
    }

    /// <summary>
    /// 엔간하면 그럴 일 없을 것 같긴한데 혹시 슬더스처럼 덱에 소멸 혹은 어지러움 추가 이런거 있을 수도 있으니까 만들어두는 함수
    /// </summary>
    public void AddBlockToDeck(int blockId) { }
    public void RemoveBlockFormDeck(int blockId) { } 

    #region private Methods
    /// <summary>
    /// 덱 셔플 함수 (내부용)
    /// </summary>
    private void ShuffleDeck()
    {
        System.Random rng = new System.Random();
        int n = _drawPile.Count;

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            RuntimeBlock temp = _drawPile[k];
            _drawPile[k] = _drawPile[n];
            _drawPile[n] = temp;
        }
        Debug.Log("[DeckSystem] 덱 셔플 완료");
    }

    /// <summary>
    /// 버림 더미를 덱으로 돌리고 다시 섞기
    /// </summary>
    private void ReshuffleDiscardPile()
    {
        Debug.Log($"[DeckSystem] 버림 더미 {_discardPile.Count}장을 덱으로 재활용");

        _drawPile.AddRange(_discardPile);
        _discardPile.Clear();
        ShuffleDeck();
    }

    /// <summary>
    /// 원본에 강화 붙이기. RuntimeBlock을 생성합니다. (Factory)
    /// </summary> 
    public RuntimeBlock CreateRuntimeBlock(int blockId)
    {
        // 블럭 데이터베이스에 이 아이디 가진 블럭 없으면 아주 곤란해
        if (!_repo.blockDatas.TryGetValue(blockId, out BlockData blockData))
        {
            Debug.LogError($"[DeckSystem] Block Id {blockId} not found");
            return null;
        }
        // RuntimeBlock 생성
        RuntimeBlock runtimeBlock = new RuntimeBlock(blockData);
        // _userData에 저장된 강화 데이터 적용
        Saved_BlockData saved = _userData.Unlocked_Blocks.Find(b => b.Owner_blockID == blockId);
        if (saved != null)
        {
            runtimeBlock.ApplySavedData(saved, _repo);
        }
        return runtimeBlock;
    }
    #endregion

    #region Utility
    /// <summary>
    /// 디버그용 덱 상태 출력 함수
    /// </summary>
    public void PrintDeckStatus()
    {
        Debug.Log($"=== 덱 상태 ===");
        Debug.Log($"뽑을 카드: {_drawPile.Count}장");
        Debug.Log($"손패: {_hand.Count}장");
        Debug.Log($"버림 더미: {_discardPile.Count}장");
        Debug.Log($"총합: {_drawPile.Count + _hand.Count + _discardPile.Count}장");
    }
    #endregion


}

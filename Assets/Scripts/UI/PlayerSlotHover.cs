using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 플레이어 슬롯 마우스 오버 핸들러 (배치된 카드 정보)
/// </summary>
public class PlayerSlotHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
    IBeginDragHandler, IDragHandler
{
    public int tick;
    public TimelineUI timelineUI;
    public PlacedBlock placedBlock;
    // 잔상인지 아닌지
    public bool Is_prev = false;

    private Canvas canvas;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    private void Update_UIUX(bool is_on)
    {
        if (timelineUI == null || placedBlock == null) return;

        int startIndex = placedBlock.startTick;
        int endIndex = startIndex + placedBlock.linkedRuntimeBlock.BaseData.blockLength - 1;

        if (is_on)
        {
            timelineUI.ShowPlayerCardTooltip(tick, placedBlock, transform.position, Is_prev);
        }
        else
        {
            timelineUI.HidePlayerTooltip();
        }

        timelineUI.HighlightSlots(startIndex, endIndex, is_on);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
        {
            Update_UIUX(true);
            timelineUI.Hover_EffectUI(tick, true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Update_UIUX(false);
        timelineUI.Hover_EffectUI(tick, false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right) return;
        if (GameManager.Instance.IsExecutingRound) return;
        if (Is_prev || placedBlock == null) return;

        EventBus.Publish(new DragBeginEvent
        {
            pointer = eventData,
            block = placedBlock.linkedRuntimeBlock,
            startWorldPos = transform.position,
            canvas = canvas
        });

        Update_UIUX(false);

        if (TimelineManager.Instance != null)
        {
            // PlacedBlock 리스트에서만 제거
            TimelineManager.Instance.RemovePlacedBlock_OnTimeline(placedBlock);

        }

        //들기 사운드
        if (SoundManager.Instance != null)
            SoundManager.Instance.Play(SoundID.Clip_PickUp);
    }

    public void OnDrag(PointerEventData eventData)
    {
        return;
        //throw new System.NotImplementedException();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 놓여진 카드가 없으면 조작 불가 && 잔상이어도 조작 불가
        if (placedBlock == null || Is_prev) return;

        if (GameManager.Instance != null && GameManager.Instance.IsExecutingRound)
        {
            Debug.Log("전투 실행 중에는 카드를 수정할 수 없습니다");
            return;
        } 

        // 카드 제거
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Update_UIUX(false);

            if (TimelineManager.Instance != null)
            {
                TimelineManager.Instance.RemovePlacedBlock(placedBlock);
                
            }
        }
        else if(eventData.button == PointerEventData.InputButton.Left)
        {
            BlockData data = placedBlock.GetBlockData();
            int index = tick - placedBlock.startTick;

            if (data.GetEffectAt(index) == ActionType.Move)
            {
                if (TimelineManager.Instance != null)
                {
                    TimelineManager.Instance.ToggleBlockDirectionForMove(placedBlock, tick);
                    if (timelineUI != null)
                    {
                        timelineUI.ShowPlayerCardTooltip(tick, placedBlock, transform.position, Is_prev);
                    }
                }
            }
            if (data.GetEffectAt(index) == ActionType.Jump)
            {
                if (TimelineManager.Instance != null)
                {
                    TimelineManager.Instance.ToggleBlockDirectionForJump(placedBlock, tick);
                    if (timelineUI != null)
                    {
                        timelineUI.ShowPlayerCardTooltip(tick,placedBlock, transform.position, Is_prev);
                    }
                }
            }
        }



        // 이 아래는 시스템이 더 나와야 제작가능
        // 우클릭으로 카드 제거
        //if (eventData.button == PointerEventData.InputButton.Right && placedBlock != null)
        //{
        //    if (DeckManager.Instance != null)
        //    {
        //        // 놓아졌던 카드의 방향은 다시 모두 오른쪽으로 초기화
        //        placedCard.card.InitiailizeMovingDirection();
        //        // DeckManager에서 틱에서 제거
        //        DeckManager.Instance.RemovePlacedCard(placedCard);


        //        // UI 업데이트
        //        if (UIManager.Instance != null)
        //        {
        //            UIManager.Instance.UpdateHandUI(DeckManager.Instance.Hand);
        //        }

        //        if (timelineUI != null)
        //        {
        //            //timelineUI.UpdatePlayerTimeline(DeckManager.Instance.PlacedCards, DeckManager.Instance.PrevPlacedCards);
        //        }
        //    }
        //}
        //// 좌클릭으로 우.좌 변환
        //else if (eventData.button == PointerEventData.InputButton.Left && placedCard != null && !Is_prev)
        //{
        //    int relativeIndex = tick - placedCard.startTick;

        //    if (relativeIndex >= 0 && relativeIndex < placedCard.card.actionLength)
        //    {
        //        EffectType effect = placedCard.card.GetEffectAt(relativeIndex);
        //        if (effect == EffectType.Move)
        //        {
        //            placedCard.card.ToggleMovingDirection(relativeIndex);
        //            Debug.Log($"틱 {tick} (카드 인덱스{relativeIndex}) 방향 전환됨");
        //            if (timelineUI != null && DeckManager.Instance != null)
        //            {
        //                timelineUI.UpdatePlayerTimeline(DeckManager.Instance.PlacedCards, DeckManager.Instance.PrevPlacedCards);
        //            }
        //        }
        //    }
        //}
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class Effect_DropZone : TimelineDropZone, IPointerClickHandler
{
    public override void OnDrop(PointerEventData eventData)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsExecutingRound) return;

        // 특수효과 전용 기능
        if(eventData.pointerDrag != null
            && eventData.pointerDrag.GetComponent<Additional_effect_UI>() != null)
        {
            var additional_effect_UI = eventData.pointerDrag.GetComponent<Additional_effect_UI>();

            // 추가 효과 타임라인에 드랍
            bool set_effect = TimelineManager.Instance.TryPlaceEffect(additional_effect_UI.Additional_Effect, tickIndex);
        }
        else
        {
            base.OnDrop(eventData);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsExecutingRound)
        {
            Debug.Log("전투 실행 중에는 카드를 수정할 수 없습니다");
            return;
        }

        // 카드 제거
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (TimelineManager.Instance != null)
            {
                if (!TimelineManager.Instance.CanPlaceEffect(tickIndex))
                {
                    TimelineManager.Instance.RemovePlacedEffect_Index(tickIndex);
                }
                else
                {
                    return;
                }
                

            }
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsExecutingRound) return;

        if (eventData.pointerDrag != null
            && eventData.pointerDrag.GetComponent<Additional_effect_UI>() != null)
        {
            var additional_effect_UI = eventData.pointerDrag.GetComponent<Additional_effect_UI>();

            if (additional_effect_UI != null && TimelineManager.Instance != null)
            {
                originalColor = image.color;

                // 배치 가능한지 확인
                bool canPlace = false;
                bool is_effect = TimelineManager.Instance.CanPlaceEffect(tickIndex);
                bool is_memory = false;

                int currentMemory = TimelineManager.Instance._currentMemory + additional_effect_UI.Additional_Effect.cost;
                int maxMemory = TimelineManager.Instance.Max_memory;

                if (currentMemory > maxMemory)
                {
                    is_memory = false;
                }
                else
                {
                    is_memory = true;
                }

                if (is_effect && is_memory)
                {
                    canPlace = true;
                }
                else
                {
                    canPlace = false;
                }

                if (image) image.color = canPlace ? hoverGreenColor : hoverRedColor;
            }
        }
        else
        {
            base.OnPointerEnter(eventData);
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null
            && eventData.pointerDrag.GetComponent<Additional_effect_UI>() != null)
        {
            // 추가 효과 전용 기능
            if (image != null)
            {
                image.color = originalColor;
            }
        }
        else
        {
            base.OnPointerExit(eventData);
        }
    }
}

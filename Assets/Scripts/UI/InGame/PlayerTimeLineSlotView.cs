using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class PlayerTimeLineSlotView : MonoBehaviour, ITimelineSlotView
{
    [SerializeField] Action_cell cell;
    [SerializeField] CanvasGroup _canvasGroup;

    public void Clear()
    {
        cell.Clear();
        _canvasGroup.alpha = 1f;

        var hover = GetComponent<PlayerSlotHover>();
        if (hover != null)
        {
            hover.placedBlock = null;
            hover.Is_prev = false;
        }  
    }

    public void SetAction(ActionType action, MoveDirection dir, int damage, bool isPreview, bool isPrev)
    {
        cell.Update_CellVisual(action, dir, damage);

        if (isPrev) 
        {
            _canvasGroup.alpha = 0.4f;
        }
    }

    public void SetHoverData(TimelineUI timeline, int tick, PlacedBlock block, bool isPrev)
    {
        var hover = GetComponent<PlayerSlotHover>();
        if (hover == null) hover = gameObject.AddComponent<PlayerSlotHover>();

        hover.timelineUI = timeline;
        hover.tick = tick;
        hover.placedBlock = block;
        hover.Is_prev = isPrev;
    }

}

using UnityEngine;

public class PlayerTimeLineSlotView : MonoBehaviour, ITimelineSlotView
{
    [SerializeField] Action_cell cell;
    [SerializeField] CanvasGroup _canvasGroup;

    public void Clear()
    {
        cell.Clear();
        _canvasGroup.alpha = 1f;
    }

    public void SetAction(ActionType action, MoveDirection dir, int damage, bool isPreview, bool isPrev)
    {
        cell.Update_CellVisual(action, dir, damage);

        if (isPrev) 
        {
            _canvasGroup.alpha = 0.4f;
        }
    }

    public void SetHoverData(int tick, PlacedBlock block, bool isPrev)
    {
        var hover = GetComponent<PlayerSlotHover>();
        if (hover == null) hover = gameObject.AddComponent<PlayerSlotHover>();

        hover.tick = tick;
        hover.placedBlock = block;
        hover.Is_prev = isPrev;
    }

}

using UnityEngine;

public interface ITimelineSlotView
{
    void Clear(); // 빈 슬롯 상태
    void SetAction(
        ActionType action,
        MoveDirection dir,
        int damage,
        bool isPreview,
        bool isPrev
    );

    void SetHoverData(TimelineUI timeline, int tick, PlacedBlock block, bool isPrev);
}

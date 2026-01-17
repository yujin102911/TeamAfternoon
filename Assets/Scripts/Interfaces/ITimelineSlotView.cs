using UnityEngine;

public interface ITimelineSlotView
{
    void Clear(); // 빈 슬롯 상태
    void SetAction(
        ActionType action,
        MoveDirection dir,
        int damage,
        Cell_Pos cell_Pos,
        bool isPreview,
        bool isPrev,
        Additional_Effect additional_Effect = null
    );

    void SetHoverData(TimelineUI timeline, int tick, PlacedBlock block, bool isPrev);
}

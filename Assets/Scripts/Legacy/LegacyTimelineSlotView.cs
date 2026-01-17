using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class LegacyTimelineSlotView : MonoBehaviour, ITimelineSlotView
{
    [SerializeField] Image root;
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI text;

    [Header("타임라인 슬롯 스프라이트")]
    public Sprite Set_SlotSprite;         // 장착 시 슬롯 스프라이트
    public Sprite Empty_SlotSprite;       // 빈 슬롯 스프라이트

    [Header("플레이어 색상 설정")]
    [Header("none 색상")]
    public Color occupiedColor = new Color(0.3f, 0.3f, 0.3f);
    [TabGroup("Attack")]
    public Color Player_attackColor;
    [TabGroup("Attack")]
    public Sprite Sword_icon;
    [TabGroup("Move")]
    public Color Player_moveColor;
    [TabGroup("Move")]
    public Sprite Front_icon;
    [TabGroup("Move")]
    public Sprite Back_icon;
    [TabGroup("Move")]
    public Sprite Left_icon;
    [TabGroup("Move")]
    public Sprite Right_icon;


    [TabGroup("Cure")]
    [SerializeField]
    private Color[] _cureColors;
    [TabGroup("Cure")]
    public Sprite[] _cureIcons;

    public void Clear()
    {
        root.color = Color.white;
        root.sprite = Empty_SlotSprite;
        text.text = "";
        icon.enabled = false;
        icon.sprite = null;
    }

    public void SetAction(
        ActionType action,
        MoveDirection dir,
        int damage,
        Cell_Pos cellPos,
        bool isPreview,
        bool isPrev,
        Additional_Effect additional_Effect
    )
    {
        root.sprite = Set_SlotSprite;

        Color color = occupiedColor;

        switch (action)
        {
            case ActionType.None:
                text.text = "-";
                break;

            case ActionType.Attack:
                text.text = "▲";
                color = Player_attackColor;
                icon.sprite = Sword_icon;
                break;

            case ActionType.Cure:
                text.text = "";
                color = _cureColors[damage];
                icon.sprite = _cureIcons[damage];
                break;

            case ActionType.Move:
                color = Player_moveColor;

                switch (dir)
                {
                    case MoveDirection.Front: text.text = "R"; icon.sprite = Front_icon; break;
                    case MoveDirection.Right: text.text = "D"; icon.sprite = Right_icon; break;
                    case MoveDirection.Back: text.text = "L"; icon.sprite = Back_icon; break;
                    case MoveDirection.Left: text.text = "U"; icon.sprite = Left_icon; break;
                }
                break;
        }

        if (isPrev)
        {
            var c = color;
            c.a = 0.25f;
            root.color = c;
        }
        else
        {
            root.color = color;
        }

        icon.enabled = true;
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

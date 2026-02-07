using UnityEngine;
using UnityEngine.EventSystems;

public class MinesweeperPointerUpCatcher : MonoBehaviour, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private MinesweeperGridView gridView;

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        gridView.CancelPress();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        gridView.CancelPress();
    }
}

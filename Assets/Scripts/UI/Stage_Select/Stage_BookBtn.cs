using UnityEngine;
using UnityEngine.EventSystems;

public class Stage_BookBtn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int Stage_ID = 0;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (StageScene_UIManager.Instance != null)
            StageScene_UIManager.Instance.BookHoverEnter(Stage_ID);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (StageScene_UIManager.Instance != null)
            StageScene_UIManager.Instance.BookHoverExit();
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class Stage_BookBtn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (StageScene_UIManager.Instance != null)
            StageScene_UIManager.Instance.Light_On();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (StageScene_UIManager.Instance != null)
            StageScene_UIManager.Instance.Light_Off();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}

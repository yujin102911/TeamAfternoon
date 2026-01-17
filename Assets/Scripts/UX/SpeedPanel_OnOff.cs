using UnityEngine;
using UnityEngine.EventSystems;

public class SpeedPanel_OnOff : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private GameObject _desc;

    private void Start()
    {
        if(_desc != null)
            _desc.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _desc.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _desc.SetActive(false);
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Enemy_slot : MonoBehaviour
{
    [SerializeField]
    private Image _slot;
    [SerializeField]
    private Image _innerSlot;
    [SerializeField]
    private TextMeshProUGUI _slotText;

    public void Show(Color color, string message)
    {
        var out_color = _slot.color;
        out_color.a = 1f;
        _slot.color = out_color;

        var inner_color = _innerSlot.color;
        inner_color.a = 1f;
        _innerSlot.color = inner_color;

        _slot.color = color;

        _slotText.text = message;
    }

    public void Hide()
    {
        var color = _slot.color;
        color.a = 0f;
        _slot.color = color;

        var inner_color = _innerSlot.color;
        inner_color.a = 0f;
        _innerSlot.color = inner_color;

        _slotText.text = "";
    }
}

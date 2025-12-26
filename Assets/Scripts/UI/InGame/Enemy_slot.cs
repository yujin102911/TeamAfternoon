using NUnit.Framework;
using System.Collections.Generic;
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

    public virtual void Show(Color color, string message, List<int> sectors = null)
    {
        Show_Slot();

        var inner_color = _innerSlot.color;
        inner_color.a = 1f;
        _innerSlot.color = inner_color;

        _slot.color = color;

        _slotText.text = message;
    }

    public virtual void Hide()
    {
        Hide_Slot();

        var inner_color = _innerSlot.color;
        inner_color.a = 0f;
        _innerSlot.color = inner_color;

        _slotText.text = "";
    }

    public void Show_Slot()
    {
        var out_color = _slot.color;
        out_color.a = 1f;
        _slot.color = out_color;
    }

    public void Hide_Slot() 
    {
        var color = _slot.color;
        color.a = 0f;
        _slot.color = color;
    }
}

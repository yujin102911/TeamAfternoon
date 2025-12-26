using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class sector_EnemySlot : Enemy_slot
{
    [SerializeField]
    private Sprite _nomalSector;
    [SerializeField]
    private Sprite _hitSector;
    [SerializeField]
    private Image[] _imageSectors;

    public override void Show(Color color, string message, List<int> sectors)
    {
        Show_Slot();

        for (int i = 0; i < _imageSectors.Length; i++) 
        {
            var view_color = _imageSectors[i].color;
            view_color.a = 1.0f;

            if (sectors != null)
            {
                if (sectors.Contains(i + 1))
                {
                    _imageSectors[i].sprite = _hitSector;
                }
                else
                {
                    _imageSectors[i].sprite = _nomalSector;
                }
            }

            _imageSectors[i].color = view_color;
        }
    }

    public override void Hide()
    {
        Hide_Slot();

        for (int i = 0; i < _imageSectors.Length; i++)
        {
            var color = _imageSectors[i].color;
            color.a = 0f;
            _imageSectors[i].sprite = _nomalSector;
            _imageSectors[i].color = color;
        }
    }
}

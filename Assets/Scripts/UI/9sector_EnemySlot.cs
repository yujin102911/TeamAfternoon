using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Special_Pattern
{
    None,
    Stone
}

public class sector_EnemySlot : Enemy_slot
{
    [Header("일반 공격")]
    [SerializeField]
    private GameObject _attackIcon;
    [SerializeField]
    private Sprite _nomalSector;
    [SerializeField]
    private Sprite _hitSector;
    [SerializeField]
    private Sprite _stoneSector;

    [Header("3*3 그리드")]
    [SerializeField]
    private Image[] _imageSectors;

    [Header("특수 패턴")]
    [SerializeField]
    private GameObject _patternIcon;
    [SerializeField]
    private Sprite _stoneIcon;

    public override void Show(Color color, string message, Special_Pattern pattern, List<int> sectors)
    {
        Show_Slot();

        _attackIcon.SetActive(false);
        _patternIcon.SetActive(false);

        switch (pattern)
        {
            case Special_Pattern.None:
                _attackIcon.SetActive(true);

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

                        //돌위치 확인
                        if (GameManager.Instance.BattleSystem.IsSectorBlocked(i+1)) {
                            _imageSectors[i].sprite = _stoneSector;
                        }
                    }

                    _imageSectors[i].color = view_color;
                }

                break;
            
            case Special_Pattern.Stone:
                _patternIcon.SetActive(true);
                _patternIcon.GetComponent<Image>().sprite = _stoneIcon;
                break;
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

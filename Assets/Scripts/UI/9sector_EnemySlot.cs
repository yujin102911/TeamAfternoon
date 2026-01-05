using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Special_Pattern
{
    None,
    Stone,
    Wind,
    Dash
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
    [SerializeField]
    private Sprite _dashRightSector;
    [SerializeField]
    private Sprite _dashLeftSector;

    [Header("3*3 그리드")]
    [SerializeField]
    private Image[] _imageSectors;

    [Header("특수 패턴")]
    [SerializeField]
    private GameObject _patternIcon;
    [SerializeField]
    private Sprite _stoneIcon;
    [SerializeField]
    private Sprite _windRightIcon;
    [SerializeField]
    private Sprite _windLeftIcon;

    public override void Show(Color color, string message, Special_Pattern pattern, List<int> sectors, bool is_left)
    {
        Show_Slot();

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

            case Special_Pattern.Wind:
                //바람 패턴 구현 예정
                _patternIcon.SetActive(true);
                _patternIcon.GetComponent<Image>().sprite = is_left ? _windLeftIcon : _windRightIcon;
                break;
            case Special_Pattern.Dash:
                //대시 패턴 구현 예정
                _attackIcon.SetActive(true);

                for (int i = 0; i < _imageSectors.Length; i++)
                {
                    var view_color = _imageSectors[i].color;
                    view_color.a = 1.0f;

                    if (sectors != null)
                    {
                        if (sectors.Contains(i + 1))
                        {
                            _imageSectors[i].sprite = is_left ? _dashLeftSector : _dashRightSector;
                        }
                        else
                        {
                            _imageSectors[i].sprite = _nomalSector;
                        }

                        //돌위치 확인
                        if (GameManager.Instance.BattleSystem.IsSectorBlocked(i + 1))
                        {
                            _imageSectors[i].sprite = _stoneSector;
                        }
                    }

                    _imageSectors[i].color = view_color;
                }
                break;
        }


    }

    public override void Hide()
    {
        Hide_Slot();

        _attackIcon.SetActive(false);
        _patternIcon.SetActive(false);

        for (int i = 0; i < _imageSectors.Length; i++)
        {
            var color = _imageSectors[i].color;
            color.a = 0f;
            _imageSectors[i].sprite = _nomalSector;
            _imageSectors[i].color = color;
        }
    }
}

using NUnit.Framework;
using Sirenix.OdinInspector;
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
    [TabGroup("Back")]
    [SerializeField]
    private Image _backImage;
    [TabGroup("Back")]
    [SerializeField]
    private Sprite _nomalBack;
    [TabGroup("Back")]
    [SerializeField]
    private Sprite _hardBack;

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
    private Image[] _nomalSectors;

    [Header("4*3 그리드")]
    [SerializeField]
    private Image[] _bigSectors;

    [Header("특수 패턴")]
    [SerializeField]
    private GameObject _patternIcon;
    [SerializeField]
    private Sprite _stoneIcon;
    [SerializeField]
    private Sprite _windRightIcon;
    [SerializeField]
    private Sprite _windLeftIcon;

    private Image[] _imageSectors;

    public override void Show(int tick, bool is_normal, Color color, string message, Special_Pattern pattern, List<int> sectors, bool is_left)
    {
        Show_Slot();

        if (GameManager.Instance != null && GameManager.Instance.UserGameData.Difficulty == Difficulty.Hard)
        {
            _backImage.sprite = _hardBack;
        }
        else
        {
            _backImage.sprite= _nomalBack;
        }

        if (is_normal)
        {
            _imageSectors = _nomalSectors;
        }
        else
        {
            _imageSectors = _bigSectors;
        }

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
                //변경되는 위치에따른 방향 계산
                if (TimelineManager.Instance != null)
                {
                    for (int i = tick - 1; i >= 1; i--)
                    {
                        if (TimelineManager.Instance.enemyPattern.GetDashAt(i) != null)
                        {
                            is_left = !is_left;
                        }
                    }
                }

                _patternIcon.SetActive(true);
                _patternIcon.GetComponent<Image>().sprite = is_left ? _windLeftIcon : _windRightIcon;
                break;
            case Special_Pattern.Dash:
                //대시 패턴 구현 예정
                _attackIcon.SetActive(true);

                //변경되는 위치에따른 방향 계산
                if (TimelineManager.Instance != null)
                {
                    for (int i = tick - 1; i >= 1; i--)
                    {
                        if (TimelineManager.Instance.enemyPattern.GetDashAt(i) != null)
                        {
                            is_left = !is_left;
                        }
                    }
                }

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

        foreach(var sector in _nomalSectors)
        {
            var color = sector.color;
            color.a = 0f;
            sector.sprite = _nomalSector;
            sector.color = color;
        }


        foreach (var sector in _bigSectors)
        {
            var color = sector.color;
            color.a = 0f;
            sector.sprite = _nomalSector;
            sector.color = color;
        }
    }
}

using Sirenix.OdinInspector;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class Melee_Tooltip : MonoBehaviour
{
    [Header("3*3 그리드")]
    [SerializeField]
    private GameObject _normalGroup;
    [SerializeField]
    private Image[] _nomalSectors;

    [Header("4*3 그리드")]
    [SerializeField]
    private GameObject _bigGroup;
    [SerializeField]
    private Image[] _bigSectors;

    [TabGroup("Sprite")]
    [SerializeField]
    private Sprite _nomalSector;
    [TabGroup("Sprite")]
    [SerializeField]
    private Sprite _successSector;

    //실질적 스프라이트 변경을 위한 배열
    private Image[] _imageSectors;

    public void show(ActionType action, int tick)
    {
        int colum = 1;

        if (GameManager.Instance.CurrentStageData.MapSize == MapSize.Grid_4x3)
        {
            _imageSectors = _bigSectors;
            _bigGroup.SetActive(true);
            colum = 4;
        }
        else
        {
            _imageSectors = _nomalSectors;
            _normalGroup.SetActive(true);
            colum = 3;
        }

        bool is_left = false;

        if (TimelineManager.Instance != null)
        {
            is_left = TimelineManager.Instance.enemyPattern.Get_Is_left();

            for (int i = tick-1; i >= 1; i--)
            {
                if(TimelineManager.Instance.enemyPattern.GetDashAt(i) != null)
                {
                    is_left = !is_left;
                    break;
                }
            }
        }
            

        

        switch (action)
        {
            case ActionType.Attack:
            case ActionType.Sword_start:

                for (int i = 0; i < _imageSectors.Length; i++)
                {
                    if (is_left)
                    {
                        if(i%colum == 0)
                        {
                            _imageSectors[i].sprite = _successSector;
                        }
                        else
                        {
                            _imageSectors[i].sprite = _nomalSector;
                        }

                    }
                    else
                    {
                        if (i % colum == colum - 1)
                        {
                            _imageSectors[i].sprite = _successSector;
                        }
                        else
                        {
                            _imageSectors[i].sprite = _nomalSector;
                        }
                    }
                }

                gameObject.SetActive(true);
                break;

            case ActionType.Bow_single:
            case ActionType.Bow_start:

                for(int i = 0; i < _imageSectors.Length; i++)
                {
                    _imageSectors[i].sprite = _successSector;
                }

                gameObject.SetActive(true);
                break;

            default:
                Hide();
                break;
        }
    }

    public void Hide()
    {
        _bigGroup.SetActive(false);
        _normalGroup.SetActive(false);

        gameObject.SetActive(false);
    }
}

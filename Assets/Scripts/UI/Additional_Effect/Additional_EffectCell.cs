using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Additional_EffectCell : MonoBehaviour
{
    [SerializeField]
    private Image _back;
    [SerializeField]
    private Image _icon;

    [TabGroup("Dash")]
    [SerializeField]
    private Sprite _dashBack;
    [TabGroup("Dash")]
    [SerializeField] 
    private Sprite _dubleDashIcon;

    [TabGroup("Attack")]
    [SerializeField]
    private Sprite _damageBack;
    [TabGroup("Attack")]
    [SerializeField]
    private Sprite _damageUpIcon;

    [TabGroup("Critical")]
    [SerializeField]
    private Sprite _criticBack;
    [TabGroup("Critical")]
    [SerializeField]
    private Sprite _criticIcon;

    [TabGroup("Sturn")]
    [SerializeField]
    private Sprite _sturnBack;
    [TabGroup("Sturn")]
    [SerializeField]
    private Sprite _sturnIcon;

    [TabGroup("Critical_3")]
    [SerializeField]
    private Sprite _Critical_3Back;
    [TabGroup("Critical_3")]
    [SerializeField]
    private Sprite _Critical_3Icon;

    [TabGroup("HealAll")]
    [SerializeField]
    private Sprite _HealAllBack;
    [TabGroup("HealAll")]
    [SerializeField]
    private Sprite _HealAllIcon;

    public void Update_CellVisual(Additional_Effect additional_Effect)
    {
        switch (additional_Effect.effectType)
        {
            case EffectType.None:
                Clear_CellVisual();
                break;
            case EffectType.Critical:
                _back.sprite = _criticBack;
                _icon.sprite = _criticIcon;
                break;
            case EffectType.Duble_Dash:
                _back.sprite = _dashBack;
                _icon.sprite = _dubleDashIcon;
                break;
            case EffectType.Damage_Up:
                _back.sprite = _damageBack;
                _icon.sprite = _damageUpIcon;
                break;
            case EffectType.Sturn:
                _back.sprite = _sturnBack;
                _icon.sprite = _sturnIcon;
                break;
            case EffectType.Critical_3:
                _back.sprite = _Critical_3Back;
                _icon.sprite = _Critical_3Icon;
                break;
            case EffectType.HealAll:
                _back.sprite = _HealAllBack;
                _icon.sprite = _HealAllIcon;
                break;
        }
    }

    public void Clear_CellVisual()
    {
        
    }

}

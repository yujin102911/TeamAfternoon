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

    public void Update_CellVisual(Additional_Effect additional_Effect)
    {
        switch (additional_Effect.effectType)
        {
            case EffectType.None:
                Clear_CellVisual();
                break;
            case EffectType.Critical:
                //Image.color = additional_Effect.effectColor;
                //txt.text = "crit";
                break;
            case EffectType.Duble_Dash:
                _back.sprite = _dashBack;
                _icon.sprite = _dubleDashIcon;
                break;
            case EffectType.Damage_Up:
                _back.sprite = _damageBack;
                _icon.sprite = _damageUpIcon;
                break;
        }
    }

    public void Clear_CellVisual()
    {
        
    }

}

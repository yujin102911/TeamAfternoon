using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class Effect_Line : MonoBehaviour
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

    public void Show(Additional_Effect additional_Effect)
    {
        this.gameObject.SetActive(true);

        switch (additional_Effect.effectType)
        {
            case EffectType.None:
                Hide();
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

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}

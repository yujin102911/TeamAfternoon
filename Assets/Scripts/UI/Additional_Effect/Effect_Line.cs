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

    public void Show(Additional_Effect additional_Effect)
    {
        this.gameObject.SetActive(true);

        switch (additional_Effect.effectType)
        {
            case EffectType.None:
                Hide();
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
        }
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Hover(bool is_enter)
    {

        if(is_enter)
        {
            _back.color = new Color(0.9f, 0.9f, 0.9f, 1.0f);
        }
        else
        {
            _back.color = Color.white;
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Additional_EffectCell : MonoBehaviour
{
    [SerializeField]
    private Image Image;
    [SerializeField]
    private TextMeshProUGUI txt;

    public void Update_CellVisual(Additional_Effect additional_Effect)
    {
        switch (additional_Effect.effectType)
        {
            case EffectType.None:
                Clear_CellVisual();
                break;
            case EffectType.Critical:
                Image.color = additional_Effect.effectColor;
                txt.text = "crit";
                break;
            case EffectType.Duble_Dash:
                Image.color = additional_Effect.effectColor;
                txt.text = "Duble_Dash";
                break;
            case EffectType.Damage_Up:
                Image.color = additional_Effect.effectColor;
                txt.text = "단데증";
                break;
        }
    }

    public void Clear_CellVisual()
    {
        Image.color = Color.white;
        txt.text = string.Empty;
    }

}

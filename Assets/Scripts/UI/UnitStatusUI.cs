using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 뭐가됐든 HP 표시하는 UI
/// </summary>
public class UnitStatusUI : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private TextMeshProUGUI _nameText;

    public void Init(string name, int currentHP, int maxHP)
    {
        if (_nameText != null) _nameText.text = name;
        UpdateHP(currentHP, maxHP);
    }

    public void UpdateHP(int currentHP, int maxHP)
    {
        if (_hpSlider != null)
        {
            _hpSlider.maxValue = maxHP;
            _hpSlider.value = currentHP;
        }
        if (_hpText != null)
        {
            _hpText.text = $"{currentHP} / {maxHP}";
        }
    }

    public void UpdateCureGauage(int current, int max)
    {
        if (_hpSlider != null)
        {
            _hpSlider.maxValue = max;
            _hpSlider.value = current;
        }
        if (_hpText != null)
        {
            _hpText.text = $"{current}<color=#7e6c4b><size=30>/{max}</size></color>";
        }
    }
}

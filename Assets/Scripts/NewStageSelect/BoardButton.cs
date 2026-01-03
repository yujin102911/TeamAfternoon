using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BoardButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _buttonTitleText;
    private StageData _stageData;
    private BoardPanel _panel;

    public void Setup(StageData stage ,BoardPanel panel)
    {
        _stageData = stage;
        _panel = panel;
        _buttonTitleText.text = $"Day {stage.StageNumber}";
        GetComponent<Button>().onClick.AddListener(() => _panel.SelectDay(_stageData));
    }

}

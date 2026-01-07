using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BoardButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _buttonTitleText;

    [Header("폰트 설정")]
    [SerializeField] private TMP_FontAsset regul;
    [SerializeField] private TMP_FontAsset bold;

    private StageData _stageData;
    private BoardPanel _panel;

    public void Setup(StageData stage ,BoardPanel panel)
    {
        _stageData = stage;
        _panel = panel;
        _buttonTitleText.text = $"Day {stage.StageNumber}";

        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => _panel.SelectDay(_stageData));
    }

    public void UpdateVisual(bool isSelected)
    {
        UserGameData currentUser = ServiceLocator.Instance.CurrentUser;
        if (currentUser == null) return;

        bool isRead = currentUser.IsBoardRead(_stageData.StageNumber);

        _buttonTitleText.font = isSelected ? bold : regul;
    }

    public int GetStageNumber()
    {
        return _stageData.StageNumber;
    }

}

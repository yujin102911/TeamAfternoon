using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private Image _readingIcon;
    [SerializeField] private Sprite _readingSprite;
    [SerializeField] private Sprite _unreadingSprite;

    private StageData _data;
    private System.Action<StageData> _onSelect;

    public void Setup(StageData data, System.Action<StageData> onSelect)
    {
        _data = data;
        _onSelect = onSelect;

        UpdateVisual();

        _titleText.text = $"{data.StageName}";
        GetComponent<Button>().onClick.AddListener(() => {
            _data.IsRead = true;
            UpdateVisual();
            _onSelect?.Invoke(_data);
        });
    }

    public void UpdateVisual()
    {
        if (_data == null) return;

        if (_readingIcon != null)
        {
            _readingIcon.sprite = _data.IsRead ? _readingSprite : _unreadingSprite;
        }
        if (_data.IsRead)
        {
            _titleText.fontStyle = FontStyles.Normal;
        }
        else
        {
            _titleText.fontStyle = FontStyles.Bold;
        }
        _titleText.text = $"{_data.StageName}";

    }

}

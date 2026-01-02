using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private Image _readingIcon;
    [SerializeField] private Sprite _readingSprite;
    [SerializeField] private Sprite _unreadingSprite;

    private StageData _stageData;
    private MailContent _mailData;
    private System.Action<StageData, MailContent> _onSelect;

    public void Setup(StageData stage, MailContent mail, System.Action<StageData, MailContent> onSelect)
    {
        _stageData = stage;
        _mailData = mail;
        _onSelect = onSelect;

        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();

        btn.onClick.AddListener(() => {
            _mailData.isRead = true;
            UpdateVisual();
            _onSelect?.Invoke(_stageData, _mailData);
        });
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (_mailData == null) return;

        if (_readingIcon != null)
        {
            _readingIcon.sprite = _mailData.isRead ? _readingSprite : _unreadingSprite;
        }
        if (_mailData.isRead)
        {
            _titleText.fontStyle = FontStyles.Normal;
        }
        else
        {
            _titleText.fontStyle = FontStyles.Bold;
        }
        _titleText.text = $"{_mailData.subject}";

    }

}

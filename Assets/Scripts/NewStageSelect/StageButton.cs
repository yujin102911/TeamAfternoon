using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class StageButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private Image _readingIcon;
    [SerializeField] private Sprite _readingSprite;
    [SerializeField] private Sprite _unreadingSprite;

    [Header("폰트 연결")]
    [SerializeField] private TMP_FontAsset regul;
    [SerializeField] private TMP_FontAsset bold;

    private StageData _stageData;
    private MailContent _mailData;
    private int _mailIndex;
    private System.Action<StageData, int, MailContent> _onSelect;

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }
    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(UnityEngine.Localization.Locale locale)
    {
        UpdateVisual();
    }

    public void Setup(StageData stage, int mailIndex, MailContent mail, System.Action<StageData, int, MailContent> onSelect)
    {
        _stageData = stage;
        _mailIndex = mailIndex;
        _mailData = mail;
        _onSelect = onSelect;

        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();

        btn.onClick.AddListener(() => {
            ServiceLocator.Instance.CurrentUser.SetMailRead(_stageData.StageNumber, _mailIndex);
            UpdateVisual();
            _onSelect?.Invoke(_stageData, _mailIndex, _mailData);
        });
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (_mailData == null || ServiceLocator.Instance.CurrentUser == null) return;

        bool isRead = ServiceLocator.Instance.CurrentUser.IsMailRead(_stageData.StageNumber, _mailIndex);

        if (_readingIcon != null)
        {
            _readingIcon.sprite = isRead ? _readingSprite : _unreadingSprite;
        }
        if (_titleText != null)
        {
            _titleText.font = isRead ? regul : bold;
            _titleText.text = _mailData.subject.GetLocalizedString();
        }

    }

}

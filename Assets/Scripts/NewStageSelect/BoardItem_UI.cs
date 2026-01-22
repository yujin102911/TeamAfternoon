using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class BoardItem_UI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _contextText;

    [SerializeField]
    private TMP_FontAsset EN_KR;
    [SerializeField]
    private TMP_FontAsset Ja_Zhan;

    private BoardEntry _currentEntry;

    public void Setup(BoardEntry entry)
    {
        UnsubscribeEvents();

        Font_SetUp(LocalizationSettings.SelectedLocale);

        _currentEntry = entry;

        if (_currentEntry.title != null)
        {
            _currentEntry.title.StringChanged += UpdateTitle;
            _currentEntry.title.RefreshString();
        }

        if (_currentEntry.content != null)
        {
            _currentEntry.content.StringChanged += UpdateContent;
            _currentEntry.content.RefreshString();
        }
    }

    private void UpdateTitle(string value) => _titleText.text = value;
    private void UpdateContent(string value) => _contextText.text = value;

    private void OnDestroy() => UnsubscribeEvents();

    private void UnsubscribeEvents()
    {
        if (_currentEntry == null) return;
        if (_currentEntry.title != null) _currentEntry.title.StringChanged -= UpdateTitle;
        if (_currentEntry.content != null) _currentEntry.content.StringChanged -= UpdateContent;
    }

    private void Font_SetUp(Locale locale)
    {
        if (locale == null) return;

        string code = locale.Identifier.Code;

        if(code == "ja" || code == "zh-Hans")
        {
            _titleText.font = Ja_Zhan;
            _contextText.font = Ja_Zhan;
        }
        else
        {
            _titleText.font = EN_KR;
            _contextText.font = EN_KR;
        }
    }
}

using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BoardItem_UI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _contextText;

    private BoardEntry _currentEntry;

    public void Setup(BoardEntry entry)
    {
        UnsubscribeEvents();

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

}

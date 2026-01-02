using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BoardItem_UI : MonoBehaviour
{
    [SerializeField] private Image _thumbnailImage;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _contextText;

    public void Setup(BoardEntry entry)
    {
        if (_thumbnailImage != null) _thumbnailImage.sprite = entry.illustration;
        if (_titleText != null) _titleText.text = entry.title;
        if (_contextText != null) _contextText.text = entry.content;
    }

}

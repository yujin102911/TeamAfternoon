using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class Icon_cell : MonoBehaviour
{
    [SerializeField]
    private Image _iconImage;
    [SerializeField]
    private TextMeshProUGUI nameText;
    [SerializeField]
    private LocalizedString _nameText;

    private void Awake()
    {
        _nameText.StringChanged += OnNameChanged;
    }

    private void OnDestroy()
    {
        _nameText.StringChanged -= OnNameChanged;
    }

    private void OnNameChanged(string value) => nameText.text = value;

    public void Update_CellVisual(Jungle_Game jungle_Game)
    {
        _iconImage.sprite = jungle_Game.Icon;
        _nameText.TableEntryReference = jungle_Game.TitleKey;
        _nameText.RefreshString();
    }
}

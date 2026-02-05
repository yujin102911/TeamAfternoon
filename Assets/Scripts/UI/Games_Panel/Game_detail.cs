using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class Game_detail : MonoBehaviour
{
    [SerializeField]
    private Image _capsule;

    [SerializeField]
    private TextMeshProUGUI nameText;
    [SerializeField]
    private LocalizedString _nameText;

    [SerializeField]
    private TextMeshProUGUI descText;
    [SerializeField]
    private LocalizedString _descText;

    private void Awake()
    {
        _nameText.StringChanged += OnNameChanged;
        _descText.StringChanged += OnDescChanged;
    }

    private void OnDestroy()
    {
        _nameText.StringChanged -= OnNameChanged;
        _descText.StringChanged -= OnDescChanged;
    }

    private void OnNameChanged(string value) => nameText.text = value;
    private void OnDescChanged(string value) => descText.text = value;

    public void Update_DetailVisual(Jungle_Game jungle_Game)
    {
        _capsule.sprite = jungle_Game.Capsule;
        _nameText.TableEntryReference = jungle_Game.TitleKey;
        _nameText.RefreshString();
        _descText.TableEntryReference = jungle_Game.DescKey;
        _descText.RefreshString();

        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}

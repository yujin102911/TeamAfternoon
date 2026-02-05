using UnityEngine;

[CreateAssetMenu(fileName = "New Jungle_Game", menuName = "Data/Jungle_Game Data")]
public class Jungle_Game : ScriptableObject
{
    [SerializeField]
    private string _titleKey;
    [SerializeField]
    private string _descKey;
    [SerializeField]
    private Sprite _capsule;
    [SerializeField]
    private Sprite _icon;
    [SerializeField]
    private string _storeURL;

    public string TitleKey => _titleKey;
    public string DescKey => _descKey;
    public Sprite Capsule => _capsule;
    public Sprite Icon => _icon;
    public string StoreURL => _storeURL;
}

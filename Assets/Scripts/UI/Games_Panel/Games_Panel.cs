using UnityEngine;
using UnityEngine.UI;

public class Games_Panel : MonoBehaviour
{
    [SerializeField]
    private Jungle_Game[] _jungle_Games;
    private Jungle_Game _selectedGame = null;

    [SerializeField]
    private GameObject _iconPrefab;
    [SerializeField]
    private Transform _iconsParent;

    [SerializeField]
    private Game_detail _gameDetail;
    [SerializeField]
    private Button _storeButton;

    private void Awake()
    {
        for(int i = 0; i < _jungle_Games.Length; i++)
        {
            int index = i; // Capture the current value of i

            var iconObj = Instantiate(_iconPrefab, _iconsParent);
            Icon_cell gameIcon = iconObj.GetComponent<Icon_cell>();
            Button iconButton = iconObj.GetComponent<Button>();

            gameIcon.Update_CellVisual(_jungle_Games[i]);
            iconButton.onClick.AddListener(() => SelectGame(index));
        }

        _gameDetail.Hide();
        _storeButton.onClick.AddListener(OnClickStore);
        _storeButton.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _storeButton.onClick.RemoveListener(OnClickStore);
    }

    private void OnDisable()
    {
        _gameDetail.Hide();
        _storeButton.gameObject.SetActive(false);
    }

    private void SelectGame(int index)
    {
        _selectedGame = _jungle_Games[index];
        _gameDetail.Update_DetailVisual(_selectedGame);
        _storeButton.gameObject.SetActive(true);
    }

    private void OnClickStore()
    {
        if (_selectedGame == null) return;
        if (string.IsNullOrWhiteSpace(_selectedGame.StoreURL)) return;

        SteamManager.OpenOverlayWebPage(_selectedGame.StoreURL);
    }
}

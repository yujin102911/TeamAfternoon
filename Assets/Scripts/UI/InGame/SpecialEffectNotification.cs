using UnityEngine;
using UnityEngine.UI;

public class SpecialEffectNotification : MonoBehaviour
{
    [SerializeField] private GameObject _visualRoot;
    [SerializeField] private Button _closeButton;

    private void Awake()
    {
        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(Hide);
        }
        if (_visualRoot != null) _visualRoot.SetActive(false);
    }

    public void Show()
    {
        if (_visualRoot != null)
        {
            _visualRoot.SetActive(true);
        }
    }

    private void Hide()
    {
        if (_visualRoot != null)
        {
            _visualRoot.SetActive(false);
        }
    }
    

}

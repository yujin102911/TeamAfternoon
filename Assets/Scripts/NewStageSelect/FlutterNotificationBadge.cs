using UnityEngine;

public class FlutterNotificationBadge : MonoBehaviour
{
    [SerializeField] private GameObject _notificationIcon;

    private bool _hasChecked = false;

    private void OnEnable()
    {
        TwitterPanel.OnPanelOpened += HandlePanelOpened;
        UpdateBadgeState();
    }

    private void OnDisable()
    {
        TwitterPanel.OnPanelOpened -= HandlePanelOpened;
    }

    private void HandlePanelOpened()
    {
        _hasChecked = true;
        UpdateBadgeState();
    }

    private void UpdateBadgeState()
    {
        if (_notificationIcon != null)
        {
            _notificationIcon.SetActive(!_hasChecked);
        }
    }
}

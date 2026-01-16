using UnityEngine;

public class BoardNotificationBadge : MonoBehaviour
{
    [SerializeField] private GameObject _notificationIcon;

    private void OnEnable()
    {
        BoardPanel.OnBoardStatusChanged += RefreshBadge;
        RefreshBadge();
    }
    private void OnDisable()
    {
        BoardPanel.OnBoardStatusChanged -= RefreshBadge;
    }

    private void RefreshBadge()
    {
        if (_notificationIcon != null)
        {
            bool hasUnread = ServiceLocator.Instance.CurrentRepository.HasUnreadBoard();
            _notificationIcon.SetActive(hasUnread);
        }
    }

}

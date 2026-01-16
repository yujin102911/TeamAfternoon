using UnityEngine;

public class MailNotificationBadge : MonoBehaviour
{
    [SerializeField] private GameObject _notificationIcon;

    private void OnEnable()
    {
        MailPanel.OnMailStatusChanged += RefreshBadge;
        RefreshBadge();
    }

    private void OnDisable()
    {
        MailPanel.OnMailStatusChanged -= RefreshBadge;
    }

    private void RefreshBadge()
    {
        if (_notificationIcon != null)
        {
            bool hasUnread = ServiceLocator.Instance.CurrentRepository.HasUnreadMail();
            _notificationIcon.SetActive(hasUnread);
        }
    }

}

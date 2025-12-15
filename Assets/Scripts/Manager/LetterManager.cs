using UnityEngine;

public class LetterManager : MonoBehaviour
{
    [SerializeField] private GameObject bookButton;
    [SerializeField] private GameObject notification;
    [SerializeField] private GameObject letterPanel;
    [SerializeField] private GameObject letterButton;

    public void PressLetterButton()
    {
        letterPanel.SetActive(true);
        notification.SetActive(false);
        letterButton.SetActive(false);
    }

    public void PressXButton()
    {
        letterPanel.SetActive(false);
        bookButton.SetActive(true);
        letterButton.SetActive(true);
    }

}

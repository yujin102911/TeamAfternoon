using UnityEngine;
using UnityEngine.UI;

public class NextButton : MonoBehaviour
{
    [SerializeField] private Button _nextButton;
    [SerializeField] private GameObject _currentPage;
    [SerializeField] private GameObject _nextPage;

    private void Awake()
    {
        if (_nextButton != null)
        {
            _nextButton.onClick.AddListener(ClickButton);
        }
    }

    private void ClickButton()
    {
        _currentPage.SetActive(false);
        _nextPage.SetActive(true);
    }

}

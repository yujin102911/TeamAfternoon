using UnityEngine.UI;
using UnityEngine;

public class AgreeButton : MonoBehaviour
{
    [Header("UI참조")]
    [SerializeField] private Toggle _yesToggle;
    [SerializeField] private Button _nextButton;

    private void Awake()
    {
        _yesToggle.onValueChanged.AddListener(HandleToggleChanged);
    }
    private void Start()
    {
        _nextButton.interactable = _yesToggle.isOn;
    }

    private void HandleToggleChanged(bool isOn)
    {
        _nextButton.interactable = isOn;
    }

}

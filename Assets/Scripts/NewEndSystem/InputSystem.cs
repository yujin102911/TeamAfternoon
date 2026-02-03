using UnityEngine;
using TMPro;

public class InputSystem : MonoBehaviour
{
    public TMP_InputField mainInputField;
    public TextMeshProUGUI displayResultText;
    public TextMeshProUGUI countText;

    [SerializeField] private int maxLimit = 100;

    private void Start()
    {
        mainInputField.onValueChanged.AddListener(UpdateUI);
        UpdateUI(mainInputField.text);
    }

    void UpdateUI(string input)
    {
        if (displayResultText != null)
        {
            displayResultText.text = input;
        }

        if (countText != null)
        {
            int limit = mainInputField.characterLimit;
            countText.text = $"{input.Length} / {limit}";
        }
    }

}

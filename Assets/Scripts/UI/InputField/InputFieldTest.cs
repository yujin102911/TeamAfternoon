using UnityEngine;
using TMPro;

public class InputFieldTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI outputText;

    private void Awake()
    {
        // 엔터 입력 콜백 등록
        inputField.onSubmit.AddListener(OnSubmit);
    }

    private void OnDestroy()
    {
        inputField.onSubmit.RemoveListener(OnSubmit);
    }

    private void OnSubmit(string value)
    {
        // 빈 문자열이면 무시
        if (string.IsNullOrWhiteSpace(value))
            return;

        // 출력
        outputText.text = value;

        // 입력창 초기화
        inputField.text = string.Empty;

        // 다시 포커스 주기 (연속 입력용)
        inputField.ActivateInputField();
    }
}

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Message에 있는 온갖 버튼들을 관리하는 매니저
/// </summary>
public class MessagePanel : MonoBehaviour
{
    [SerializeField] private Button _closeButton;

    private void Awake()
    {
        _closeButton.onClick.AddListener(ClosePanel);
    }

    private void ClosePanel()
    {
        Debug.Log("닫기 버튼이 눌렸습니다.");
        gameObject.SetActive(false);
    }

}

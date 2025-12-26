using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : MonoBehaviour
{
    // 게임 설정 패널에 관련된 기능 여따가 넣으면 될듯
    [SerializeField] private Button _xButton;

    private void Awake()
    {
        _xButton.onClick.AddListener(CloseSetting);
    }
    private void CloseSetting()
    {
        gameObject.SetActive(false);
    }
}

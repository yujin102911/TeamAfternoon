using UnityEngine;
using UnityEngine.UI;

public class LogoutPanel : MonoBehaviour
{
    [SerializeField] private Button _xBtn; // 오른쪽 위 X 버튼
    [SerializeField] private Button _noBtn; // 아니요
    [SerializeField] private Button _yesBtn; // 네

    [SerializeField] private string _titleSceneName = "";

    private void Awake()
    {
        _xBtn.onClick.AddListener(TurnOffPanel);
        _noBtn.onClick.AddListener(TurnOffPanel);

        _yesBtn.onClick.AddListener(ChangeScene);
    }

    public void TurnOffPanel()
    {
        gameObject.SetActive(false);
    }

    public void ChangeScene()
    {
        Debug.Log("저장");
        SaveService.Save(ServiceLocator.Instance.CurrentUser);
        TwitSaveService.Save(ServiceLocator.Instance.CurrentTwitData);
        ServiceLocator.Instance.Scene.Load(_titleSceneName);
    }

}

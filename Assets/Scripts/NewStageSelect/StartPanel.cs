using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class StartPanel : MonoBehaviour
{
    [Header("패널 연결")]
    [SerializeField] private GameObject _startPanelObject;
    [SerializeField] private GameObject _quitPanelObject;
    [SerializeField] private GameObject _creditPanelObject;
    [SerializeField] private GameObject _settingPanelObject;

    [Header("위치 설정")]
    [SerializeField] private Vector2 _fixedPosition = Vector2.zero;

    [Header("버튼 연결")]
    [SerializeField] private Button _blockerButton;
    [SerializeField] private Button _startButton;
    [Header("추가 버튼 연결")]
    [SerializeField] private Button _quitButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _creditButton;
    [SerializeField] private TextMeshProUGUI _timeText;

    private void Awake()
    {
        _blockerButton.onClick.AddListener(CloseMenu);
        _startButton.onClick.AddListener(OpenMenu);

        _quitButton.onClick.AddListener(OpenQuit);
        _settingsButton.onClick.AddListener(OpenSettings);
        _creditButton.onClick.AddListener(OpenCredit);
        CloseMenu();
    }
    private void Update()
    {
        if (_timeText != null)
        {
            _timeText.text = DateTime.Now.ToString("hh:mm tt");
        }
    }

    private void ResetPanelPosition(GameObject panel)
    {
        if (panel != null)
        {
            RectTransform rt = panel.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = _fixedPosition;
            }
        }
    }

    public void OpenMenu()
    {
        _startPanelObject.SetActive(true);
    }
    public void CloseMenu()
    {
        _startPanelObject.SetActive(false);
    }
    // 추후 자식 기능들도 여기에 추가하심 될듯용
    public void OpenQuit()
    {
        ResetPanelPosition(_quitPanelObject);
        _quitPanelObject.SetActive(true);
        CloseMenu ();
    }
    public void OpenSettings()
    {
        _settingPanelObject.SetActive(true);
        CloseMenu () ;
    }
    public void OpenCredit()
    {
        _creditPanelObject.SetActive(true);
        CloseMenu ();
    }


}

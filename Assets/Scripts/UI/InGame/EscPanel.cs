using UnityEngine;
using UnityEngine.UI;

public class EscPanel : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject _escPanel;

    [Header("버튼")]
    [SerializeField] private Button _goTitleButton;
    [SerializeField] private Button _goBackButton;
    [SerializeField] private Button _xButton;

    [Header("타이틀 씬 이름")]
    [SerializeField] private string _titleScene = "TitleScene";

    private float savedTimeScale = 1.0f;


    private void Awake()
    {
        _goTitleButton.onClick.AddListener(GoToTitle);
        _goBackButton.onClick.AddListener(TurnOffEscPanel);
        _xButton.onClick.AddListener(TurnOffEscPanel);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_escPanel != null)
            {
                if (_escPanel.gameObject.activeInHierarchy)
                    TurnOffEscPanel();
                else
                    TurnOnEscPanel();
            }
        }
    }

    private void TurnOnEscPanel()
    {
        _escPanel.SetActive(true);
        if (ServiceLocator.Instance != null && ServiceLocator.Instance.Cursor != null)
        {
            ServiceLocator.Instance.Cursor.SetCursorConfined(false);
        }
        if (Time.timeScale > 0)
        {
            savedTimeScale = Time.timeScale;
            Time.timeScale = 0;
        }
    }

    private void TurnOffEscPanel()
    {
        _escPanel.SetActive(false);
        if (ServiceLocator.Instance != null && ServiceLocator.Instance.Cursor != null)
        {
            ServiceLocator.Instance.Cursor.SetCursorConfined(true);
        }
        Time.timeScale = savedTimeScale;
    }

    private void GoToTitle()
    {
        ServiceLocator.Instance.Scene.Load(_titleScene);
    }

}

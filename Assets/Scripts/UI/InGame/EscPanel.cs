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

    [Header("씬 이름")]
    [SerializeField] private string _easyScene = "EasyStageScene";
    [SerializeField] private string _hardScene = "HardStageScene";

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
        if (Time.timeScale > 0)
        {
            savedTimeScale = Time.timeScale;
            Time.timeScale = 0;

            if (SoundManager.Instance != null)
                SoundManager.Instance.Play(SoundID.UI_Popup);
        }
    }

    private void TurnOffEscPanel()
    {
        _escPanel.SetActive(false);
        Time.timeScale = savedTimeScale;
    }

    private void GoToTitle()
    {
        Time.timeScale = 1.0f;
        if (ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Easy)
        {
            ServiceLocator.Instance.Scene.Load(_easyScene);
        }
        else if (ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Hard)
        {
            ServiceLocator.Instance.Scene.Load(_hardScene);
        }
    }

}

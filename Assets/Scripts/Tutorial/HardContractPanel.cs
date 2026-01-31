using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HardContractPanel : MonoBehaviour
{
    [Header("씬 이름 설정")]
    [SerializeField] private string _hardMainSceneName = "HardMainScene";

    [Header("UI 참조")]
    [SerializeField] private IntroPanel _introPanel;
    [SerializeField] private Button _finishButton;

    private bool _isSceneLoading = false;

    private void Awake()
    {
        _finishButton.onClick.AddListener(OnFinishButtonClick);
    }

    public void OnFinishButtonClick()
    {
        _finishButton.interactable = false;
        ServiceLocator.Instance.SaveNowUserData();
        ServiceLocator.Instance.SetTutorialClear(true);
        StartCoroutine(End());

    }

    private IEnumerator End()
    {
        if (_isSceneLoading) yield break;
        _isSceneLoading = true;

        AsyncOperation asyncLoad = null;

        if (ServiceLocator.Instance != null && ServiceLocator.Instance.Scene != null)
        {
            asyncLoad = ServiceLocator.Instance.Scene.LoadAsync(_hardMainSceneName);
        }
        if (asyncLoad != null)
        {
            asyncLoad.allowSceneActivation = false;
        }
        yield return new WaitForSecondsRealtime(0.5f);
        _introPanel.StartDayChange(0, 1);
        yield return _introPanel.FadeOutRoutine();
        if (asyncLoad != null)
        {
            asyncLoad.allowSceneActivation = true;
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(_hardMainSceneName);
        }
    }

}

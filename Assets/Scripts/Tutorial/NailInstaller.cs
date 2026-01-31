using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NailInstaller : MonoBehaviour
{
    [SerializeField] private Button _p1Btn;
    [SerializeField] private GameObject _p1;
    [SerializeField] private GameObject _p2;
    [SerializeField] private GameObject _p3;
    [SerializeField] private Button _endBtn;
    [SerializeField] private string _tutoSelectScene = "TutorialDesktop";
    [SerializeField] private IntroPanel _introPanel;
    [SerializeField] private Image _progressBar;
    [SerializeField] private float _downloadDuration = 2.0f;

    private bool _isSecneLoading = false;

    private void Awake()
    {
        _p1Btn.onClick.AddListener(Install);
        _endBtn.onClick.AddListener(GoTuto);
    }

    private void OnEnable()
    {
        transform.SetAsLastSibling();
    }

    private void Install()
    {
        _p1.SetActive(false);
        _p2.SetActive(true);
        StartCoroutine(DownloadRoutine());
    }

    private IEnumerator DownloadRoutine()
    {
        ServiceLocator.Instance.Cursor.StartAnimation("Loading");
        if (_progressBar != null)
        {
            _progressBar.fillAmount = 0;

            float elapsed = 0f;
            while (elapsed < _downloadDuration)
            {
                elapsed += Time.deltaTime;
                _progressBar.fillAmount = Mathf.Clamp01(elapsed / _downloadDuration);
                yield return null;
            }
            _progressBar.fillAmount = 1f;
        }
        ServiceLocator.Instance.Cursor.StopAnimation();
        yield return new WaitForSeconds(0.5f);
        _p2.SetActive(false);
        _p3.SetActive(true);
    }

    private void GoTuto()
    {
        _endBtn.interactable = false;
        StartCoroutine(EndButton());
    }

    // end버튼 클릭 시 나오는 연출 IEnumerator
    private IEnumerator EndButton()
    {
        if (_isSecneLoading) yield break;

        _isSecneLoading = true;

        AsyncOperation asyncLoad = null;
        if (ServiceLocator.Instance != null &&  ServiceLocator.Instance.Scene != null)
        {
            asyncLoad = ServiceLocator.Instance.Scene.LoadAsync(_tutoSelectScene);
        }
        if (asyncLoad != null)
        {
            asyncLoad.allowSceneActivation = false;
        }
        else
        {
            Debug.LogError($"[NailInstaller] 씬 '{_tutoSelectScene}'을 찾을 수 없거나 로드에 실패했습니다. Build Settings를 확인하세요.");
        }

        yield return new WaitForSeconds(0.5f);
        _introPanel.StartDayChange(0, 1);
        yield return _introPanel.FadeOutRoutine();

        if (asyncLoad != null)
        {
            asyncLoad.allowSceneActivation = true;
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(_tutoSelectScene);
        }

    }

}

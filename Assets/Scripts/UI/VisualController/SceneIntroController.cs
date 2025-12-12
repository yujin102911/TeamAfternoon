using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
///  씬 시작시 타닥타닥 연출
/// </summary>
public class SceneIntroController : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private GameObject _introPanel;
    [SerializeField] private TextMeshProUGUI _introText;
    [SerializeField] private CanvasGroup _panelCanvasGroup;

    [Header("연출 설정")]
    [TextArea(3, 10)]
    [SerializeField] private string _message = "옛날 옛적에~~";
    [SerializeField] private float _typingSpeed = 0.05f;
    [SerializeField] private float _startDelay = 0.5f;
    [SerializeField] private float _endDelay = 1f;
    [SerializeField] private float _fadeDuration = 1f;

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentStageData  != null)
        {
            _message = GameManager.Instance.CurrentStageData.IntroMessage;
        }
        StartCoroutine(PlayIntroSequence());
    }

    public void SetMessage(string message)
    {
        _message = message;
    }

    private IEnumerator PlayIntroSequence()
    {
        _introPanel.SetActive(true);
        _panelCanvasGroup.alpha = 1;
        _panelCanvasGroup.blocksRaycasts = true;
        _introText.text = "";

        yield return new WaitForSeconds(_startDelay);

        foreach(char letter in _message.ToCharArray())
        {
            _introText.text += letter;
            // 사운드 추가 시 여기 삽입
            yield return new WaitForSeconds(_typingSpeed);
        }

        yield return new WaitForSeconds(_endDelay);

        float elapsedTime = 0f;
        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            _panelCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / _fadeDuration);
            yield return null;
        }
        _panelCanvasGroup.blocksRaycasts = false;
        _introPanel.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.OnIntroCompleted();
    }

}

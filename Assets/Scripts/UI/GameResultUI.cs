using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 한 판 끝나고 뜨는 패널 관리용
/// </summary>
public class GameResultUI : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject _victoryPanel;
    [SerializeField] private GameObject _defeatPanel;

    [Header("버튼")]
    [SerializeField] private Button _goToMainBtn;
    [SerializeField] private string _librarySceneName;

    [Header("연출")]
    [SerializeField] private float _delaySeconds = 1.0f;
    [SerializeField] private float _fadeDuration = 1.0f;

    private void Start()
    {
        _victoryPanel.SetActive(false);
        _defeatPanel.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBattleEnded += HandleBattleEnded;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBattleEnded -= HandleBattleEnded;
        }
    }

    private void HandleBattleEnded(bool isVictory)
    {
        if (isVictory)
        {
            StartCoroutine(ShowPanelCoroutine(_victoryPanel));
        }
        else
        {
            StartCoroutine(ShowPanelCoroutine(_defeatPanel));
        }
    }

    private IEnumerator ShowPanelCoroutine(GameObject targetPanel)
    {
        CanvasGroup cg = targetPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = targetPanel.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        targetPanel.SetActive(true);

        Debug.Log($"[{targetPanel.name}] {_delaySeconds}초 대기 중...");
        yield return new WaitForSeconds(_delaySeconds);
        Debug.Log($"[{targetPanel.name}] 페이드 인 시작");

        float timer = 0f;
        while (timer < _fadeDuration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, timer/ _fadeDuration);
            yield return null;
        }
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    public void GoToLibrary()
    {
        Debug.Log("고투 라이브러리");
        ServiceLocator.Instance.Scene.Load(_librarySceneName);
    }

}

using System.Collections;
using TMPro;
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

    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI _victoryText;
    [SerializeField] private TextMeshProUGUI _defeatText;

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

    private void HandleBattleEnded(bool isVictory, int totalRound, int hitCount, int attackCount, int leftHP)
    {
        if (isVictory)
        {
            StartCoroutine(ShowPanelCoroutine(isVictory, totalRound, hitCount, attackCount, leftHP, _victoryPanel, _victoryText));
        }
        else
        {
            StartCoroutine(ShowPanelCoroutine(isVictory, totalRound, hitCount, attackCount, leftHP, _defeatPanel, _defeatText));
        }
    }

    private IEnumerator ShowPanelCoroutine(bool isVictory, int totalRound, int hitCount, int attackCount, int leftHP, GameObject targetPanel, TextMeshProUGUI targetText)
    {
        CanvasGroup cg = targetPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = targetPanel.AddComponent<CanvasGroup>();

        if (isVictory)
        {
            if (targetText != null)
            {
                targetText.text =
                    $"본 전투는 총 <size=72><color=#626262> {totalRound}</size></color>라운드에 걸쳐 진행되었으며,\n" +
                    $"전투 중 사역마는 <size=72><color=#626262> {hitCount}</size></color>회의 피격을 받았으며\n" +
                    $"<size=72><color=#626262>{attackCount}</size></color>회의 유효 공격을 수행하였습니다.\n" +
                    $"전투 종료 시점 기준 잔존 체력은 <size=72><color=#626262> {leftHP}</size></color>로 확인되었습니다.\n\n" +
                    $"이상으로 본 전투 성과 보고를 마치겠습니다.";

            }
        }
        else
        {
            if (targetText != null)
            {
                targetText.text = 
                    $"본 전투는 총 <size=72><color=#626262> {totalRound}</size></color>라운드에 걸쳐\n"
                    +$"진행되었으나, 전투 중 사역마는\n"
                    +$"<size=72><color=#626262>{hitCount}</size></color>회의 피격을 받았으며\n"
                    +$"<size=72><color=#626262>{attackCount}</size></color>회의 유효 공격을\n"
                    +$"수행하였습니다.\n"
                    +$"전투 종료 시점 기준,\n"
                    +$"사역마의 전투 지속은 불가능한\n"
                    +$"상태로 확인되었습니다.";

            }
        }

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

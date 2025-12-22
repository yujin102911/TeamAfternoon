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

    [Header("수정 연출")]
    [SerializeField]
    private NoiseTextureGenerator _noiseTextureGenerator;
    [SerializeField] 
    private RectTransform _crystalTransform;
    [SerializeField]
    private Vector3 _startPos;
    public float moveY = 0;
    [SerializeField]
    private float duration = 1.0f;

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

        // 수정 연출 추가

        if (isVictory)
        {
            yield return StartCoroutine(MoveUpByY_Ease(_startPos, moveY, duration));

            if (targetText != null)
            {
                targetText.text =
                    $"토토님께,\n\n" +
                    $"「오즈의 마법사」 정화 결과를 아래와 같이 보고합니다.\n\n" +
                    $"본 정화는 총 <size=48><color=#626262>{totalRound}</size></color>줄에 걸쳐 진행되었으며,\n" +
                    $"작업 중 <size=48><color=#626262> {attackCount}</size></color>회의 정화를 하였습니다.\n\n" +
                    $"책의 코어는 이제 완전히 정화되었습니다.\n\n" +
                    $"해피 엔딩을 빕니다";

            }
        }
        else
        {
            if (targetText != null)
            {
                targetText.text =
                    $"토토님께,\n\n"
                    + $"「오즈의 마법사」 정화 결과를 아래와 같이 보고합니다.\n\n"
                    + $"본 정화는 총 <size=48><color=#626262>{totalRound}</size></color>줄에 걸쳐 진행되었으며,\n"
                    + $"작업 중 <size=48><color=#626262>{attackCount}</size></color>회의 정화를 하였지만,\n"
                    + $"책의 코어에 접근하는 것은 실패하였습니다.\n"
                    + $"빠른 시일 내에 해당 책에 재조치가 있을 예정입니다.\n";

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

        if (SoundManager.Instance != null)
            SoundManager.Instance.Play(SoundID.SFX_Execute);

        ServiceLocator.Instance.Scene.Load(_librarySceneName);
    }

    IEnumerator MoveUpByY_Ease(Vector3 startPos, float moveY, float duration)
    {
        Vector3 endPos = startPos + Vector3.up * moveY;

        float elapsed = 0f;

        if (_noiseTextureGenerator != null)
            _noiseTextureGenerator.GenerateNoiseTexture();

        CardBurnEffect cardBurnEffect = _crystalTransform.GetComponent<CardBurnEffect>();

        if(!_crystalTransform.gameObject.activeSelf)
            _crystalTransform.gameObject.SetActive(true);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Ease Out
            t = 1f - Mathf.Pow(1f - t, 3f);

            _crystalTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        _crystalTransform.anchoredPosition = endPos;

        cardBurnEffect.Initialize();
        
        yield return StartCoroutine(cardBurnEffect.BurnAnimationCoroutine(null));
    }

}

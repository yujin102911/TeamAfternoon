using UnityEngine;
using System.Collections;

public class SmoothFadeAndBlink : MonoBehaviour
{
    [Header("설정")]
    public CanvasGroup targetObject;
    public int blinkCount = 3;
    public float speed = 10f;

    private bool _isProcessing = false;

    public void StartEffect()
    {
        if (targetObject != null && !_isProcessing)
        {
            StartCoroutine(SequenceRoutine());
        }
    }

    IEnumerator SequenceRoutine()
    {
        _isProcessing = true;

        // 1. 시작: Fade In
        targetObject.alpha = 0f;
        targetObject.gameObject.SetActive(true);
        yield return StartCoroutine(LerpAlpha(0.6f));

        // 2. 중간: 깜빡임
        for (int i = 0; i < blinkCount; i++)
        {
            yield return StartCoroutine(LerpAlpha(0.3f));
            yield return StartCoroutine(LerpAlpha(0.6f));
        }

        // 3. 끝: Fade Out
        yield return StartCoroutine(LerpAlpha(0f));

        targetObject.gameObject.SetActive(false);

        _isProcessing = false;
    }

    IEnumerator LerpAlpha(float targetAlpha)
    {
        while (Mathf.Abs(targetObject.alpha - targetAlpha) > 0.001f)
        {
            // Time.deltaTime 대신 Time.unscaledDeltaTime 사용
            targetObject.alpha = Mathf.Lerp(targetObject.alpha, targetAlpha, Time.unscaledDeltaTime * speed);

            // yield return null도 TimeScale이 0이어도 매 프레임 재개됩니다.
            yield return null;
        }
        targetObject.alpha = targetAlpha;
    }
}
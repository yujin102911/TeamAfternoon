using UnityEngine;
using UnityEngine.UI; // Image 컴포넌트 사용을 위해 필요

public class UIAnimation : MonoBehaviour
{
    public enum AnimMode { RelativePingPong, AbsoluteFadeLoop }

    [Header("Mode Selection")]
    public AnimMode currentMode = AnimMode.AbsoluteFadeLoop;

    [Header("Common Settings")]
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Option 1: Relative Settings")]
    public float pingPongDuration = 2.0f;
    public Vector2 relativeOffset = new Vector2(200f, 0f); // UI는 픽셀 단위이므로 값을 크게 잡으세요.

    [Header("Option 2: Absolute Settings")]
    public float moveDuration = 2.0f;
    public float fadeDuration = 1.0f;
    public Vector3 startPos = new Vector3(-200, 0, 0); // RectTransform 기준 좌표
    public Vector3 endPos = new Vector3(200, 0, 0);

    private float timer = 0f;
    private Vector3 initialOrigin;
    private RectTransform rectTransform; // UI 위치 제어용
    private Image targetImage;           // UI 컬러 제어용
    private Color originalColor;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        targetImage = GetComponent<Image>();

        initialOrigin = rectTransform.anchoredPosition; // UI는 anchoredPosition이 기준입니다.

        if (targetImage != null)
            originalColor = targetImage.color;

        if (currentMode == AnimMode.AbsoluteFadeLoop)
            rectTransform.anchoredPosition = startPos;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (currentMode == AnimMode.RelativePingPong)
        {
            HandleRelativePingPong();
        }
        else
        {
            HandleAbsoluteFadeLoop();
        }
    }

    void HandleRelativePingPong()
    {
        float pingPong = Mathf.PingPong(timer / pingPongDuration, 1f);
        float curveValue = movementCurve.Evaluate(pingPong);

        Vector3 targetOffset = new Vector3(relativeOffset.x, relativeOffset.y, 0);
        rectTransform.anchoredPosition = initialOrigin + (targetOffset * curveValue);

        SetAlpha(1f);
    }

    void HandleAbsoluteFadeLoop()
    {
        float totalDuration = moveDuration + fadeDuration;

        if (timer <= moveDuration)
        {
            float moveProgress = timer / moveDuration;
            float curveValue = movementCurve.Evaluate(moveProgress);
            rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, curveValue);
            SetAlpha(1f);
        }
        else if (timer <= totalDuration)
        {
            rectTransform.anchoredPosition = endPos;
            float fadeProgress = (timer - moveDuration) / fadeDuration;
            SetAlpha(Mathf.Lerp(1f, 0f, fadeProgress));
        }
        else
        {
            timer = 0f;
            rectTransform.anchoredPosition = startPos;
            SetAlpha(1f);
        }
    }

    void SetAlpha(float alpha)
    {
        if (targetImage != null)
        {
            Color newColor = originalColor;
            newColor.a = alpha;
            targetImage.color = newColor;
        }
    }
}
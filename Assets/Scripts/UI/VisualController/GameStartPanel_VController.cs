using System.Collections;
using UnityEngine;

public class GameStartPanel_VController : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    public void FadeOut()
    {
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        float time = 0f;
        float startAlpha = canvasGroup.alpha;

        canvasGroup.blocksRaycasts = false; // 클릭 차단
        canvasGroup.interactable = false;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(false); // 완전히 비활성화

        yield return null;

        canvasGroup.alpha = 1.0f;
    }
}

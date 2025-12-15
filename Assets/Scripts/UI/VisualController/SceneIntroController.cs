using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
///  씬 시작시 타닥타닥 연출
/// </summary>
public class SceneIntroController : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private List<CanvasGroup> _fadeLayers;

    [Header("연출 설정")]
    [SerializeField] private float _startDelay = 0.5f;
    [SerializeField] private float _fadeDuration = 1f;

    private void Start()
    {
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        foreach(CanvasGroup group in _fadeLayers)
        {
            if (group != null)
            {
                group.gameObject.SetActive(true);
                group.alpha = 1f;
                group.blocksRaycasts = true;
            }
        }
        yield return new WaitForSeconds(_startDelay);

        foreach (CanvasGroup group in _fadeLayers)
        {
            if (group == null) continue;
            float elapsedTime = 0f;
            while (elapsedTime < _fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                group.alpha = Mathf.Lerp(1f, 0f, elapsedTime / _fadeDuration);
                yield return null;
            }
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.gameObject.SetActive(false);
        }
        if (GameManager.Instance != null) 
            GameManager.Instance.OnIntroCompleted();

    }

}

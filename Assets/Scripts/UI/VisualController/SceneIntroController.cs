using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///  씬 패널 교체용 스크립트
/// </summary>
public class SceneIntroController : MonoBehaviour
{
    [System.Serializable]
    public class IntroLayer
    {
        [HorizontalGroup("Row")]
        [HideLabel]
        [Required("CanvasGroup을 넣어주세요")]
        public CanvasGroup Group;

        [HorizontalGroup("Row", Width = 150)]
        [LabelText("Duration")]
        [SuffixLabel("sec", Overlay = true)]
        public float Duration = 1.5f;
    }

    [Title("Intro Sequence")]
    [ListDrawerSettings(ShowIndexLabels = true, AddCopiesLastElement = true)] // 번호 표시, 복사 기능
    [LabelText("Fade Out Layers")]
    public List<IntroLayer> _introLayers;

    [PropertySpace(15)]
    [Title("Global Settings")]
    [LabelText("Start Delay (sec)")]
    [SerializeField] private float _startDelay = 0.5f; // 시작 전 대기 시간


    private void Start()
    {
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        foreach(var layer in _introLayers)
        {
            if (layer.Group != null)
            {
                layer.Group.gameObject.SetActive(true);
                layer.Group.alpha = 1f;
                layer.Group.blocksRaycasts = true; 
            }
        }
        yield return new WaitForSeconds(_startDelay);

        foreach (var layer in _introLayers)
        {
            if (layer.Group == null) continue;

            float duration = layer.Duration;

            if (duration > 0f)
            {
                float elapsedTime = 0f;
                while (elapsedTime < duration)
                {
                    elapsedTime += Time.deltaTime;
                    layer.Group.alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
                    yield return null;
                }
            }
            layer.Group.alpha = 0f;
            layer.Group.blocksRaycasts = false;
            layer.Group.gameObject.SetActive(false);
        }
        if (GameManager.Instance != null) 
            GameManager.Instance.OnIntroCompleted();

    }

}

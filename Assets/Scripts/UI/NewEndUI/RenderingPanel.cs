using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;

public class RenderingPanel : MonoBehaviour
{
    public event Action OnRenderingFinished;

    [Header("UI 설정")]
    [SerializeField] private List<GameObject> _progressBlocks = new List<GameObject>();

    [Header("시간 설정")]
    [SerializeField] private float _totalDuration = 3.0f;
    [SerializeField] private AnimationCurve _speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Title("테스트 컨트롤")]
    [Button("렌더링 시작 테스트", ButtonSizes.Large)]
    [InfoBox("이 버튼은 플레이 모드에서만 작동합니다")]
    public void TestStart()
    {
        if (Application.isPlaying)
        {
            StartRendering(() => Debug.Log("<color=green>렌더링 완료!</color>"));
        }
        else
        {
            Debug.LogWarning("게임이 실행 중일 때만 테스트할 수 있습니다.");
        }
    }

    public void StartRendering(Action onComplete)
    {
        gameObject.SetActive(true);
        ServiceLocator.Instance.Cursor.StartAnimation("Loading");
        StartCoroutine(AnimateProgressBar(onComplete));
    }

    private IEnumerator AnimateProgressBar(Action onComplete)
    {
        foreach (var block in _progressBlocks) block.SetActive(false);
        int totalBlocks = _progressBlocks.Count;
        for (int i = 0; i < totalBlocks; i++)
        {
            float progress = (float)i / totalBlocks;
            float t = _speedCurve.Evaluate(progress);
            float waitTime = (_totalDuration / totalBlocks) * (1.1f - t);
            yield return new WaitForSeconds(waitTime);
            _progressBlocks[i].SetActive(true);
        }
        yield return new WaitForSeconds(0.5f);

        ServiceLocator.Instance.Cursor.StopAnimation();

        onComplete?.Invoke();
        OnRenderingFinished?.Invoke();
        gameObject.SetActive(false);
    }

}

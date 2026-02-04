using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intro_Canvas : MonoBehaviour
{
    [System.Serializable]
    public class IntroLayer
    {
        [HorizontalGroup("Row")]
        [HideLabel]
        [Required("CanvasGroup을 넣어주세요")]
        public CanvasGroup Group;

        [HorizontalGroup("Row", Width = 110)]
        [LabelText("Delay")]
        [SuffixLabel("sec", Overlay = true)]
        public float Delay = 0.5f; // ✅ 각 스텝 시작 후 자동진행까지 기다리는 시간

        [HorizontalGroup("Row", Width = 130)]
        [LabelText("Fade")]
        [SuffixLabel("sec", Overlay = true)]
        public float Duration = 1.5f; // ✅ 페이드 시간

        [HorizontalGroup("Row")]
        [LabelText("SoundID")]
        public SoundID _soundID = SoundID.None;
    }

    [SerializeField]
    private GameObject _finalPanel; // 최종 패널
    [SerializeField]
    private float _finalDelay = 0.5f; // 마지막 컷 후 대기 시간

    [Title("Intro Sequence")]
    [ListDrawerSettings(ShowIndexLabels = true, AddCopiesLastElement = true)]
    [LabelText("Fade Out Layers")]
    public List<IntroLayer> _introLayers = new List<IntroLayer>();

    private Coroutine _sequenceCo;
    private bool _skipRequested = false;

    private void Start()
    {
        // 초기 세팅: 전부 켜고 alpha=1
        if(!_finalPanel.activeSelf)
            _finalPanel.SetActive(true);

        foreach (var layer in _introLayers)
        {
            if (layer.Group == null) continue;
            layer.Group.alpha = 0f;
            layer.Group.blocksRaycasts = false; // 화면 클릭을 가로채지 않게
        }

        _sequenceCo = StartCoroutine(PlaySequence());
    }

    private void Update()
    {
        // 진행 중 클릭 → 현재 스텝 스킵 요청
        if (_sequenceCo != null && Input.GetMouseButtonDown(0))
        {
            _skipRequested = true;
        }
    }

    private IEnumerator PlaySequence()
    {
        for (int i = 0; i < _introLayers.Count; i++)
        {
            var layer = _introLayers[i];
            if (layer.Group == null) continue;

            // 스킵 플래그 초기화: "이번 스텝"에 대한 스킵만 받기
            _skipRequested = false;

            // ✅ 1) Delay 동안 기다림(자동진행 구간)
            float delay = Mathf.Max(0f, layer.Delay);
            float t = 0f;
            while (t < delay)
            {
                if (_skipRequested)
                {
                    // 스텝 스킵: 즉시 완료 처리 후 다음 스텝으로
                    ForceCompleteStep(layer, true);
                    goto NEXT_STEP;
                }

                t += Time.deltaTime;
                yield return null;
            }

            // ✅ 2) FadeOut 진행(자동진행 구간)
            yield return FadeOutOrSkip(layer);

        // 스텝 끝나면 다음 스텝으로
        NEXT_STEP:
            continue;
        }

        // ===== 마지막 스텝 =====
        _skipRequested = false;

        float ft = 0f;
        while (ft < _finalDelay)
        {
            if (_skipRequested)
            {
                // ✅ 클릭하면: 정리하고 즉시 코루틴 종료
                EndIntroAndStop();
                yield break;
            }

            ft += Time.unscaledDeltaTime; // realtime 대기
            yield return null;
        }

        // ✅ 클릭 없이 시간이 끝나도 동일하게 정리
        EndIntroAndStop();
    }

    private void EndIntroAndStop()
    {
        _sequenceCo = null;

        if (_finalPanel != null)
            _finalPanel.SetActive(false);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopSFX();
            SoundManager.Instance.Play(SoundID.UI_Clear);
        }
            

        // 스킵 플래그는 굳이 true로 둘 필요 없음 (다음 루프도 없고 종료니까)
        _skipRequested = false;
    }

    private IEnumerator FadeOutOrSkip(IntroLayer layer)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopSFX();
            if (layer._soundID != SoundID.None)
            {
                SoundManager.Instance.Play(layer._soundID);
            }
        }
        var group = layer.Group;

        float duration = Mathf.Max(0f, layer.Duration);

        if (duration <= 0f)
        {
            ForceCompleteStep(layer, false);
            yield break;
        }

        float elapsed = 0f;
        float startAlpha = group.alpha;

        while (elapsed < duration)
        {
            if (_skipRequested)
            {
                ForceCompleteStep(layer, false);
                yield break;
            }

            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / duration);
            yield return null;
        }

        ForceCompleteStep(layer, false);
    }

    private void ForceCompleteStep(IntroLayer layer, bool is_sound)
    {
        if (SoundManager.Instance != null && is_sound)
        {
            SoundManager.Instance.StopSFX();
            if (layer._soundID != SoundID.None)
            {
                SoundManager.Instance.Play(layer._soundID);
            }
        }

        var group = layer.Group;
        if (group == null) return;

        group.alpha = 1f;
        group.blocksRaycasts = false;
    }
}

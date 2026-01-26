using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class StepSlider : MonoBehaviour
{
    [Header("")]
    [SerializeField]
    private TimelineUI _timelineUI;

    public Slider slider;
    public int steps = 16; // 마디 수

    private int prevStep = 0;

    private Coroutine playRoutine;

    public event System.Action<int> OnStepSelected;

    void Start()
    {
        slider.minValue = 0;
        slider.maxValue = steps - 1;
        slider.wholeNumbers = false;

        slider.onValueChanged.AddListener(OnSliderChanged);
        Debug.Log(slider.onValueChanged.GetPersistentEventCount());
    }

    private void OnEnable()
    {
        
    }

    void OnSliderChanged(float value)
    {
        if (!slider.interactable)
            return; // 연출 중이면 무시

        int step = Mathf.RoundToInt(value);
        slider.SetValueWithoutNotify(step);

        if(prevStep != step)
        {
            OnStepChanged(step);
            prevStep = step;
        }
        
    }

    void OnStepChanged(int step)
    {
        // 여기서 타임라인 이동 / 애니메이션 프리뷰 등

        _timelineUI.SetCurrent_Tick(step);
        _timelineUI.Hide_Preview();

        if (step != 0)
        {
            
            _timelineUI.Show_Preview(step);
        }
        OnStepSelected?.Invoke(step);
    }

    public void Play(float duration)
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);

        playRoutine = StartCoroutine(PlayRoutine(duration));
    }

    public Coroutine Return(float duration)
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);

        // 타임라인이 0일 때 미리보기 보이는 버그 수정
        _timelineUI.SetCurrent_Tick(0);

        playRoutine = StartCoroutine(ReturnRoutine(duration));
        return playRoutine;
    }

    public IEnumerator Return_IEnumerator(float duration)
    {
        return ReturnRoutine(duration);
    }

    private IEnumerator PlayRoutine(float duration)
    {
        slider.interactable = false; // 입력 차단
        
        _timelineUI.Hide_Preview();

        float time = 0f;
        slider.value = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;   // 0 → 1
            slider.value = Mathf.Lerp(0f, slider.maxValue, t);            // 연속적 증가
            yield return null;
        }

        slider.value = slider.maxValue; // 보정
        slider.interactable = true;
    }

    private IEnumerator ReturnRoutine(float duration)
    {
        slider.interactable = false; // 입력 차단

        _timelineUI.Hide_Preview();

        float time = 0f;
        slider.value = slider.maxValue; // 보정
        

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;   // 0 → 1
            slider.value = Mathf.Lerp(slider.maxValue, 0f, t);            // 연속적 감소
            yield return null;
        }

        slider.value = 0f;
        slider.interactable = true;
        //_timelineUI.SetActive_Slots(true);
    }

    public void Stop()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }
        slider.interactable = false;
    }
}

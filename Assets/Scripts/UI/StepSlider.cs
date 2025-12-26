using UnityEngine;
using UnityEngine.UI;

public class StepSlider : MonoBehaviour
{
    [Header("")]
    [SerializeField]
    private TimelineUI _timelineUI;

    public Slider slider;
    public int steps = 16; // 마디 수

    void Start()
    {
        slider.minValue = 0;
        slider.maxValue = steps - 1;
        slider.wholeNumbers = false;

        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnSliderChanged(float value)
    {
        int step = Mathf.RoundToInt(value);
        slider.SetValueWithoutNotify(step);

        OnStepChanged(step);

        //
        if (step != 0)
        {
            _timelineUI.OnCursorExit();
            _timelineUI.OnCursorEnter(step);
        }
            
    }

    void OnStepChanged(int step)
    {
        Debug.Log($"현재 마디: {step}");
        // 여기서 타임라인 이동 / 애니메이션 프리뷰 등
    }
}

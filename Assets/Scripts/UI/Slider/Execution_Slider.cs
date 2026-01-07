using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Execution_Slider : MonoBehaviour
{
    public Slider slider;

    [SerializeField]
    private TextMeshProUGUI _currentTime;
    [SerializeField]
    private TextMeshProUGUI _maxTime;

    private int _maxRound = 1;
    private Coroutine playRoutine;

    public void Initialize(int maxRound)
    {
        _maxRound = maxRound;
        slider.value = 0f;
        _maxTime.text = $"{maxRound:00}:00";
        UpdateText(slider.value);
    }

    // 슬라이더 진행 연출 코루틴
    public void Play(float duration)
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);

        playRoutine = StartCoroutine(PlayRoutine(duration));
    }

    private IEnumerator PlayRoutine(float duration)
    {
        float start_value = slider.value;
        float end_value = slider.value + (1.0f / _maxRound);

        float time = 0f;
        slider.value = start_value;
        UpdateText(slider.value);

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;   // 0 → 1
            slider.value = Mathf.Lerp(start_value, end_value, t);            // 연속적 증가
            UpdateText(slider.value);
            yield return null;
        }

        slider.value = end_value; // 보정
        UpdateText(slider.value);
    }

    private void UpdateText(float currentValue)
    {
        int totalSeconds = Mathf.FloorToInt(currentValue * _maxRound * 60);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        _currentTime.text = $"{minutes:00}:{seconds:00}";
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

using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class IntroPanel : MonoBehaviour
{
    [SerializeField] private GameObject _introPanel;
    [SerializeField] private TextMeshProUGUI _dayCountText;

    [Header("숫자 카운트 설정")]
    [SerializeField] private int _startDay = 0;
    [SerializeField] private int _endDay = 0;
    [SerializeField] private float _countUpDuration = 1.0f;

    [Header("페이드아웃 설정")]
    [SerializeField] private float _stayDuration = 1.5f;
    [SerializeField] private float _fadeDuration = 1.0f;

    private CanvasGroup _introCanvasGroup;

    private void Start()
    {
        if (_introPanel != null)
            _introCanvasGroup = _introPanel.GetComponent<CanvasGroup>();
        if (_introCanvasGroup != null) _introCanvasGroup.alpha = 0f;
        if (_dayCountText != null)
        {
            UserGameData currentUser = ServiceLocator.Instance?.CurrentUser;
            if (currentUser != null && ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Easy)
            {
                _dayCountText.text = $"Day {_startDay:D2}";
            }
            else if (currentUser != null && ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Hard)
            {
                _dayCountText.text = $"Week {_startDay:D2}";
            }
        }
    }
    public void StartDayChange(int startDay, int endDay)
    {
        _startDay = startDay;
        _endDay = endDay;
    }

    public IEnumerator FadeOutRoutine()
    {
        if (_introCanvasGroup != null)
        {
            _introCanvasGroup.blocksRaycasts = true;
            float elpased = 0f;
            while (elpased < _countUpDuration)
            {
                elpased += Time.unscaledDeltaTime;
                _introCanvasGroup.alpha = Mathf.Clamp01(elpased / _countUpDuration);
                yield return null;
            }
        }

        float elapsed = 0f;
        while (elapsed < _countUpDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            int displayDay = (int)Mathf.Lerp(_startDay, _endDay, elapsed / _countUpDuration);

            UserGameData currentUser = ServiceLocator.Instance?.CurrentUser;
            if (currentUser != null)
            {
                if (_dayCountText != null)
                {
                    if (ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Easy)
                    {
                        _dayCountText.text = $"Day {displayDay:D2}";
                    }
                    else if (ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Hard)
                    {
                        _dayCountText.text = $"Week {displayDay:D2}";
                    }
                }
            }
            else
            {
                _dayCountText.text = $"Day {displayDay:D2}";
            }
            yield return null;

        }

        if (_dayCountText != null)
        {
            UserGameData currentUser = ServiceLocator.Instance?.CurrentUser;

            if (currentUser != null && ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Easy)
            {
                _dayCountText.text = $"Day {_endDay:D2}";
            }
            else if (currentUser != null && ServiceLocator.Instance.CurrentUser.Difficulty == Difficulty.Hard)
            {
                _dayCountText.text = $"Week {_endDay:D2}";
            }
        }

        yield return new WaitForSecondsRealtime(_stayDuration);

    }
}

using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum Direction
{
    Left,
    Up,
    Right,
    Down
}

[RequireComponent(typeof(Image))]
public class Cover_Effect : MonoBehaviour
{
    private Image _image;
    private Coroutine _running;

    [SerializeField] private Direction _direction;
    [SerializeField] private float _duration = 0.5f;
    [SerializeField] private bool _isOpen = true;

    private void Awake()
    {
        _image = GetComponent<Image>();

        // Filled 타입 필수
        _image.type = Image.Type.Filled;
        _image.fillMethod = Image.FillMethod.Horizontal;
        _image.fillAmount = 1f;
    }

    public IEnumerator Play_Effect(Direction direction, float duration, bool is_open)
    {
        yield return Slide_Image(direction, duration, is_open);
    }


    private IEnumerator Slide_Image(Direction direction, float duration, bool is_open)
    {
        SetupFill(direction, is_open);

        float from = is_open ? 0f : 1f;
        float to = is_open ? 1f : 0f;

        // 시작값 즉시 반영
        _image.fillAmount = from;

        if (duration <= 0f)
        {
            _image.fillAmount = to;
            _running = null;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // UI 연출이면 unscaled 추천(일시정지 영향 X)
            float a = Mathf.Clamp01(t / duration);

            // 선형 보간 (원하면 Ease 적용 가능)
            _image.fillAmount = Mathf.Lerp(from, to, a);

            yield return null;
        }

        _image.fillAmount = to;
        _running = null;
    }

    /// <summary>
    /// 방향 + 열림/닫힘에 따라 fillMethod/origin을 세팅
    /// 규칙: direction은 "사라지는/드러나는 방향" 기준
    /// </summary>
    private void SetupFill(Direction direction, bool is_open)
    {
        // 열린다(0→1) / 닫힌다(1→0) 자체는 fillAmount에서 처리
        // 여기서는 "어느 방향으로 진행되는지"만 결정

        switch (direction)
        {
            case Direction.Left:
                // 왼쪽으로 진행(= 오른쪽 끝부터 변화)
                _image.fillMethod = Image.FillMethod.Horizontal;
                _image.fillOrigin = (int)Image.OriginHorizontal.Right; // 오른쪽 기준
                break;

            case Direction.Right:
                // 오른쪽으로 진행(= 왼쪽 끝부터 변화)
                _image.fillMethod = Image.FillMethod.Horizontal;
                _image.fillOrigin = (int)Image.OriginHorizontal.Left;  // 왼쪽 기준
                break;

            case Direction.Up:
                // 위로 진행(= 아래쪽부터 변화)
                _image.fillMethod = Image.FillMethod.Vertical;
                _image.fillOrigin = (int)Image.OriginVertical.Bottom;  // 아래 기준
                break;

            case Direction.Down:
                // 아래로 진행(= 위쪽부터 변화)
                _image.fillMethod = Image.FillMethod.Vertical;
                _image.fillOrigin = (int)Image.OriginVertical.Top;     // 위 기준
                break;
        }

        // 이미지가 Filled 지원하는 스프라이트인지도 중요:
        // 일반적으로 문제 없지만, 스프라이트 import 설정이 이상하면 티어링/왜곡될 수 있음.
    }
}

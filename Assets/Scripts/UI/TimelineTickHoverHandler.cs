using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 타임라인 각 틱 슬롯에 붙어 마우스 호버 시 프리뷰를 요청하는 핸들러
/// </summary>
public class TimelineTickHoverHandler : MonoBehaviour, 
    IPointerEnterHandler, IPointerExitHandler
{
    public int tickIndex;
    private TimelineUI _timelineUI;

    [SerializeField]
    private Image _outCircle;
    [SerializeField]
    private Image _inCircle;

    private void Start()
    {
        _timelineUI = GetComponentInParent<TimelineUI>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!GameManager.Instance.IsSectorSelected) return;
        if (GameManager.Instance.IsExecutingRound) return;

        Show();

        if (_timelineUI != null && GameManager.Instance.IsSectorSelected)
        {
            _timelineUI.OnCursorEnter(tickIndex);
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!GameManager.Instance.IsSectorSelected) return;
        if (GameManager.Instance.IsExecutingRound) return;

        Hide();

        if (_timelineUI != null)
            _timelineUI.OnCursorExit();
    }

    public void Show()
    {
        _outCircle.color = new Color(_outCircle.color.r, _outCircle.color.g, _outCircle.color.b, 1f);
        _inCircle.color = new Color(_inCircle.color.r, _inCircle.color.g, _inCircle.color.b, 1f);
    }

    public void Hide()
    {
        _outCircle.color = new Color(_outCircle.color.r, _outCircle.color.g, _outCircle.color.b, 0f);
        _inCircle.color = new Color(_inCircle.color.r, _inCircle.color.g, _inCircle.color.b, 0f);
    }

}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StepSliderClickJump : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Slider slider;
    [SerializeField] private RectTransform trackRect;
    [SerializeField] private int steps = 16;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!slider.interactable)
            return;

        // 클릭 위치를 Track 기준 로컬 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            trackRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPos
        );

        // Track의 좌우 기준 0~1로 정규화
        float width = trackRect.rect.width;
        float normalized = Mathf.Clamp01((localPos.x + width * 0.5f) / width);

        // step 계산
        int step = Mathf.RoundToInt(normalized * (steps - 1));

        // 값 설정
        slider.SetValueWithoutNotify(step);
        slider.onValueChanged.Invoke(step);
    }
}
